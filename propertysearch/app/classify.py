"""Lane and renovation band for one listing.

Order is fixed: (1) building condition, (2) Dutch and French keywords in the
title and description, (3) EPC and construction year as a note only.
EPC and year never move a listing between lanes.
"""

from __future__ import annotations

import re
import unicodedata
from dataclasses import dataclass

MOVE_IN_CONDITIONS = {"as_new", "just_renovated", "good"}
RENOVATE_CONDITIONS = {
    "to_be_done_up": "to_be_done_up",
    "to_renovate": "to_renovate",
    "to_restore": "to_restore",
    "casco": "to_restore",
}

_CONDITION_ALIASES = {
    "as new": "as_new",
    "asnew": "as_new",
    "new": "as_new",
    "just renovated": "just_renovated",
    "newly renovated": "just_renovated",
    "freshly renovated": "just_renovated",
    "good": "good",
    "good condition": "good",
    "to be done up": "to_be_done_up",
    "to refresh": "to_be_done_up",
    "to renovate": "to_renovate",
    "to restore": "to_restore",
    "casco": "casco",
}


@dataclass(frozen=True)
class Classification:
    lane: str
    band: str | None
    reason: str
    supporting_note: str


def normalise_condition(raw: str | None) -> str | None:
    if raw is None:
        return None
    text = raw.strip().lower().replace("-", " ").replace("_", " ")
    text = re.sub(r"\s+", " ", text)
    if not text:
        return None
    return _CONDITION_ALIASES.get(text, text if text in _CONDITION_ALIASES.values() else None)


def fold_text(*parts: str | None) -> str:
    raw = "\n".join(part or "" for part in parts)
    normalised = unicodedata.normalize("NFKD", raw)
    stripped = "".join(ch for ch in normalised if not unicodedata.combining(ch))
    return stripped.lower()


def _forced_renovate_band(text: str) -> str | None:
    """Phrases that say the apartment must be worked on, strongest first."""
    if re.search(r"\bcasco\b|\bruwbouw\b|te restaureren|\ba restaurer\b|\bto restore\b", text):
        return "to_restore"
    if re.search(
        r"\bte renoveren\b|\ba renover\b|\bto renovate\b|moet gerenoveerd|renovatie nodig",
        text,
    ):
        return "to_renovate"
    if re.search(
        r"op te frissen|te moderniseren|\bto be done up\b|lichte renovatie|\ba rafraichir\b",
        text,
    ):
        return "to_be_done_up"
    return None


def _dated_rooms(text: str) -> bool:
    """Close variants of keuken / badkamer that describe work, not a bare mention."""
    patterns = (
        r"(oude|gedateerde|verouderde)\s+keuken",
        r"keuken\s+(is\s+)?(te vernieuwen|gedateerd|verouderd)",
        r"(oude|gedateerde|verouderde)\s+badkamer",
        r"badkamer\s+(is\s+)?(te vernieuwen|gedateerd|verouderd)",
        r"cuisine\s+(a renover|desuete|a rafraichir)",
        r"salle de bains?\s+(a renover|desuete)",
    )
    return any(re.search(pattern, text) for pattern in patterns)


def _move_in_keywords(text: str) -> bool:
    if re.search(
        r"instapklaar|comme neuf|nieuwbouw|zo goed als nieuw|pret a emmenager",
        text,
    ):
        return True
    if re.search(r"\bgerenoveerd\b", text) and not re.search(r"\b(niet|te)\s+gerenoveerd\b", text):
        return True
    for match in re.finditer(r"\b(nieuw|nieuwe|neuf|neuve)\b", text):
        after = text[match.end() : match.end() + 24]
        if re.match(r"\s+(keuken|badkamer|cuisine|salle)\b", after):
            continue
        before = text[max(0, match.start() - 6) : match.start()]
        if before.endswith("niet ") or before.endswith("te "):
            continue
        return True
    return False


def supporting_note(epc_label: str | None, construction_year: int | None) -> str:
    bits: list[str] = []
    if epc_label:
        bits.append(f"EPC {epc_label}")
    if construction_year:
        bits.append(f"construction year {construction_year}")
    if not bits:
        return "No EPC or construction year stored."
    joined = " and ".join(bits)
    return f"{joined} noted as supporting context only; it does not change the lane."


def classify(
    *,
    building_condition: str | None,
    title: str | None,
    description: str | None,
    epc_label: str | None = None,
    construction_year: int | None = None,
) -> Classification:
    text = fold_text(title, description)
    condition = normalise_condition(building_condition)
    note = supporting_note(epc_label, construction_year)
    forced = _forced_renovate_band(text)

    if condition in RENOVATE_CONDITIONS:
        band = RENOVATE_CONDITIONS[condition]
        return Classification(
            lane="renovate",
            band=band,
            reason=f"Building condition is {condition.replace('_', ' ')}.",
            supporting_note=note,
        )

    if condition in MOVE_IN_CONDITIONS:
        if forced:
            return Classification(
                lane="renovate",
                band=forced,
                reason=(
                    "Condition reads as move-in, but the title or description says "
                    "the apartment still has to be renovated, refreshed, or restored."
                ),
                supporting_note=note,
            )
        if _dated_rooms(text):
            return Classification(
                lane="move_in",
                band="dated_livable",
                reason=(
                    "Building condition is "
                    f"{condition.replace('_', ' ')}. The text describes a dated kitchen "
                    "or bathroom, so the renovation band sits above the €10,000 move-in cap."
                ),
                supporting_note=note,
            )
        return Classification(
            lane="move_in",
            band="move_in",
            reason=f"Building condition is {condition.replace('_', ' ')}, with no renovate wording.",
            supporting_note=note,
        )

    if forced:
        return Classification(
            lane="renovate",
            band=forced,
            reason="No building condition. Dutch or French keywords say work is required.",
            supporting_note=note,
        )
    if _move_in_keywords(text):
        band = "dated_livable" if _dated_rooms(text) else "move_in"
        return Classification(
            lane="move_in",
            band=band,
            reason="No building condition. Keywords describe a move-in apartment.",
            supporting_note=note,
        )
    if _dated_rooms(text):
        return Classification(
            lane="renovate",
            band="dated_livable",
            reason="No building condition. The kitchen or bathroom is described as dated.",
            supporting_note=note,
        )
    return Classification(
        lane="review",
        band=None,
        reason="No building condition and no clear move-in or renovate keywords. Photos first.",
        supporting_note=note,
    )
