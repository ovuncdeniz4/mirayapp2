"""Planning figures for the €246,000 loan. Not mortgage or tax advice.

Flanders sole primary home, 2026 compromis assumption: 2% registration duty
when the buyers qualify. Notary and file costs are estimated as €1,800 plus
0.3% of the asking price. On €200,000 that is €4,000 registration and €2,400
notary, about €6,400 altogether.

A fuller notary quote on this budget is often €4,000–€4,500. Cards use the
calibrated planning figure so the €6,400 example stays checkable. Renovation
bands scale a little when living area is above 80 m²: +0.4% per extra m².
"""

from __future__ import annotations

LOAN_EUR = 246_000
DISCLAIMER = (
    "Planning aid, not mortgage or tax advice. Figures assume a Flanders sole "
    "primary home and a 2026 compromis at 2% registration duty if you qualify, "
    "plus notary and file costs estimated as €1,800 + 0.3% of the asking price. "
    "On €200,000 that is €4,000 registration and €2,400 notary, about €6,400. "
    "A fuller notary quote on this budget is often €4,000–€4,500; cards use the "
    "calibrated planning figure. The loan ceiling is €246,000. Renovation ranges "
    "are rough bands, scaled slightly when the living area is above 80 m²."
)
REGISTRATION_RATE = 0.02
NOTARY_BASE_EUR = 1_800
NOTARY_RATE = 0.003
SURFACE_SCALE_PER_M2 = 0.004

# (low, high). Restore/casco is "€70,000 and up"; the high figure is only the
# ceiling the loan filter uses, and the UI says "and up".
RENOVATION_BANDS: dict[str, tuple[int, int]] = {
    "move_in": (0, 10_000),
    "dated_livable": (15_000, 25_000),
    "to_be_done_up": (25_000, 40_000),
    "to_renovate": (40_000, 70_000),
    "to_restore": (70_000, 120_000),
}

BAND_LABELS = {
    "move_in": "Move-in",
    "dated_livable": "Dated but livable",
    "to_be_done_up": "To be done up",
    "to_renovate": "To renovate",
    "to_restore": "To restore or casco",
}


def surface_factor(surface_m2: float | None) -> float:
    if surface_m2 is None or surface_m2 <= 80:
        return 1.0
    return 1.0 + (surface_m2 - 80) * SURFACE_SCALE_PER_M2


def purchase_costs(price: int) -> dict[str, int]:
    registration = int(round(price * REGISTRATION_RATE))
    notary = int(round(NOTARY_BASE_EUR + price * NOTARY_RATE))
    return {
        "registration_duty": registration,
        "notary_and_admin": notary,
        "total": registration + notary,
    }


def renovation_range(band: str | None, surface_m2: float | None) -> tuple[int | None, int | None]:
    if band is not None and band not in RENOVATION_BANDS:
        return None, None
    if band is None:
        return None, None
    low, high = RENOVATION_BANDS[band]
    factor = surface_factor(surface_m2)
    return int(round(low * factor)), int(round(high * factor))


def budget(
    *,
    price: int,
    surface_m2: float | None,
    band: str | None,
) -> dict[str, int | bool | None]:
    """Cash and headroom after applying the loan to price + costs + renovation."""
    costs = purchase_costs(price)
    reno_low, reno_high = renovation_range(band, surface_m2)
    cost_total = costs["total"]

    if reno_low is None or reno_high is None:
        project_price_and_costs = price + cost_total
        return {
            "registration_duty": costs["registration_duty"],
            "notary_and_admin": costs["notary_and_admin"],
            "purchase_costs": cost_total,
            "reno_low": None,
            "reno_high": None,
            "project_low": None,
            "project_high": None,
            "cash_needed_low": None,
            "cash_needed_high": None,
            "headroom_low": None,
            "headroom_high": None,
            "fits_loan": False,
            "over_loan": project_price_and_costs > LOAN_EUR,
            "renovation_unknown": True,
        }

    project_low = price + cost_total + reno_low
    project_high = price + cost_total + reno_high
    return {
        "registration_duty": costs["registration_duty"],
        "notary_and_admin": costs["notary_and_admin"],
        "purchase_costs": cost_total,
        "reno_low": reno_low,
        "reno_high": reno_high,
        "project_low": project_low,
        "project_high": project_high,
        "cash_needed_low": max(0, project_low - LOAN_EUR),
        "cash_needed_high": max(0, project_high - LOAN_EUR),
        "headroom_low": LOAN_EUR - project_high,
        "headroom_high": LOAN_EUR - project_low,
        "fits_loan": project_high <= LOAN_EUR,
        "over_loan": project_low > LOAN_EUR,
        "renovation_unknown": False,
    }
