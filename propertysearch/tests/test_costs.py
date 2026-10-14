"""Budget math for the €246,000 planning loan."""

from app.costs import budget, purchase_costs, renovation_range, surface_factor


def test_two_hundred_thousand_is_about_6400():
    costs = purchase_costs(200_000)
    assert costs["registration_duty"] == 4_000
    assert costs["notary_and_admin"] == 2_400
    assert costs["total"] == 6_400


def test_registration_is_two_percent():
    assert purchase_costs(189_000)["registration_duty"] == 3_780
    assert purchase_costs(250_000)["registration_duty"] == 5_000


def test_surface_scales_a_little_above_80():
    assert surface_factor(80) == 1
    assert surface_factor(70) == 1
    assert surface_factor(100) == 1.08
    low, high = renovation_range("to_be_done_up", 100)
    assert (low, high) == (27_000, 43_200)
    assert renovation_range("move_in", 80) == (0, 10_000)


def test_done_up_target_band_can_fit_the_loan():
    # €189,000, 102 m², to be done up: about €190k plus €30–40k of work.
    result = budget(price=189_000, surface_m2=102, band="to_be_done_up")
    assert result["purchase_costs"] == 6_147
    assert result["reno_low"] == 27_200
    assert result["reno_high"] == 43_520
    assert result["project_high"] == 238_667
    assert result["fits_loan"] is True
    assert result["over_loan"] is False
    assert result["cash_needed_high"] == 0
    assert result["headroom_low"] == 7_333


def test_high_renovation_estimate_marks_over_and_blocks_the_filter():
    result = budget(price=175_000, surface_m2=110, band="to_renovate")
    assert result["fits_loan"] is False
    assert result["over_loan"] is False
    assert result["cash_needed_low"] == 0
    assert result["cash_needed_high"] > 0


def test_casco_over_the_loan_is_still_calculable():
    result = budget(price=210_000, surface_m2=85, band="to_restore")
    assert result["over_loan"] is True
    assert result["fits_loan"] is False
    assert result["cash_needed_low"] == 42_030
    assert result["reno_low"] == 71_400


def test_price_plus_costs_can_exceed_the_loan_with_no_work():
    result = budget(price=239_000, surface_m2=88, band="move_in")
    assert result["over_loan"] is True
    assert result["cash_needed_low"] == 297


def test_unknown_renovation_does_not_pass_the_loan_filter():
    result = budget(price=205_000, surface_m2=92, band=None)
    assert result["fits_loan"] is False
    assert result["renovation_unknown"] is True
    assert result["cash_needed_low"] is None
