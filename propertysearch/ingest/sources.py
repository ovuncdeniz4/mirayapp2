"""Fetch the three active portals, or parse a page saved from a browser."""

from __future__ import annotations

import time
from dataclasses import dataclass, field
from pathlib import Path

from ingest import parse_immoweb, parse_immovlan, parse_zimmo
from ingest.filters import passes_search
from ingest.http_client import fetch
from ingest.models import ListingDraft
from ingest.robots import allowed, parser_from_text

PAGE_DELAY_SECONDS = 3.0
ROOT = Path(__file__).resolve().parents[1]
INBOX = ROOT / "data" / "inbox"


@dataclass
class SourceRun:
    portal: str
    status: str
    detail: str
    http_status: int | None = None
    fetched_urls: list[str] = field(default_factory=list)
    parsed: int = 0
    kept: int = 0
    listings: list[ListingDraft] = field(default_factory=list)

    def as_dict(self) -> dict:
        return {
            "portal": self.portal,
            "status": self.status,
            "detail": self.detail,
            "http_status": self.http_status,
            "fetched_urls": self.fetched_urls,
            "parsed": self.parsed,
            "kept": self.kept,
        }


def fetch_active(delay: float = PAGE_DELAY_SECONDS) -> list[SourceRun]:
    """Try each active portal. A block stops that portal and the next one still runs."""
    plans = (
        ("immoweb", "https://www.immoweb.be/robots.txt", [parse_immoweb.candidate_urls()], parse_immoweb.parse_document),
        (
            "zimmo",
            "https://www.zimmo.be/robots.txt",
            [_paged(base) for base in parse_zimmo.CANDIDATE_URLS],
            parse_zimmo.parse_document,
        ),
        ("immovlan", "https://immovlan.be/robots.txt", parse_immovlan.candidate_groups(), parse_immovlan.parse_document),
    )
    runs: list[SourceRun] = []
    for index, (portal, robots_url, groups, parse) in enumerate(plans):
        if index:
            time.sleep(delay)
        runs.append(_fetch_group(portal, robots_url, groups, parse, delay))
    return runs


def _paged(base: str, pages: int = 3) -> list[str]:
    urls = [base]
    for page in range(2, pages + 1):
        joiner = "&" if "?" in base else "?"
        urls.append(f"{base}{joiner}page={page}")
    return urls


def _fetch_group(portal: str, robots_url: str, groups: list[list[str]], parse, delay: float) -> SourceRun:
    robots_fetch = fetch(robots_url)
    if robots_fetch.status in {401, 403, 429}:
        return SourceRun(
            portal=portal,
            status="skipped",
            detail="robots.txt refused the request, so this portal was not fetched.",
            http_status=robots_fetch.status,
        )
    if robots_fetch.status == 404:
        parser = parser_from_text("User-agent: *\nAllow: /\n")
    elif robots_fetch.status != 200 or not robots_fetch.body.strip():
        return SourceRun(
            portal=portal,
            status="skipped",
            detail="robots.txt was not available, so this portal was not fetched.",
            http_status=robots_fetch.status or None,
        )
    else:
        parser = parser_from_text(robots_fetch.body)
    agent = "Mozilla/5.0"
    chosen: list[str] | None = None
    for group in groups:
        permitted = [url for url in group if allowed(parser, agent, url)]
        if permitted:
            chosen = permitted
            break
    if not chosen:
        return SourceRun(
            portal=portal,
            status="skipped",
            detail="The search URL is disallowed by robots.txt.",
        )

    listings: list[ListingDraft] = []
    fetched: list[str] = []
    last_status: int | None = None
    for index, url in enumerate(chosen):
        if index:
            time.sleep(delay)
        result = fetch(url)
        last_status = result.status
        if not allowed(parser, agent, result.url):
            return SourceRun(
                portal=portal,
                status="skipped",
                detail="The response URL is disallowed by robots.txt. Stopped.",
                http_status=result.status,
                fetched_urls=fetched,
                listings=listings,
                parsed=len(listings),
            )
        if result.blocked:
            detail = result.error or "The portal did not serve the search page."
            return SourceRun(
                portal=portal,
                status="blocked",
                detail=f"{detail} Stopped without a bypass.",
                http_status=result.status,
                fetched_urls=fetched + [url],
                listings=listings,
                parsed=len(listings),
            )
        fetched.append(result.url)
        page_listings = parse(result.body)
        if not page_listings:
            break
        listings.extend(page_listings)

    detail_note = ""
    if portal == "immoweb" and listings:
        listings, detail_note = _enrich_immoweb_details(listings, parser, delay)

    if not listings:
        return SourceRun(
            portal=portal,
            status="empty",
            detail="The page loaded but no structured listings were found.",
            http_status=last_status,
            fetched_urls=fetched,
        )
    return SourceRun(
        portal=portal,
        status="ok",
        detail=(
            f"Parsed {len(listings)} structured result(s) before local filters."
            + (f" {detail_note}" if detail_note else "")
        ),
        http_status=last_status,
        fetched_urls=fetched,
        parsed=len(listings),
        listings=listings,
    )


