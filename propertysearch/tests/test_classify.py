"""Renovate versus move-in rules."""

from app.classify import classify


def _lane(**kwargs):
    return classify(
        building_condition=kwargs.get("building_condition"),
        title=kwargs.get("title"),
        description=kwargs.get("description"),
        epc_label=kwargs.get("epc_label"),
        construction_year=kwargs.get("construction_year"),
    )


def test_as_new_and_good_are_move_in():
    as_new = _lane(building_condition="AS_NEW", title="Appartement", description="Zo goed als nieuw.")
    good = _lane(building_condition="GOOD", title="Appartement", description="Instapklaar appartement.")
    assert as_new.lane == "move_in" and as_new.band == "move_in"
    assert good.lane == "move_in" and good.band == "move_in"


def test_just_renovated_is_move_in():
    result = _lane(building_condition="JUST_RENOVATED", description="Recent gerenoveerd.")
    assert result.lane == "move_in"
    assert result.band == "move_in"


def test_good_condition_with_te_renoveren_goes_to_renovate():
    result = _lane(
        building_condition="GOOD",
        description="Het appartement is te renoveren.",
    )
    assert result.lane == "renovate"
    assert result.band == "to_renovate"


def test_condition_beats_conflicting_keywords():
    result = _lane(
        building_condition="TO_RENOVATE",
        title="Instapklaar",
        description="Instapklaar staat er niet.",
    )
    assert result.lane == "renovate"
    assert result.band == "to_renovate"


def test_done_up_restore_and_casco_bands():
    done = _lane(building_condition="TO_BE_DONE_UP")
    restore = _lane(building_condition="TO_RESTORE")
    casco = _lane(building_condition="CASCO", description="Instapklaar")
    assert (done.lane, done.band) == ("renovate", "to_be_done_up")
    assert (restore.lane, restore.band) == ("renovate", "to_restore")
    assert (casco.lane, casco.band) == ("renovate", "to_restore")


def test_keywords_when_condition_is_missing():
    move_in = _lane(title="Instapklaar appartement", description="Volledig gerenoveerd.")
    renovate = _lane(description="Te renoveren. De keuken is oud.")
    refresh = _lane(description="Op te frissen, verder bewoonbaar.")
    assert move_in.lane == "move_in"
    assert renovate.lane == "renovate" and renovate.band == "to_renovate"
    assert refresh.lane == "renovate" and refresh.band == "to_be_done_up"


def test_bare_kitchen_and_bathroom_stay_in_review():
    result = _lane(description="Er is een keuken en een badkamer. Berging op de gang.")
    assert result.lane == "review"
    assert result.band is None


def test_dated_kitchen_raises_the_band_without_leaving_move_in():
    result = _lane(
        building_condition="GOOD",
        description="De keuken is gedateerd en de badkamer is te vernieuwen.",
    )
    assert result.lane == "move_in"
    assert result.band == "dated_livable"


def test_french_a_renover_is_renovate():
    result = _lane(description="Appartement à rénover, cuisine à rénover.")
    assert result.lane == "renovate"
    assert result.band == "to_renovate"


def test_epc_and_year_do_not_change_the_lane():
    blocked = _lane(
        building_condition="TO_RENOVATE",
        epc_label="A",
        construction_year=2020,
        description="Te renoveren.",
    )
    review = _lane(
        description="Rustige ligging nabij het park.",
        epc_label="A",
        construction_year=2024,
    )
    assert blocked.lane == "renovate"
    assert "does not change the lane" in blocked.supporting_note
    assert review.lane == "review"
    assert "EPC A" in review.supporting_note
    assert "2024" in review.supporting_note
