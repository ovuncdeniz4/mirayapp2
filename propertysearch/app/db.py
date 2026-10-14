"""SQLite snapshot. The database file stays out of git."""

from __future__ import annotations

import json
import sqlite3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DEFAULT_DB = ROOT / "data" / "listings.sqlite"

SCHEMA = """
CREATE TABLE IF NOT EXISTS listings (
    id INTEGER PRIMARY KEY,
    dedupe_key TEXT NOT NULL UNIQUE,
    is_sample INTEGER NOT NULL DEFAULT 0,
    price INTEGER,
    postcode TEXT,
    municipality TEXT,
    street TEXT,
    street_norm TEXT,
    surface_m2 REAL,
    bedrooms INTEGER,
    epc_label TEXT,
    construction_year INTEGER,
    building_condition TEXT,
    title TEXT,
    description TEXT,
    latitude REAL,
    longitude REAL,
    photo_urls TEXT NOT NULL DEFAULT '[]',
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS listing_sources (
    id INTEGER PRIMARY KEY,
    listing_id INTEGER NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
    portal TEXT NOT NULL,
    url TEXT NOT NULL,
    external_id TEXT,
    days_on_market INTEGER,
    UNIQUE(listing_id, portal, url)
);

CREATE TABLE IF NOT EXISTS shortlist (
    listing_id INTEGER PRIMARY KEY REFERENCES listings(id) ON DELETE CASCADE,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS ingest_runs (
    id INTEGER PRIMARY KEY,
    started_at TEXT NOT NULL,
    finished_at TEXT,
    summary_json TEXT NOT NULL
);
"""


def connect(path: Path | None = None) -> sqlite3.Connection:
    db_path = Path(path) if path else DEFAULT_DB
    db_path.parent.mkdir(parents=True, exist_ok=True)
    conn = sqlite3.connect(db_path)
    conn.row_factory = sqlite3.Row
    conn.execute("PRAGMA foreign_keys = ON")
    conn.executescript(SCHEMA)
    return conn


def listing_count(conn: sqlite3.Connection, *, sample: int | None = None) -> int:
    if sample is None:
        row = conn.execute("SELECT COUNT(*) AS n FROM listings").fetchone()
    else:
        row = conn.execute(
            "SELECT COUNT(*) AS n FROM listings WHERE is_sample = ?",
            (sample,),
        ).fetchone()
    return int(row["n"])