def _enrich_immoweb_details(listings: list[ListingDraft], parser, delay: float) -> tuple[list[ListingDraft], str]:
    """Listing pages carry buildingCondition. Search cards usually do not.

    Only listings that already match the Antwerp filters are opened, with a
    pause between them, and the first block stops the rest.
    """
    agent = "Mozilla/5.0"
    fetched = 0
    filled = 0
    blocked = False
    for draft in listings:
        if blocked or fetched >= 30:
            break
        if not passes_search(draft) or draft.building_condition:
            continue
        if not draft.url.startswith("http") or not allowed(parser, agent, draft.url):
            continue
        time.sleep(delay)
        result = fetch(draft.url)
        fetched += 1
        if result.blocked or not allowed(parser, agent, result.url):
            blocked = True
            break
        detail = parse_immoweb.parse_detail(result.body)
        if detail is None:
            continue
        if detail.building_condition:
            draft.building_condition = detail.building_condition
            filled += 1
        if detail.description and len(detail.description) > len(draft.description or ""):
            draft.description = detail.description
        if detail.construction_year and not draft.construction_year:
            draft.construction_year = detail.construction_year
        if detail.epc_label and not draft.epc_label:
            draft.epc_label = detail.epc_label
        if detail.latitude and draft.latitude is None:
            draft.latitude = detail.latitude
            draft.longitude = detail.longitude
    note = f"Opened {fetched} listing page(s) for building condition; {filled} included one."
    if blocked:
        note += " A listing page was blocked, so further detail fetches stopped."
    if fetched >= 30:
        note += " Stopped at 30 listing pages."
    return listings, note


def read_inbox() -> list[SourceRun]:
    """Pages a person saved from their own browser. Not fetched by this process."""
    if not INBOX.exists():
        return []
    parsers = {
        "immoweb": parse_immoweb.parse_document,
        "zimmo": parse_zimmo.parse_document,
        "immovlan": parse_immovlan.parse_document,
    }
    runs: list[SourceRun] = []
    for path in sorted(INBOX.iterdir()):
        if path.name.startswith("."):
            continue
        if path.suffix.lower() not in {".html", ".json", ".txt"}:
            continue
        portal = next((name for name in parsers if path.name.lower().startswith(name)), None)
        if portal is None:
            runs.append(
                SourceRun(
                    portal="inbox",
                    status="skipped",
                    detail=f"{path.name} was ignored. Name it immoweb-*, zimmo-*, or immovlan-*.",
                )
            )
            continue
        listings = parsers[portal](path.read_text(encoding="utf-8", errors="replace"))
        runs.append(
            SourceRun(
                portal=portal,
                status="ok" if listings else "empty",
                detail=f"Read saved page {path.name}.",
                fetched_urls=[str(path)],
                parsed=len(listings),
                listings=listings,
            )
        )
    return runs
