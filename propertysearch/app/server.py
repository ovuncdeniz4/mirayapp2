"""Localhost dashboard. Binds to 127.0.0.1 from run.py."""

from __future__ import annotations

import json
import sqlite3
from pathlib import Path

from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse, HTMLResponse
from fastapi.staticfiles import StaticFiles

from app.costs import DISCLAIMER, LOAN_EUR
from app.db import DEFAULT_DB, connect, listing_count
from app.present import present

STATIC = Path(__file__).resolve().parent / "static"
LANES = {"move_in", "renovate", "review", "shortlist"}


def create_app(db_path: Path | None = None) -> FastAPI:
    app = FastAPI(title="Antwerp apartment search", docs_url=None, redoc_url=None)
    app.state.db_path = Path(db_path) if db_path else DEFAULT_DB
    app.mount("/static", StaticFiles(directory=STATIC), name="static")

    @app.get("/")
    def index() -> FileResponse:
        return FileResponse(STATIC / "index.html")

    @app.get("/api/health")
    def health() -> dict:
        return {"ok": True, "loan": LOAN_EUR}

    @app.get("/api/listings")
    def listings(
        lane: str = "move_in",
        municipality: str | None = None,
        max_price: int | None = None,
        min_surface: int | None = None,
        min_bedrooms: int | None = None,
        epc: str | None = None,
        fits: int = 0,
        samples: int | None = None,
    ) -> dict:
        if lane not in LANES:
            raise HTTPException(status_code=400, detail="Unknown lane")
        conn = connect(app.state.db_path)
        try:
            payload = _query(
                conn,
                lane=lane,
                municipality=municipality or "",
                max_price=max_price,
                min_surface=min_surface,
                min_bedrooms=min_bedrooms,
                epc=(epc or "").upper(),
                fits=bool(fits),
                samples=samples,
            )
        finally:
            conn.close()
        return payload

    @app.post("/api/listings/{listing_id}/shortlist")
    def toggle_shortlist(listing_id: int) -> dict:
        conn = connect(app.state.db_path)
        try:
            row = conn.execute("SELECT id FROM listings WHERE id = ?", (listing_id,)).fetchone()
            if row is None:
                raise HTTPException(status_code=404, detail="Listing not found")
            existing = conn.execute(
                "SELECT 1 FROM shortlist WHERE listing_id = ?",
                (listing_id,),
            ).fetchone()
            if existing:
                conn.execute("DELETE FROM shortlist WHERE listing_id = ?", (listing_id,))
                shortlisted = False
            else:
                conn.execute("INSERT INTO shortlist (listing_id) VALUES (?)", (listing_id,))
                shortlisted = True
            conn.commit()
        finally:
            conn.close()
        return {"id": listing_id, "shortlisted": shortlisted}

    @app.get("/sample-listing/{external_id}")
    def sample_listing(external_id: str) -> HTMLResponse:
        safe = "".join(ch for ch in external_id if ch.isalnum() or ch in "-_")
        body = f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <title>Sample listing</title>
  <link rel="stylesheet" href="/static/styles.css" />
</head>
<body>
  <main class="sample-page">
    <p class="badge sample">Sample</p>
    <h1>Sample fixture, not a live listing</h1>
    <p>Reference <code>{safe}</code> is invented data for the local dashboard.
    It is not an advertisement on Immoweb, Zimmo, or Immovlan.</p>
    <p><a href="/">Back to the dashboard</a></p>
  </main>
</body>
</html>"""
        return HTMLResponse(body)

    return app


def _query(
    conn: sqlite3.Connection,
    *,
    lane: str,
    municipality: str,
    max_price: int | None,
    min_surface: int | None,
    min_bedrooms: int | None,
    epc: str,
    fits: bool,
    samples: int | None,
) -> dict:
    live = listing_count(conn, sample=0)
    sample_n = listing_count(conn, sample=1)
    if samples is None:
        include_samples = live == 0
    else:
        include_samples = bool(samples)
    rows = conn.execute("SELECT * FROM listings ORDER BY price ASC, id ASC").fetchall()
    if not include_samples:
        rows = [row for row in rows if not row["is_sample"]]
    sources = _grouped(conn, "listing_sources", "listing_id")
    shortlisted_ids = {
        row["listing_id"]
        for row in conn.execute("SELECT listing_id FROM shortlist").fetchall()
    }
    cards = [present(row, sources.get(row["id"], []), row["id"] in shortlisted_ids) for row in rows]
    municipalities = sorted({card["municipality"] for card in cards if card["municipality"]})

    def coarse(card: dict) -> bool:
        if municipality and card["municipality"] != municipality:
            return False
        if max_price is not None and card["price"] > max_price:
            return False
        if min_surface is not None and (card["surface_m2"] or 0) < min_surface:
            return False
        if min_bedrooms is not None and (card["bedrooms"] or 0) < min_bedrooms:
            return False
        if epc and (card["epc_label"] or "") != epc:
            return False
        if fits and not card["fits_loan"]:
            return False
        return True

    matched = [card for card in cards if coarse(card)]
    counts = {
        "move_in": sum(1 for card in matched if card["lane"] == "move_in"),
        "renovate": sum(1 for card in matched if card["lane"] == "renovate"),
        "review": sum(1 for card in matched if card["lane"] == "review"),
        "shortlist": sum(1 for card in matched if card["shortlisted"]),
    }
    if lane == "shortlist":
        visible = [card for card in matched if card["shortlisted"]]
    else:
        visible = [card for card in matched if card["lane"] == lane]
    last = conn.execute(
        "SELECT summary_json FROM ingest_runs ORDER BY id DESC LIMIT 1"
    ).fetchone()
    return {
        "listings": visible,
        "counts": counts,
        "municipalities": municipalities,
        "disclaimer": DISCLAIMER,
        "loan": LOAN_EUR,
        "dataset": "sample" if include_samples and live == 0 else "live",
        "live_listings": live,
        "sample_listings": sample_n,
        "showing_samples": include_samples and sample_n > 0 and live == 0,
        "last_run": json.loads(last["summary_json"]) if last else None,
    }


def _grouped(conn: sqlite3.Connection, table: str, key: str) -> dict[int, list]:
    grouped: dict[int, list] = {}
    for row in conn.execute(f"SELECT * FROM {table}").fetchall():
        grouped.setdefault(row[key], []).append(row)
    return grouped
