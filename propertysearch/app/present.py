"""Turn a stored row into the card the dashboard shows."""

from __future__ import annotations

import json

from app.classify import classify
from app.costs import BAND_LABELS, LOAN_EUR, budget


def present(row, sources: list, shortlisted: bool) -> dict:
    photos = json.loads(row["photo_urls"] or "[]")
    classification = classify(
        building_condition=row["building_condition"],
        title=row["title"],
        description=row["description"],
        epc_label=row["epc_label"],
        construction_year=row["construction_year"],
    )
    money = budget(price=row["price"], surface_m2=row["surface_m2"], band=classification.band)
    return {
        "id": row["id"],
        "is_sample": bool(row["is_sample"]),
        "price": row["price"],
        "postcode": row["postcode"],
        "municipality": row["municipality"],
        "street": row["street"],
        "place": f"{row['postcode']} {row['municipality']}".strip(),
        "surface_m2": row["surface_m2"],
        "bedrooms": row["bedrooms"],
        "epc_label": row["epc_label"],
        "construction_year": row["construction_year"],
        "building_condition": row["building_condition"],
        "title": row["title"],
        "lane": classification.lane,
        "band": classification.band,
        "band_label": BAND_LABELS.get(classification.band or "", "Review"),
        "reason": classification.reason,
        "supporting_note": classification.supporting_note,
        "photos": photos,
        "latitude": row["latitude"],
        "longitude": row["longitude"],
        "sources": [
            {
                "portal": source["portal"],
                "url": source["url"],
                "days_on_market": source["days_on_market"],
            }
            for source in sources
        ],
        "shortlisted": shortlisted,
        "loan": LOAN_EUR,
        **money,
    }
