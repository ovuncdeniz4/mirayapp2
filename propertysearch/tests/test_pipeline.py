"""Parsers, robots-safe URLs, dedupe, and labeled samples."""

from app.db import connect
from app.present import present
from ingest.dedupe import dedupe_key, normalise_street
from ingest.filters import passes_search
from ingest.models import ListingDraft
from ingest.parse_immoweb import SEARCH_BASE, candidate_urls, parse_document as parse_immoweb
from ingest.parse_immovlan import parse_document as parse_immovlan
from ingest.parse_zimmo import parse_document as parse_zimmo
from ingest.pipeline import run_ingest, upsert_draft
from ingest.robots import allowed, parser_from_text


IMMOWEB_HTML = """
<iw-search :results='[{
  "id": 42,
  "property": {
    "type": "APARTMENT",
    "title": "Appartement",
    "bedroomCount": 2,
    "netHabitableSurface": 91,
    "location": {
      "postalCode": "2600",
      "locality": "Berchem",
      "street": "Voorbeeldstraat",
      "number": "4",
      "latitude": 51.2,
      "longitude": 4.4
    },
    "building": {"condition": "GOOD", "constructionYear": "1998"}
  },
  "transaction": {"type": "FOR_SALE", "price": {"mainValue": 210000}},
  "description": "Instapklaar."
}]'></iw-search>
"""

ZIMMO_HTML = """
<script type="application/json">
{"listings":[{
  "code": "Z1",
  "price": 199000,
  "bedrooms": 2,
  "livableSpace": 90,
  "address": {"postalCode": "2300", "locality": "Turnhout", "street": "Test", "number": "1"},
  "daysOnline": 12,
  "url": "https://www.zimmo.be/nl/z1/",
  "title": "Te renoveren",
  "description": "Te renoveren appartement",
  "condition": "TO_RENOVATE"
}]}
</script>
"""

IMMOVLAN_HTML = """
<script type="application/ld+json">
{
  "@type": "ItemList",
  "itemListElement": [{
    "@type": "ListItem",
    "item": {
      "@type": "Apartment",
      "name": "Appartement te koop",
      "url": "https://immovlan.be/nl/detail/appartement/te-koop/2500/lier/SAMPLE1",
      "description": "Te renoveren appartement",
      "address": {
        "postalCode": "2500",
        "addressLocality": "Lier",
        "streetAddress": "Voorbeeldstraat 1"
      },
      "numberOfBedrooms": 2,
      "floorSize": {"value": 90},
      "offers": {"price": 180000}
    }
  }]
}
</script>
"""


def test_immoweb_urls_do_not_use_disallowed_filters():
    urls = candidate_urls()
    assert urls[0] == SEARCH_BASE
    for url in urls:
        lowered = url.lower()
        assert "maxprice" not in lowered
        assert "countries=" not in lowered
        assert "orderby" not in lowered
    robots = "User-agent: *\nDisallow: *maxprice*\nDisallow: *countries=*\nDisallow: *orderBy*\n"
    parser = parser_from_text(robots)
    assert allowed(parser, "Mozilla/5.0", SEARCH_BASE)
    assert allowed(parser, "Mozilla/5.0", SEARCH_BASE + "?page=2")
    assert not allowed(parser, "Mozilla/5.0", SEARCH_BASE + "?countries=BE&maxprice=250000")


def test_immoweb_embedded_results():
    drafts = parse_immoweb(IMMOWEB_HTML)
    assert len(drafts) == 1
    draft = drafts[0]
    assert draft.price == 210_000
    assert draft.postcode == "2600"
    assert draft.bedrooms == 2
    assert draft.surface_m2 == 91
    assert draft.building_condition == "GOOD"
    assert draft.street == "Voorbeeldstraat 4"
    assert passes_search(draft)


def test_zimmo_keeps_days_on_market():
    drafts = parse_zimmo(ZIMMO_HTML)
    assert len(drafts) == 1
    assert drafts[0].days_on_market == 12
    assert drafts[0].postcode == "2300"
    assert drafts[0].portal == "zimmo"


IMMOVLAN_CARD = """
<article class="v3-search-card" data-url="https://immovlan.be/nl/detail/appartement/te-koop/2500/lier/SAMPLE1">
  <span class="v3-search-card-price">189&#x202F;000 &#x20AC;</span>
  <span itemprop="postalCode">2500</span>
  <span itemprop="addressLocality">Lier</span>
  <p itemprop="description">Te renoveren appartement met twee slaapkamers.</p>
  <span class="v3-search-card-pill"><strong>2</strong> Slaapkamer(s)</span>
  <span class="v3-search-card-pill"><strong>96</strong> m&#xB2;</span>
  <div class="v3-epc-watermark FlandersD"></div>
  <img data-src="https://example.test/photo.jpg" />
</article>
"""


