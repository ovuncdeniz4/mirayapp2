"""Parse an Immovlan search page (JSON-LD or listing cards)."""

from __future__ import annotations

import html
import json
import re
from html.parser import HTMLParser
from typing import Any

from ingest.fields import as_float, as_int, epc_label, first_year, price_from
from ingest.models import ListingDraft

# robots.txt disallows maxPrice and minBedrooms (those exact spellings).
# The lowercase query the site itself links is checked at fetch time and used
# when it is allowed. Price ranges on new-build projects are skipped.
MAX_PAGES = 3
_DETAIL_RE = re.compile(
    r"https?://immovlan\.be/(?:nl|fr|en)/detail/appartement/(?:te-koop|a-vendre|for-sale)/(\d{4})/([^/?#\"']+)/([^/?#\"']+)",
    flags=re.IGNORECASE,
)


FILTERED_SEARCH = (
    "https://immovlan.be/nl/vastgoed/appartement/te-koop"
    "?provinces=antwerpen&maxprice=250000&minbedrooms=2"
)
BROAD_SEARCH = "https://immovlan.be/nl/vastgoed/appartement/te-koop?provinces=antwerpen"


def candidate_groups(pages: int = MAX_PAGES) -> list[list[str]]:
    """Filtered query first. The collector drops a group robots.txt disallows."""
    return [_pages(FILTERED_SEARCH, pages), _pages(BROAD_SEARCH, pages)]


def candidate_urls(pages: int = MAX_PAGES) -> list[str]:
    return _pages(FILTERED_SEARCH, pages)


def _pages(base: str, pages: int) -> list[str]:
    count = max(1, min(pages, MAX_PAGES))
    urls = [base]
    joiner = "&" if "?" in base else "?"
    for page in range(2, count + 1):
        urls.append(f"{base}{joiner}page={page}")
    return urls


def parse_document(body: str) -> list[ListingDraft]:
    cards = _from_search_cards(body)
    if cards:
        return cards
    drafts = _from_json_ld(body)
    if drafts:
        return drafts
    return _from_cards(body)


class _JsonLdParser(HTMLParser):
    def __init__(self) -> None:
        super().__init__()
        self._capture = False
        self._chunks: list[str] = []
        self.blocks: list[str] = []

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        if tag.lower() != "script":
            return
        attr = {key.lower(): value or "" for key, value in attrs}
        if "ld+json" in attr.get("type", ""):
            self._capture = True
            self._chunks = []

    def handle_endtag(self, tag: str) -> None:
        if tag.lower() == "script" and self._capture:
            self.blocks.append("".join(self._chunks))
            self._capture = False

    def handle_data(self, data: str) -> None:
        if self._capture:
            self._chunks.append(data)


def _from_json_ld(body: str) -> list[ListingDraft]:
    parser = _JsonLdParser()
    parser.feed(body)
    drafts: list[ListingDraft] = []
    seen: set[str] = set()
    for block in parser.blocks:
        try:
            payload = json.loads(block.strip())
        except json.JSONDecodeError:
            continue
        for node in _nodes(payload):
            draft = _draft_from_ld(node)
            if draft is None or draft.external_id in seen:
                continue
            seen.add(draft.external_id)
            drafts.append(draft)
    return drafts


def _nodes(payload: Any) -> list[dict]:
    found: list[dict] = []
    if isinstance(payload, list):
        for item in payload:
            found.extend(_nodes(item))
        return found
    if not isinstance(payload, dict):
        return found
    found.append(payload)
    graph = payload.get("@graph")
    if isinstance(graph, list):
        for item in graph:
            found.extend(_nodes(item))
    elements = payload.get("itemListElement")
    if isinstance(elements, list):
        for item in elements:
            if isinstance(item, dict) and isinstance(item.get("item"), dict):
                found.append(item["item"])
            else:
                found.extend(_nodes(item))
    return found


def _draft_from_ld(node: dict) -> ListingDraft | None:
    url = node.get("url") or node.get("@id") or ""
    if not isinstance(url, str):
        return None
    match = _DETAIL_RE.search(url)
    if not match and "appartement" not in url.lower() and node.get("@type") not in {
        "Apartment",
        "Residence",
        "Product",
        "Offer",
        "RealEstateListing",
    }:
        return None
    address = node.get("address") if isinstance(node.get("address"), dict) else {}
    postcode = address.get("postalCode") or (match.group(1) if match else None)
    municipality = address.get("addressLocality") or (match.group(2).replace("-", " ") if match else None)
    street = address.get("streetAddress")
    offers = node.get("offers") if isinstance(node.get("offers"), dict) else {}
    price = price_from(offers.get("price") if offers else None)
    if price is None:
        price = price_from(node.get("price"))
    floor = node.get("floorSize") if isinstance(node.get("floorSize"), dict) else {}
    surface = as_float(floor.get("value") or node.get("floorSize") or node.get("surface"))
    bedrooms = as_int(node.get("numberOfBedrooms") or node.get("numberOfRooms"))
    code = match.group(3) if match else str(node.get("sku") or node.get("productID") or url)
    description = node.get("description") or ""
    title = node.get("name") or "Appartement"
    photos: list[str] = []
    image = node.get("image")
    if isinstance(image, str):
        photos.append(image)
    elif isinstance(image, list):
        photos.extend(str(item) for item in image if isinstance(item, str))
    condition = node.get("buildingCondition") or node.get("additionalProperty")
    if isinstance(condition, list):
        condition = None
    return ListingDraft(
        portal="immovlan",
        external_id=str(code),
        url=url if url.startswith("http") else f"https://immovlan.be{url}",
        price=price,
        postcode=str(postcode).strip() if postcode else None,
        municipality=str(municipality).title().strip() if municipality else None,
        street=str(street).strip() if street else None,
        surface_m2=surface,
        bedrooms=bedrooms,
        epc_label=epc_label(node.get("epc") or node.get("energyRating")),
        construction_year=first_year(node.get("yearBuilt")),
        building_condition=str(condition).strip() if isinstance(condition, str) else None,
        title=str(title).strip(),
        description=str(description).strip(),
        photo_urls=photos,
        property_type="APARTMENT",
        transaction_type="FOR_SALE",
    )


