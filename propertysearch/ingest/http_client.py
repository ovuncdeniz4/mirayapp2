"""One polite GET. Stops on a block. Does not solve challenges or retry past them."""

from __future__ import annotations

import urllib.error
import urllib.request
from dataclasses import dataclass

USER_AGENT = (
    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 "
    "(KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36"
)
CONTACT_AGENT = "propertysearch/0.1 (personal local Antwerp apartment search)"
TIMEOUT_SECONDS = 20
MAX_BYTES = 1_500_000

BLOCK_MARKERS = (
    "just a moment",
    "cf-challenge",
    "cf-mitigated",
    "captcha",
    "you were blocked",
    "access denied",
    "attention required",
)


@dataclass(frozen=True)
class FetchResult:
    url: str
    status: int
    body: str
    blocked: bool
    error: str | None = None


def looks_blocked(status: int, body: str) -> bool:
    if status in {401, 403, 429, 503}:
        return True
    lowered = body[:8000].lower()
    return any(marker in lowered for marker in BLOCK_MARKERS)


def fetch(url: str, timeout: int = TIMEOUT_SECONDS) -> FetchResult:
    request = urllib.request.Request(
        url,
        headers={
            "User-Agent": USER_AGENT,
            "Accept": "text/html,application/xhtml+xml,application/json;q=0.9",
            "Accept-Language": "nl-BE,nl;q=0.9,en;q=0.8",
            "From": CONTACT_AGENT,
        },
        method="GET",
    )
    try:
        with urllib.request.urlopen(request, timeout=timeout) as response:
            status = getattr(response, "status", 200)
            raw = response.read(MAX_BYTES)
            final_url = response.geturl()
    except urllib.error.HTTPError as exc:
        raw = exc.read(200_000)
        body = raw.decode("utf-8", errors="replace")
        return FetchResult(
            url=url,
            status=exc.code,
            body=body,
            blocked=looks_blocked(exc.code, body),
            error=f"HTTP {exc.code}",
        )
    except urllib.error.URLError as exc:
        return FetchResult(url=url, status=0, body="", blocked=True, error=str(exc.reason))

    body = raw.decode("utf-8", errors="replace")
    blocked = looks_blocked(status, body)
    return FetchResult(
        url=final_url,
        status=status,
        body=body,
        blocked=blocked,
        error="Blocked or challenged" if blocked else None,
    )
