"""Parse a Zimmo search page. Days on market are kept when the page has them."""

from __future__ import annotations

import html
import json
import re
from typing import Any

from ingest.fields import as_float, as_int, epc_label, first_year, photo_list, price_from
from ingest.models import ListingDraft

# /zoeken/ is disallowed by Zimmo robots.txt. These path-only candidates are
# checked against robots.txt before any request.
CANDIDATE_URLS = (
    "https://www.zimmo.be/nl/antwerpen/te-koop/appartement/",
    "https://www.zimmo.be/nl/provincie/antwerpen/te-koop/appartement/",
)
MAX_PAGES = 3


def candidate_urls(pages: int = MAX_PAGES) -> list[str]:
    count = max(1, min(pages, MAX_PAGES))
    urls: list[str] = []
    for base in CANDIDATE_URLS:
        urls.append(base)
        for page in range(2, count + 1):
            urls.append(f"{base}?page={page}")
        # The first allowed base is enough. Extra bases are fallbacks only
        # when an earlier base was disallowed, not after a block.
        break
    return urls


def fallback_urls(pages: int = 1) -> list[str]:
    count = max(1, min(pages, MAX_PAGES))
    urls: list[str] = []
    for base in CANDIDATE_URLS:
        urls.append(base)
        for page in range(2, count + 1):
            urls.append(f"{base}?pagina={page}")
    return urls


def parse_document(body: str) -> list[ListingDraft]:
    payloads = _payloads(body)
    drafts: list[ListingDraft] = []
    seen: set[str] = set()
    for payload in payloads:
        for item in _walk(payload):
            draft = _draft_from_item(item)
            if draft is None:
                continue
            marker = draft.external_id or draft.url
            if marker in seen:
                continue
            seen.add(marker)
            drafts.append(draft)
    return drafts


def _payloads(body: str) -> list[Any]:
    stripped = body.strip()
    found: list[Any] = []
    if stripped.startswith("{") or stripped.startswith("["):
        try:
            found.append(json.loads(stripped))
        except json.JSONDecodeError:
            pass
    for match in re.finditer(
        r"<script[^>]*>(.*?)</script>",
        body,
        flags=re.IGNORECASE | re.DOTALL,
    ):
        raw = html.unescape(match.group(1)).strip()
        if not raw or raw[0] not in "{[":
            assigned = re.search(r"=\s*(\{.*\}|\[.*\])\s*;?\s*$", raw, flags=re.DOTALL)
            if not assigned:
                continue
            raw = assigned.group(1)
        try:
            found.append(json.loads(raw))
        except json.JSONDecodeError:
            continue
    return found


def _walk(node: Any) -> list[dict]:
    found: list[dict] = []
    if isinstance(node, dict):
        if _looks_like_listing(node):
            found.append(node)
        for value in node.values():
            found.extend(_walk(value))
    elif isinstance(node, list):
        for value in node:
            found.extend(_walk(value))
    return found


def _looks_like_listing(node: dict) -> bool:
    keys = {key.lower() for key in node}
    address = node.get("address") if isinstance(node.get("address"), dict) else {}
    address_keys = {key.lower() for key in address}
    has_price = "price" in keys or "prijs" in keys
    has_place = bool(
        (keys | address_keys) & {"postalcode", "postal_code", "postcode", "zipcode", "zip"}
    )
    has_home = bool(
        keys
        & {
            "bedrooms",
            "bedroomcount",
            "slaapkamers",
            "livablespace",
            "surface",
            "habitable_surface",
        }
    )
    return has_price and has_place and has_home


def _draft_from_item(item: dict) -> ListingDraft | None:
    lowered = {key.lower(): value for key, value in item.items()}
    address = item.get("address") if isinstance(item.get("address"), dict) else {}
    address_l = {key.lower(): value for key, value in address.items()}
    price = price_from(lowered.get("price") or lowered.get("prijs"))
    postcode = (
        address_l.get("postalcode")
        or address_l.get("postal_code")
        or address_l.get("postcode")
        or address_l.get("zip")
        or lowered.get("postalcode")
        or lowered.get("postcode")
        or lowered.get("zipcode")
    )
    postcode = str(postcode).strip() if postcode else None
    municipality = (
        address_l.get("locality")
        or address_l.get("city")
        or address_l.get("municipality")
        or lowered.get("locality")
        or lowered.get("municipality")
    )
    street_name = address_l.get("street") or lowered.get("street") or ""
    number = address_l.get("number") or address_l.get("housenumber") or lowered.get("number") or ""
    street = " ".join(part for part in (str(street_name).strip(), str(number).strip()) if part) or None
    surface = as_float(
        lowered.get("livablespace")
        or lowered.get("livable_space")
        or lowered.get("surface")
        or lowered.get("habitable_surface")
        or lowered.get("woonopp")
    )
    bedrooms = as_int(lowered.get("bedrooms") or lowered.get("bedroomcount") or lowered.get("slaapkamers"))
    condition = lowered.get("condition") or lowered.get("buildingcondition") or lowered.get("staat")
    code = str(lowered.get("code") or lowered.get("id") or lowered.get("zimmo_code") or "").strip()
    url = lowered.get("url") or lowered.get("detailurl") or lowered.get("link")
    if not isinstance(url, str) or not url.startswith("http"):
        slug = code or "listing"
        url = f"https://www.zimmo.be/nl/{slug}/"
    days = as_int(
        lowered.get("daysonline")
        or lowered.get("days_on_market")
        or lowered.get("daysonmarket")
        or lowered.get("dagenonline")
    )
    photos = photo_list(lowered.get("photos") or lowered.get("pictures") or lowered.get("images") or lowered.get("photo"))
    latitude = _coord(address_l.get("lat") or address_l.get("latitude") or lowered.get("lat") or lowered.get("latitude"))
    longitude = _coord(address_l.get("lng") or address_l.get("longitude") or lowered.get("lng") or lowered.get("longitude"))
    title = lowered.get("title") or lowered.get("titel")
    description = lowered.get("description") or lowered.get("omschrijving") or ""
    if isinstance(description, dict):
        description = next(iter(description.values()), "")
    return ListingDraft(
        portal="zimmo",
        external_id=code or f"{postcode or 'x'}-{price or 'x'}",
        url=str(url),
        price=price,
        postcode=postcode,
        municipality=str(municipality).strip() if municipality else None,
        street=street,
        surface_m2=surface,
        bedrooms=bedrooms,
        epc_label=epc_label(lowered.get("epc") or lowered.get("epcscore") or lowered.get("energy_label")),
        construction_year=first_year(lowered.get("constructionyear") or lowered.get("bouwjaar")),
        building_condition=str(condition).strip() if condition else None,
        title=str(title).strip() if title else None,
        description=str(description).strip() if description else None,
        latitude=latitude,
        longitude=longitude,
        photo_urls=photos,
        days_on_market=days,
        property_type=str(lowered.get("type") or "APARTMENT"),
        transaction_type=str(lowered.get("status") or lowered.get("transaction") or "FOR_SALE"),
    )


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