def _from_search_cards(body: str) -> list[ListingDraft]:
    extras = _jsonld_extras(body)
    chunks = re.findall(
        r"(<article\b[^>]*class=\"[^\"]*v3-search-card[\s\"][^>]*>.*?</article>)",
        body,
        flags=re.IGNORECASE | re.DOTALL,
    )
    drafts: list[ListingDraft] = []
    seen: set[str] = set()
    for chunk in chunks:
        url_match = re.search(r'data-url="([^"]+)"', chunk)
        if not url_match:
            continue
        url = html.unescape(url_match.group(1))
        if "projectdetail" in url or "/detail/appartement/" not in url:
            continue
        code_match = _DETAIL_RE.search(url)
        if not code_match or code_match.group(3) in seen:
            continue
        seen.add(code_match.group(3))
        price_match = re.search(r'class="v3-search-card-price"[^>]*>(.*?)</span>', chunk, flags=re.DOTALL)
        price_text = html.unescape(price_match.group(1)) if price_match else ""
        if "-" in price_text or "–" in price_text:
            continue
        price = price_from(re.sub(r"[^\d]", "", price_text))
        surface_match = re.search(
            r"<strong>\s*(\d+)\s*</strong>\s*m(?:²|2|&#xB2;)",
            chunk,
            flags=re.IGNORECASE,
        )
        beds_match = re.search(
            r"<strong>\s*(\d+)\s*</strong>\s*Slaapkamer",
            chunk,
            flags=re.IGNORECASE,
        )
        description_match = re.search(
            r'itemprop="description">(.*?)</p>',
            chunk,
            flags=re.DOTALL | re.IGNORECASE,
        )
        description = ""
        if description_match:
            description = re.sub(r"<[^>]+>", " ", html.unescape(description_match.group(1)))
            description = re.sub(r"\s+", " ", description).strip()
        locality_match = re.search(r'itemprop="addressLocality">(.*?)<', chunk)
        photo_match = re.search(r'data-src="(https:[^"]+)"', chunk)
        extra = extras.get(code_match.group(3).lower(), {})
        epc_match = re.search(r"Flanders(?:Single)?([A-G])\b", chunk)
        drafts.append(
            ListingDraft(
                portal="immovlan",
                external_id=code_match.group(3),
                url=url,
                price=price,
                postcode=code_match.group(1),
                municipality=html.unescape(locality_match.group(1)).strip() if locality_match else code_match.group(2).replace("-", " ").title(),
                surface_m2=as_float(surface_match.group(1) if surface_match else None),
                bedrooms=as_int(beds_match.group(1) if beds_match else None),
                epc_label=epc_label(epc_match.group(1) if epc_match else None) or extra.get("epc"),
                construction_year=extra.get("year"),
                title="Appartement te koop",
                description=description or None,
                photo_urls=[photo_match.group(1)] if photo_match else [],
                property_type="APARTMENT",
                transaction_type="FOR_SALE",
            )
        )
    return drafts


def _jsonld_extras(body: str) -> dict[str, dict]:
    extras: dict[str, dict] = {}
    parser = _JsonLdParser()
    parser.feed(body)
    for block in parser.blocks:
        try:
            payload = json.loads(block.strip())
        except json.JSONDecodeError:
            continue
        if not isinstance(payload, dict) or payload.get("@type") != "RealEstateListing":
            continue
        url = str(payload.get("url") or "")
        match = _DETAIL_RE.search(url)
        if not match:
            continue
        entity = payload.get("mainEntity") if isinstance(payload.get("mainEntity"), dict) else {}
        epc = None
        for prop in entity.get("additionalProperty") or []:
            if isinstance(prop, dict) and prop.get("propertyID") == "energyPerformanceCertificate":
                epc = epc_label(prop.get("value"))
        extras[match.group(3).lower()] = {
            "year": first_year(entity.get("yearBuilt")),
            "epc": epc,
        }
    return extras


def _from_cards(body: str) -> list[ListingDraft]:
    drafts: list[ListingDraft] = []
    seen: set[str] = set()
    for match in _DETAIL_RE.finditer(body):
        code = match.group(3)
        if code in seen:
            continue
        seen.add(code)
        window = html.unescape(re.sub(r"<[^>]+>", " ", body[match.start() : match.start() + 1200]))
        window = re.sub(r"\s+", " ", window)
        price_match = re.search(r"(\d{2,3}(?:[\s.]\d{3})+)\s*€", window)
        surface_match = re.search(r"(\d{2,3}(?:[.,]\d+)?)\s*m²", window)
        beds_match = re.search(r"(\d+)\s+slaapkamer", window, flags=re.IGNORECASE)
        price = price_from(price_match.group(1) if price_match else None)
        drafts.append(
            ListingDraft(
                portal="immovlan",
                external_id=code,
                url=match.group(0),
                price=price,
                postcode=match.group(1),
                municipality=match.group(2).replace("-", " ").title(),
                surface_m2=as_float(surface_match.group(1) if surface_match else None),
                bedrooms=as_int(beds_match.group(1) if beds_match else None),
                title="Appartement te koop",
                description=window[:500],
                property_type="APARTMENT",
                transaction_type="FOR_SALE",
            )
        )
    return drafts
