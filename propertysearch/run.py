"""Start the localhost dashboard, using the last snapshot or a fresh ingest."""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from app.db import DEFAULT_DB, connect, listing_count  # noqa: E402
from app.server import create_app  # noqa: E402
from ingest.pipeline import run_ingest  # noqa: E402


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Antwerp apartment search. Serves http://127.0.0.1 only.",
    )
    parser.add_argument(
        "--ingest",
        action="store_true",
        help="Fetch Immoweb, Zimmo, and Immovlan, then open the dashboard.",
    )
    parser.add_argument(
        "--sample",
        action="store_true",
        help="Load the labeled sample fixtures (they stay marked as samples).",
    )
    parser.add_argument(
        "--no-serve",
        action="store_true",
        help="Ingest or seed, then exit without starting the server.",
    )
    parser.add_argument("--port", type=int, default=8765)
    parser.add_argument("--db", type=Path, default=DEFAULT_DB)
    args = parser.parse_args()

    db_path = args.db if args.db.is_absolute() else ROOT / args.db
    if args.ingest or args.sample or _listing_count(db_path) == 0:
        summary = run_ingest(
            db_path,
            use_network=args.ingest,
            seed_samples_if_empty=True,
            force_samples=args.sample,
        )
        _print_summary(summary)
    else:
        print(f"Using the snapshot at {db_path}.")
    if args.no_serve:
        return

    import uvicorn

    app = create_app(db_path)
    print(f"Dashboard: http://127.0.0.1:{args.port}")
    uvicorn.run(app, host="127.0.0.1", port=args.port, log_level="info")


def _listing_count(db_path) -> int:
    if not db_path.exists():
        return 0
    conn = connect(db_path)
    try:
        return listing_count(conn)
    finally:
        conn.close()


def _print_summary(summary: dict) -> None:
    print(
        f"Snapshot: {summary.get('live_listings', 0)} live, "
        f"{summary.get('sample_listings', 0)} sample."
    )
    if summary.get("seeded_samples"):
        print("Loaded labeled sample fixtures because there was no snapshot yet.")
    for source in summary.get("sources") or []:
        print(
            f"  {source.get('portal')}: {source.get('status')} "
            f"(HTTP {source.get('http_status')}) — {source.get('detail')}"
        )
    if not summary.get("sources"):
        print("No portal fetch on this run. Use: python run.py --ingest")


if __name__ == "__main__":
    main()
