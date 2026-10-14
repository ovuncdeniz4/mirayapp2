"""Parse Immoweb search HTML or the public classifieds JSON the site embeds."""

from __future__ import annotations

import html
import json
import re
from typing import Any

from ingest.fields import as_float, dig, epc_label, first_year, photo_list, price_from
from ingest.models import ListingDraft

# Path-only search. maxPrice, countries=, and orderBy are disallowed by
# Immoweb robots.txt, so the collector filters those after the page loads.
SEARCH_BASE = "https://www.immoweb.be/en/search/apartment/for-sale/antwerpen/province"
MAX_PAGES = 4


def candidate_urls(pages: int = MAX_PAGES) -> list[str]:
    count = max(1, min(pages, MAX_PAGES))
    urls = [SEARCH_BASE]
    for page in range(2, count + 1):
        urls.append(f"{SEARCH_BASE}?page={page}")
    return urls


def parse_document(body: str) -> list[ListingDraft]:
    payload = _load_payload(body)
    if payload is None:
        return []
    items = _result_items(payload)
    drafts: list[ListingDraft] = []
    for item in items:
        draft = _draft_from_item(item)
        if draft is not None:
            drafts.append(draft)
    return drafts


def _load_payload(body: str) -> Any:
    stripped = body.strip()
    if stripped.startswith("{") or stripped.startswith("["):
        try:
            return json.loads(stripped)
        except json.JSONDecodeError:
            return None

    attribute = re.search(
        r"<iw-search\b[^>]*?\s:results\s*=\s*(['\"])(.*?)\1",
        body,
        flags=re.IGNORECASE | re.DOTALL,
    )
    if attribute:
        raw = html.unescape(attribute.group(2))
        try:
            return json.loads(raw)
        except json.JSONDecodeError:
            return None

    for match in re.finditer(
        r"<script[^>]*type=['\"]application/json['\"][^>]*>(.*?)</script>",
        body,
        flags=re.IGNORECASE | re.DOTALL,
    ):
        try:
            return json.loads(html.unescape(match.group(1)).strip())
        except json.JSONDecodeError:
            continue
    return None


def _result_items(payload: Any) -> list[dict]:
    if isinstance(payload, list):
        return [item for item in payload if isinstance(item, dict)]
    if not isinstance(payload, dict):
        return []
    for key in ("results", "classifieds", "items"):
        value = payload.get(key)
        if isinstance(value, list):
            return [item for item in value if isinstance(item, dict)]
    return []


def _draft_from_item(item: dict) -> ListingDraft | None:
    prop = item.get("property") if isinstance(item.get("property"), dict) else {}
    location = prop.get("location") if isinstance(prop.get("location"), dict) else {}
    if not location and isinstance(item.get("location"), dict):
        location = item["location"]
    transaction = item.get("transaction") if isinstance(item.get("transaction"), dict) else {}
    building = prop.get("building") if isinstance(prop.get("building"), dict) else {}

    external_id = str(item.get("id") or item.get("classifiedId") or "").strip()
    price = price_from(transaction.get("price"))
    if price is None:
        price = price_from(item.get("price"))
    postcode = location.get("postalCode") or location.get("postal_code") or item.get("postalCode")
    postcode = str(postcode).strip() if postcode else None
    municipality = location.get("locality") or location.get("city") or item.get("locality")
    street_parts = [
        str(location.get("street") or item.get("street") or "").strip(),
        str(location.get("number") or item.get("number") or "").strip(),
    ]
    street = " ".join(part for part in street_parts if part) or None
    surface = as_float(
        prop.get("netHabitableSurface")
        or prop.get("habitableSurface")
        or item.get("surface")
        or item.get("netHabitableSurface")
    )
    bedrooms = prop.get("bedroomCount")
    if bedrooms is None:
        bedrooms = item.get("bedroomCount") or item.get("bedrooms")
    try:
        bedrooms_n = int(bedrooms) if bedrooms is not None else None
    except (TypeError, ValueError):
        bedrooms_n = None

    condition = (
        building.get("condition")
        or prop.get("buildingCondition")
        or item.get("buildingCondition")
        or item.get("building_condition")
    )
    title = prop.get("title") or item.get("title")
    description = prop.get("description") or item.get("description") or ""
    if isinstance(description, dict):
        description = description.get("nl") or description.get("fr") or description.get("en") or ""
    pitch = prop.get("salesPitch")
    if isinstance(pitch, dict):
        pitch = pitch.get("text") or ""
    if not description and pitch:
        description = str(pitch)
    year = first_year(
        building.get("constructionYear")
        or prop.get("constructionYear")
        or item.get("constructionYear")
    )
    certificate = transaction.get("certificate") if isinstance(transaction, dict) else None
    epc = epc_label(
        dig(prop, "energy", "epcScore")
        or dig(item, "energy", "epcScore")
        or item.get("epcScore")
        or item.get("epc")
        or certificate
    )
    if epc is None:
        epc = _epc_from_text(str(description))
    photos = photo_list((item.get("media") or {}).get("pictures") if isinstance(item.get("media"), dict) else None)
    if not photos:
        photos = photo_list(item.get("pictures") or item.get("photo"))
    latitude = _coord(location.get("latitude") or item.get("latitude"))
    longitude = _coord(location.get("longitude") or item.get("longitude"))
    property_type = prop.get("type") or item.get("propertyType") or item.get("type")
    transaction_type = transaction.get("type") or item.get("transactionType")
    if not external_id:
        external_id = f"{postcode or 'x'}-{price or 'x'}-{street or 'x'}"
    url = item.get("url") or f"https://www.immoweb.be/en/classified/{external_id}"
    return ListingDraft(
        portal="immoweb",
        external_id=external_id,
        url=str(url),
        price=price,
        postcode=postcode,
        municipality=str(municipality).strip() if municipality else None,
        street=street,
        surface_m2=surface,
        bedrooms=bedrooms_n,
        epc_label=epc,
        construction_year=year,
        building_condition=str(condition).strip() if condition else None,
        title=str(title).strip() if title else None,
        description=str(description).strip() if description else None,
        latitude=latitude,
        longitude=longitude,
        photo_urls=photos,
        property_type=str(property_type).strip() if property_type else None,
        transaction_type=str(transaction_type).strip() if transaction_type else None,
    )


def parse_detail(body: str) -> ListingDraft | None:
    """Read window.classified on a public listing page. That blob carries condition."""
    match = re.search(r"window\.classified\s*=\s*(\{.*?\})\s*;\s*\n", body, flags=re.DOTALL)
    if not match:
        return None
    try:
        payload = json.loads(match.group(1))
    except json.JSONDecodeError:
        return None
    if not isinstance(payload, dict):
        return None
    return _draft_from_item(payload)


def _epc_from_text(text: str) -> str | None:
    labeled = re.search(r"\bEPC\s+\d[\d\s.]*\(\s*([A-G])\s*\)", text, flags=re.IGNORECASE)
    if labeled:
        return labeled.group(1).upper()
    short = re.search(r"\bEPC\s+([A-G])\b", text, flags=re.IGNORECASE)
    if short:
        return short.group(1).upper()
    return None


def _coord(value: Any) -> float | None:
    try:
        if value is None:
            return None
        number = float(value)
    except (TypeError, ValueError):
        return None
    if number == 0:
        return None
    return number
