import pytest
from dungeon_master.rules import calculate_attribute, calculate_milestone_bonus, calculate_invader_essence_reward
from dungeon_master.models import DungeonLord, DungeonRank


def test_level_1_attribute():
    assert calculate_attribute(100.0, 1) == 100.0


def test_level_10_attribute():
    # Level 10: linear = 0.01 * 9 = 0.09, milestone = 0.10 -> total 1 + 0.09 + 0.10 = 1.19
    val = calculate_attribute(100.0, 10)
    assert abs(val - 119.0) < 1e-5


def test_level_25_attribute():
    # Level 25: linear = 0.24, milestones: lvl 10 (+0.10), lvl 20 (+0.10), lvl 25 (+0.25) -> total = 1.69 multiplier
    val = calculate_attribute(100.0, 25)
    assert abs(val - 169.0) < 1e-5


def test_dungeon_lord_hp_scaling():
    lord = DungeonLord(level=1, hp_base=100.0)
    assert lord.hp_max == 100.0

    lord.level = 10
    assert abs(lord.hp_max - 119.0) < 1e-5


def test_invader_essence_reward():
    reward = calculate_invader_essence_reward(invader_level=2, invader_count=3, settlement_reputation=20)
    # base = 25 * 3 * 2 = 150. rep mult = 1.20 -> 180
    assert reward == 180
