"""Source catalog. Only active portals are fetched."""

ACTIVE_PORTALS = (
    {
        "id": "immoweb",
        "name": "Immoweb",
        "rank": 1,
        "home": "https://www.immoweb.be",
        "note": "First source. Public search pages embed structured results.",
    },
    {
        "id": "zimmo",
        "name": "Zimmo",
        "rank": 2,
        "home": "https://www.zimmo.be",
        "note": "Second source, for ads Immoweb misses, plus days on market when present.",
    },
    {
        "id": "immovlan",
        "name": "Immovlan",
        "rank": 3,
        "home": "https://immovlan.be",
        "note": "Third source.",
    },
)

# Listed so they are not forgotten. The collector does not request them.
LATER_PORTALS = (
    {"id": "logic-immo", "name": "Logic-Immo", "home": "https://www.logic-immo.be"},
    {"id": "realo", "name": "Realo", "home": "https://www.realo.be"},
    {"id": "era", "name": "ERA", "home": "https://www.era.be"},
    {"id": "century21", "name": "Century 21", "home": "https://www.century21.be"},
    {"id": "trevi", "name": "Trevi", "home": "https://www.trevi.be"},
)

OUT_OF_SCOPE = (
    {
        "id": "biddit",
        "name": "Biddit / notary sales",
        "reason": "Public notary sales are out of scope for this search.",
    },
)