def test_immovlan_search_card():
    drafts = parse_immovlan(IMMOVLAN_CARD)
    assert len(drafts) == 1
    assert drafts[0].price == 189_000
    assert drafts[0].surface_m2 == 96
    assert drafts[0].bedrooms == 2
    assert drafts[0].epc_label == "D"
    assert drafts[0].postcode == "2500"
    assert "te renoveren" in (drafts[0].description or "").lower()


def test_immovlan_json_ld():
    drafts = parse_immovlan(IMMOVLAN_HTML)
    assert len(drafts) == 1
    assert drafts[0].price == 180_000
    assert drafts[0].municipality == "Lier"
    assert drafts[0].bedrooms == 2
    assert "te renoveren" in (drafts[0].description or "").lower()


def test_search_filter_rejects_the_wrong_province_and_type():
    ok = ListingDraft(
        portal="immoweb",
        external_id="1",
        url="https://example.test/1",
        price=200_000,
        postcode="2000",
        street="Voorbeeldstraat 1",
        surface_m2=90,
        bedrooms=2,
        property_type="APARTMENT",
        transaction_type="FOR_SALE",
    )
    assert passes_search(ok)
    brussels = ListingDraft(**{**ok.__dict__, "postcode": "1000", "external_id": "2"})
    house = ListingDraft(**{**ok.__dict__, "property_type": "HOUSE", "external_id": "3"})
    rent = ListingDraft(**{**ok.__dict__, "transaction_type": "FOR_RENT", "external_id": "4"})
    assert not passes_search(brussels)
    assert not passes_search(house)
    assert not passes_search(rent)


def test_street_normalisation_and_dedupe_key():
    assert normalise_street("Kerkstr. 12") == "kerkstraat 12"
    left = ListingDraft(
        portal="immoweb",
        external_id="a",
        url="https://example.test/a",
        price=189_000,
        postcode="2500",
        street="Monsterstraat 4",
        surface_m2=102,
        bedrooms=3,
    )
    right = ListingDraft(
        portal="zimmo",
        external_id="b",
        url="https://example.test/b",
        price=189_000,
        postcode="2500",
        street="monsterstraat 4",
        surface_m2=102.2,
        bedrooms=3,
    )
    assert dedupe_key(left) == dedupe_key(right)


def test_upsert_keeps_every_source_link(tmp_path):
    conn = connect(tmp_path / "t.sqlite")
    first = ListingDraft(
        portal="immoweb",
        external_id="a",
        url="https://www.immoweb.be/en/classified/a",
        price=189_000,
        postcode="2500",
        municipality="Lier",
        street="Monsterstraat 4",
        surface_m2=102,
        bedrooms=3,
        building_condition="TO_BE_DONE_UP",
        title="Op te frissen",
        description="Op te frissen",
        property_type="APARTMENT",
        transaction_type="FOR_SALE",
        photo_urls=["https://example.test/1.jpg"],
    )
    second = ListingDraft(
        portal="zimmo",
        external_id="b",
        url="https://www.zimmo.be/nl/b/",
        price=189_000,
        postcode="2500",
        municipality="Lier",
        street="Monsterstraat 4",
        surface_m2=102,
        bedrooms=3,
        days_on_market=21,
        property_type="APARTMENT",
        transaction_type="FOR_SALE",
        photo_urls=["https://example.test/2.jpg"],
    )
    assert upsert_draft(conn, first, is_sample=False) == "inserted"
    assert upsert_draft(conn, second, is_sample=False) == "merged"
    row = conn.execute("SELECT id FROM listings").fetchone()
    sources = conn.execute(
        "SELECT portal, url, days_on_market FROM listing_sources WHERE listing_id = ? ORDER BY portal",
        (row["id"],),
    ).fetchall()
    assert [source["portal"] for source in sources] == ["immoweb", "zimmo"]
    assert sources[1]["days_on_market"] == 21
    conn.close()


def test_samples_are_labeled_and_cover_every_lane(tmp_path):
    db_path = tmp_path / "sample.sqlite"
    summary = run_ingest(db_path, use_network=False, read_saved_pages=False)
    assert summary["live_listings"] == 0
    assert summary["sample_listings"] >= 8
    assert summary["seeded_samples"] is True
    conn = connect(db_path)
    flags = {row["is_sample"] for row in conn.execute("SELECT is_sample FROM listings")}
    assert flags == {1}
    cards = []
    for row in conn.execute("SELECT * FROM listings"):
        sources = conn.execute(
            "SELECT * FROM listing_sources WHERE listing_id = ?",
            (row["id"],),
        ).fetchall()
        cards.append(present(row, sources, False))
    lanes = {card["lane"] for card in cards}
    assert lanes == {"move_in", "renovate", "review"}
    merged = next(card for card in cards if card["street"] == "Monsterstraat 4")
    assert {source["portal"] for source in merged["sources"]} == {"immoweb", "zimmo"}
    assert any(source["days_on_market"] == 21 for source in merged["sources"])
    review = next(card for card in cards if card["lane"] == "review")
    assert len(review["photos"]) == 3
    assert all(card["is_sample"] for card in cards)
    conn.close()
