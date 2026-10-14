# Antwerp apartment search

Local dashboard for apartments for sale in the province of Antwerp. It stays on your machine. Nothing here is published, and nothing here is mortgage or tax advice.

The search is: apartment, for sale, postcodes 2000–2999, asking price at most €250,000, at least 2 bedrooms, at least 80 m². The loan ceiling used on the cards is €246,000.

Two tracks:

- **Move-in** — condition as new, just renovated, or good, unless the text says it still has to be renovated. Planning allowance about €0–€10,000 (a dated kitchen or bathroom is shown higher).
- **Renovate** — to be done up, to renovate, to restore, or casco. These stay visible when price + work is over the loan, and the card says so. The useful band is about €190,000 plus €30,000–€40,000 of work.

Purchase costs are shown on every card. The planning rule is 2% registration duty (Flanders sole primary home, 2026 compromis, if you qualify) plus notary and file costs of €1,800 + 0.3% of the asking price. On €200,000 that is €4,000 + €2,400, about €6,400. A fuller notary quote is often €4,000–€4,500; the cards use the €6,400 rule so you can check the arithmetic. Cash still needed is what is left after applying the €246,000 loan to price + renovation + those costs.

## Install

From this folder:

```bash
cd propertysearch
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
```

## Open the dashboard

```bash
python run.py
```

Then open http://127.0.0.1:8765

That uses the last SQLite snapshot in `data/listings.sqlite`. The database is gitignored. If there is no snapshot yet, the command loads the sample fixtures so the lanes, filters, and shortlist can be tried. Sample cards are labeled **Sample** and are not mixed in as live listings.

Search keeps working from that snapshot with the network off. Map tiles still come from OpenStreetMap when you are online. Leaflet itself is stored in `app/static/vendor/`.

## Refresh listings

```bash
python run.py --ingest
```

This reads each portal's `robots.txt`, requests only URLs that file allows, waits a few seconds between pages, and stops that portal when the site returns a block or a challenge. It does not log in, solve a captcha, or use a scraper account.

Active sources, in order: Immoweb, Zimmo, Immovlan. Logic-Immo, Realo, ERA, Century 21, and Trevi are listed in `ingest/catalog.py` and are not fetched. Biddit and notary sales are out of scope.

Immoweb's robots.txt disallows search URLs that contain `maxprice`, `countries=`, or `orderBy`, so the collector opens the province path and applies price, bedrooms, and surface locally. The same idea is used on the other two sites when a filter parameter is disallowed.

If a portal blocks this machine, the run log (printed in the terminal and under **Last ingest** on the dashboard) counts it and the other portals still run. Listings already in the snapshot stay there.

### If the portals block you

Save the search page from your own browser (the normal page, after it has loaded) and put the file in `data/inbox/`:

- `immoweb-….html` or `.json`
- `zimmo-….html` or `.json`
- `immovlan-….html` or `.json`

Then run `python run.py --ingest` again. Saved pages are read as real listings. Do not drop the sample fixtures in that folder.

To force the labeled samples back in:

```bash
python run.py --sample
```

## What a fetch does

Duplicates across sites collapse on postcode + normalised street + surface + price. Every source link is kept. Zimmo days-on-market are kept when the page includes them.

Classification order:

1. Building condition (`as new`, `just renovated`, `good`, `to be done up`, `to renovate`, `to restore`, `casco`).
2. Dutch and French keywords in the title and description (`instapklaar`, `gerenoveerd`, `nieuw`, `te renoveren`, `casco`, and close variants of `keuken` / `badkamer`).
3. EPC and construction year are shown as context only. They never move a card to another lane.

Renovation bands, scaled a little when the apartment is larger than 80 m²:

| Band | Planning range |
| --- | --- |
| Move-in | €0–€10,000 |
| Dated but livable | €15,000–€25,000 |
| To be done up | €25,000–€40,000 |
| To renovate | €40,000–€70,000 |
| To restore or casco | €70,000 and up |

The filter **Price + renovation + costs within €246,000** uses the high end of that range. Review cards have no renovation figure, so they do not pass that filter. The shortlist is a tick on the card and is stored in SQLite.

## What the portals did on 14 October 2026

From this environment, one polite run:

- **Immoweb** served the province search (HTTP 200). Price, bedrooms, and surface were applied locally because `maxprice`, `countries=`, and `orderBy` are disallowed in robots.txt. Listing pages were opened, with a pause, only for results that already matched, and most of those pages included `buildingCondition`.
- **Zimmo** was not fetched. Its robots.txt responded with HTTP 403, so the collector stopped for that site instead of working around the refusal.
- **Immovlan** served the apartment search (HTTP 200), including the `maxprice=250000` and `minbedrooms=2` query, which robots.txt still allows. Those parameters are lowercase; the disallowed spellings are `maxPrice` and `minBedrooms`.

Logic-Immo, Realo, ERA, Century 21, and Trevi were not requested. If a later run is blocked, the previous SQLite snapshot stays usable, and `data/inbox/` is there for a page saved from a browser. If the snapshot is empty, `python run.py` loads the labeled samples.

## Tests

```bash
pytest
```

## Layout

- `ingest/` — portal catalog and fetchers
- `app/` — classification, cost estimate, FastAPI UI
- `data/` — SQLite snapshot (gitignored) and sample fixtures
- `tests/` — budget math and move-in / renovate rules
