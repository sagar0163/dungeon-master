import pytest
from dungeon_master.dice import DiceEngine


def test_standard_dice_roll():
    engine = DiceEngine(seed=42)
    res = engine.roll("1d20+5")
    assert res.expression == "1d20+5"
    assert res.modifier == 5
    assert 6 <= res.total <= 25


def test_keep_highest_roll():
    engine = DiceEngine(seed=42)
    res = engine.roll("4d6kh3")
    assert len(res.rolls) == 4
    assert 3 <= res.total <= 18


def test_advantage_roll():
    engine = DiceEngine(seed=123)
    res = engine.roll("1d20adv+3")
    assert res.advantage_mode == "advantage"
    assert len(res.rolls) == 2
    assert res.total == max(res.rolls) + 3


def test_disadvantage_roll():
    engine = DiceEngine(seed=123)
    res = engine.roll("1d20dis+3")
    assert res.advantage_mode == "disadvantage"
    assert len(res.rolls) == 2
    assert res.total == min(res.rolls) + 3


def test_deterministic_seed_replay():
    e1 = DiceEngine(seed=999)
    e2 = DiceEngine(seed=999)

    r1 = e1.roll("2d6+3")
    r2 = e2.roll("2d6+3")

    assert r1.total == r2.total
    assert r1.rolls == r2.rolls
