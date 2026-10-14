"""Dedupe key: postcode + normalised street + surface + price."""

from __future__ import annotations

import re
import unicodedata

from ingest.models import ListingDraft


def normalise_street(street: str | None) -> str:
    if not street:
        return ""
    text = unicodedata.normalize("NFKD", street)
    text = "".join(ch for ch in text if not unicodedata.combining(ch))
    text = text.lower().replace("str.", "straat").replace("ln.", "laan")
    text = re.sub(r"[^a-z0-9]+", " ", text)
    return re.sub(r"\s+", " ", text).strip()


def dedupe_key(draft: ListingDraft) -> str:
    street = normalise_street(draft.street)
    if not street or draft.postcode is None or draft.surface_m2 is None or draft.price is None:
        return f"incomplete|{draft.fallback_id()}"
    surface = str(int(round(float(draft.surface_m2))))
    return f"{draft.postcode}|{street}|{surface}|{int(draft.price)}"
