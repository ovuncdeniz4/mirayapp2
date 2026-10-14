"""Keep a draft only when it matches the Antwerp search. Do not invent gaps."""

from __future__ import annotations

from ingest.models import ListingDraft

MAX_PRICE = 250_000
MIN_BEDROOMS = 2
MIN_SURFACE = 80
RENT_OR_OUT = {
    "FOR_RENT",
    "RENT",
    "TE_HUUR",
    "A_LOUER",
    "PUBLIC_SALE",
    "LIFE_ANNUITY",
    "NOTARY_SALE",
}
NOT_APARTMENT = {"HOUSE", "HUIS", "MAISON", "LAND", "GROND", "TERRAIN", "GARAGE", "OFFICE", "PARKING"}


def passes_search(draft: ListingDraft) -> bool:
    if draft.price is None or draft.price > MAX_PRICE or draft.price <= 0:
        return False
    if draft.bedrooms is None or draft.bedrooms < MIN_BEDROOMS:
        return False
    if draft.surface_m2 is None or draft.surface_m2 < MIN_SURFACE:
        return False
    if not draft.postcode or not str(draft.postcode).isdigit():
        return False
    postcode = int(draft.postcode)
    if postcode < 2000 or postcode > 2999:
        return False
    if draft.property_type:
        kind = draft.property_type.upper().replace("-", "_").replace(" ", "_")
        if kind in NOT_APARTMENT:
            return False
    if draft.transaction_type:
        kind = draft.transaction_type.upper().replace("-", "_").replace(" ", "_")
        if kind in RENT_OR_OUT:
            return False
    return True
