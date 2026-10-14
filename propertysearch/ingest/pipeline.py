"""Store drafts, merge duplicates, and seed labeled samples."""

from __future__ import annotations

import json
from datetime import datetime, timezone
from pathlib import Path

from app.db import connect, listing_count
from ingest.catalog import LATER_PORTALS, OUT_OF_SCOPE
from ingest.dedupe import dedupe_key, normalise_street
from ingest.filters import passes_search
from ingest.models import ListingDraft
from ingest.sources import fetch_active, read_inbox

ROOT = Path(__file__).resolve().parents[1]
SAMPLES_PATH = ROOT / "data" / "fixtures" / "samples.json"


def run_ingest(
    db_path: Path,
    *,
    use_network: bool = False,
    read_saved_pages: bool = True,
    seed_samples_if_empty: bool = True,
    force_samples: bool = False,
) -> dict:
    started = _now()
    summary: dict = {
        "started_at": started,
        "sources": [],
        "inserted": 0,
        "merged": 0,
        "filtered": 0,
        "seeded_samples": False,
        "later_portals_not_fetched": [portal["id"] for portal in LATER_PORTALS],
        "out_of_scope": [item["id"] for item in OUT_OF_SCOPE],
    }
    conn = connect(db_path)
    try:
        if use_network:
            for source_run in fetch_active():
                summary["sources"].append(_store_run(conn, source_run, summary, is_sample=False))
        if read_saved_pages:
            for source_run in read_inbox():
                summary["sources"].append(_store_run(conn, source_run, summary, is_sample=False))
        if force_samples or (seed_samples_if_empty and listing_count(conn) == 0):
            replaced = seed_samples(conn)
            summary["seeded_samples"] = True
            summary["sample_rows_loaded"] = replaced
        summary["live_listings"] = listing_count(conn, sample=0)
        summary["sample_listings"] = listing_count(conn, sample=1)
        summary["finished_at"] = _now()
        conn.execute(
            "INSERT INTO ingest_runs (started_at, finished_at, summary_json) VALUES (?, ?, ?)",
            (started, summary["finished_at"], json.dumps(summary)),
        )
        conn.commit()
    finally:
        conn.close()
    return summary


def seed_samples(conn) -> int:
    conn.execute("DELETE FROM listings WHERE is_sample = 1")
    drafts = load_sample_drafts()
    kept = 0
    for draft in drafts:
        action = upsert_draft(conn, draft, is_sample=True)
        if action in {"inserted", "merged"}:
            kept += 1
    return kept


def load_sample_drafts() -> list[ListingDraft]:
    payload = json.loads(SAMPLES_PATH.read_text(encoding="utf-8"))
    drafts: list[ListingDraft] = []
    for item in payload:
        drafts.append(
            ListingDraft(
                portal=item["portal"],
                external_id=item["external_id"],
                url=item["url"],
                price=item.get("price"),
                postcode=str(item.get("postcode") or ""),
                municipality=item.get("municipality"),
                street=item.get("street"),
                surface_m2=item.get("surface_m2"),
                bedrooms=item.get("bedrooms"),
                epc_label=item.get("epc_label"),
                construction_year=item.get("construction_year"),
                building_condition=item.get("building_condition"),
                title=item.get("title"),
                description=item.get("description"),
                latitude=item.get("latitude"),
                longitude=item.get("longitude"),
                photo_urls=list(item.get("photo_urls") or []),
                days_on_market=item.get("days_on_market"),
                property_type=item.get("property_type") or "APARTMENT",
                transaction_type=item.get("transaction_type") or "FOR_SALE",
            )
        )
    return drafts


def upsert_draft(conn, draft: ListingDraft, *, is_sample: bool) -> str:
    if not passes_search(draft):
        return "filtered"
    key = dedupe_key(draft)
    existing = conn.execute("SELECT * FROM listings WHERE dedupe_key = ?", (key,)).fetchone()
    if existing is not None and existing["is_sample"] == 0 and is_sample:
        return "filtered"
    photos = _merge_photos(
        json.loads(existing["photo_urls"]) if existing else [],
        draft.photo_urls,
    )
    if existing is None:
        cursor = conn.execute(
            """
            INSERT INTO listings (
                dedupe_key, is_sample, price, postcode, municipality, street, street_norm,
                surface_m2, bedrooms, epc_label, construction_year, building_condition,
                title, description, latitude, longitude, photo_urls
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """,
            (
                key,
                1 if is_sample else 0,
                draft.price,
                draft.postcode,
                draft.municipality,
                draft.street,
                normalise_street(draft.street),
                draft.surface_m2,
                draft.bedrooms,
                draft.epc_label,
                draft.construction_year,
                draft.building_condition,
                draft.title,
                draft.description,
                draft.latitude,
                draft.longitude,
                json.dumps(photos),
            ),
        )
        listing_id = cursor.lastrowid
        action = "inserted"
    else:
        listing_id = existing["id"]
        description = _prefer_text(existing["description"], draft.description)
        condition = existing["building_condition"] or draft.building_condition
        title = _prefer_text(existing["title"], draft.title)
        live_wins = existing["is_sample"] == 1 and not is_sample
        conn.execute(
            """
            UPDATE listings SET
                is_sample = ?,
                price = ?,
                postcode = ?,
                municipality = ?,
                street = ?,
                street_norm = ?,
                surface_m2 = ?,
                bedrooms = ?,
                epc_label = ?,
                construction_year = ?,
                building_condition = ?,
                title = ?,
                description = ?,
                latitude = COALESCE(?, latitude),
                longitude = COALESCE(?, longitude),
                photo_urls = ?,
                updated_at = CURRENT_TIMESTAMP
            WHERE id = ?
            """,
            (
                0 if live_wins or not existing["is_sample"] else 1,
                draft.price if live_wins else existing["price"],
                draft.postcode if live_wins else existing["postcode"],
                draft.municipality if live_wins else existing["municipality"],
                draft.street if live_wins else existing["street"],
                normalise_street(draft.street) if live_wins else existing["street_norm"],
                draft.surface_m2 if live_wins else existing["surface_m2"],
                draft.bedrooms if live_wins else existing["bedrooms"],
                draft.epc_label or existing["epc_label"],
                draft.construction_year or existing["construction_year"],
                condition if not live_wins else draft.building_condition,
                title if not live_wins else draft.title,
                description if not live_wins else draft.description,
                draft.latitude,
                draft.longitude,
                json.dumps(photos if not live_wins else draft.photo_urls),
                listing_id,
            ),
        )
        action = "merged"
    conn.execute(
        """
        INSERT OR IGNORE INTO listing_sources (listing_id, portal, url, external_id, days_on_market)
        VALUES (?, ?, ?, ?, ?)
        """,
        (listing_id, draft.portal, draft.url, draft.external_id, draft.days_on_market),
    )
    return action


def _store_run(conn, source_run, summary: dict, *, is_sample: bool) -> dict:
    payload = source_run.as_dict()
    kept = 0
    for draft in source_run.listings:
        action = upsert_draft(conn, draft, is_sample=is_sample)
        summary[action] = summary.get(action, 0) + 1
        if action in {"inserted", "merged"}:
            kept += 1
    payload["kept"] = kept
    return payload


def _merge_photos(existing: list[str], incoming: list[str]) -> list[str]:
    merged: list[str] = []
    for url in list(existing) + list(incoming):
        if url and url not in merged:
            merged.append(url)
    return merged


def _prefer_text(current: str | None, incoming: str | None) -> str | None:
    if incoming and (not current or len(incoming) > len(current)):
        return incoming
    return current


def _now() -> str:
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat()
