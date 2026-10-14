"""Draft listing produced by a portal parser, before dedupe and storage."""

from __future__ import annotations

from dataclasses import dataclass, field


@dataclass
class ListingDraft:
    portal: str
    external_id: str
    url: str
    price: int | None = None
    postcode: str | None = None
    municipality: str | None = None
    street: str | None = None
    surface_m2: float | None = None
    bedrooms: int | None = None
    epc_label: str | None = None
    construction_year: int | None = None
    building_condition: str | None = None
    title: str | None = None
    description: str | None = None
    latitude: float | None = None
    longitude: float | None = None
    photo_urls: list[str] = field(default_factory=list)
    days_on_market: int | None = None
    property_type: str | None = None
    transaction_type: str | None = None

    def fallback_id(self) -> str:
        return f"{self.portal}:{self.external_id or self.url}"
