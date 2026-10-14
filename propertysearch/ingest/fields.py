"""Shared field extraction for portal JSON. Missing fields stay missing."""

from __future__ import annotations

import re
from typing import Any


def as_int(value: Any) -> int | None:
    if value is None or value is False:
        return None
    if isinstance(value, bool):
        return None
    if isinstance(value, (int, float)):
        return int(value)
    text = str(value).strip().replace(" ", "").replace(".", "").replace(",", "")
    digits = re.search(r"\d+", text)
    if not digits:
        return None
    return int(digits.group(0))


def as_float(value: Any) -> float | None:
    if value is None or isinstance(value, bool):
        return None
    if isinstance(value, (int, float)):
        return float(value)
    text = str(value).strip().replace(" ", "").replace(",", ".")
    match = re.search(r"\d+(?:\.\d+)?", text)
    if not match:
        return None
    return float(match.group(0))


def first_year(value: Any) -> int | None:
    if value is None:
        return None
    match = re.search(r"(18|19|20)\d{2}", str(value))
    if not match:
        return None
    year = int(match.group(0))
    if 1800 <= year <= 2035:
        return year
    return None


def epc_label(value: Any) -> str | None:
    if value is None:
        return None
    text = str(value).strip().upper()
    match = re.fullmatch(r"([A-G])\+?", text)
    if match:
        return match.group(1)
    return None


def price_from(value: Any) -> int | None:
    if isinstance(value, dict):
        for key in ("mainValue", "main_value", "amount", "value", "price"):
            if key in value and value[key] is not None:
                found = price_from(value[key])
                if found:
                    return found
        return None
    found = as_int(value)
    if found is None or found < 10_000 or found > 5_000_000:
        return None
    return found


def photo_list(value: Any) -> list[str]:
    urls: list[str] = []
    if isinstance(value, str) and value.startswith("http"):
        urls.append(value)
    elif isinstance(value, list):
        for item in value:
            urls.extend(photo_list(item))
    elif isinstance(value, dict):
        for key in ("largeUrl", "mediumUrl", "smallUrl", "url", "src", "uri"):
            candidate = value.get(key)
            if isinstance(candidate, str) and candidate.startswith("http"):
                urls.append(candidate)
                break
    seen: list[str] = []
    for url in urls:
        if url not in seen:
            seen.append(url)
    return seen


def dig(obj: dict, *path: str) -> Any:
    current: Any = obj
    for key in path:
        if not isinstance(current, dict) or key not in current:
            return None
        current = current[key]
    return current
