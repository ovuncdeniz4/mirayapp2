"""robots.txt gate with * and $ wildcards.

The Python standard-library parser only checks path prefixes, so a rule such
as ``Disallow: *maxprice*`` would not match. These portal files are written
for the usual wildcard rules, and the collector follows those.
"""

from __future__ import annotations

import re
from urllib.parse import unquote, urlparse


class Robots:
    def __init__(self, groups: list[tuple[list[str], list[tuple[str, str]]]]):
        self.groups = groups

    def can_fetch(self, user_agent: str, url: str) -> bool:
        rules = _select_group(self.groups, user_agent)
        if not rules:
            return True
        target = _target(url)
        best_length = -1
        best_allow = True
        for kind, pattern in rules:
            if kind == "disallow" and pattern == "":
                continue
            if not _matches(pattern, target):
                continue
            length = len(pattern)
            allow = kind == "allow"
            if length > best_length or (length == best_length and allow):
                best_length = length
                best_allow = allow
        return best_allow


def parser_from_text(robots_txt: str) -> Robots:
    groups: list[tuple[list[str], list[tuple[str, str]]]] = []
    agents: list[str] = []
    rules: list[tuple[str, str]] = []

    def flush() -> None:
        nonlocal agents, rules
        if agents:
            groups.append((agents, rules))
        agents = []
        rules = []

    for raw in robots_txt.splitlines():
        line = raw.split("#", 1)[0].strip()
        if not line or ":" not in line:
            continue
        key, value = line.split(":", 1)
        key = key.strip().lower()
        value = value.strip()
        if key == "user-agent":
            if rules:
                flush()
            agents.append(value)
        elif key in {"allow", "disallow"} and agents:
            rules.append((key, value))
    flush()
    return Robots(groups)


def allowed(parser: Robots, user_agent: str, url: str) -> bool:
    return parser.can_fetch(user_agent, url)


def _select_group(groups, user_agent: str) -> list[tuple[str, str]]:
    token = user_agent.split("/", 1)[0].lower()
    best: list[tuple[str, str]] | None = None
    best_len = -1
    fallback: list[tuple[str, str]] | None = None
    for agents, rules in groups:
        for agent in agents:
            if agent == "*":
                if fallback is None:
                    fallback = rules
                continue
            needle = agent.lower()
            if needle and needle in token and len(needle) > best_len:
                best = rules
                best_len = len(needle)
    if best is not None:
        return best
    return fallback or []


def _target(url: str) -> str:
    parsed = urlparse(url)
    path = unquote(parsed.path or "/")
    if parsed.query:
        return path + "?" + unquote(parsed.query)
    return path


def _matches(pattern: str, target: str) -> bool:
    if pattern == "":
        return False
    parts: list[str] = []
    for char in pattern:
        if char == "*":
            parts.append(".*")
        elif char == "$":
            parts.append("$")
        else:
            parts.append(re.escape(char))
    if not pattern.endswith("$"):
        parts.append(".*")
    return re.match("".join(parts), target) is not None
