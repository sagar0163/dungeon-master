import pytest
from dungeon_master.rules import calculate_attribute, calculate_invader_essence_reward
from dungeon_master.models import DungeonLord, DungeonRank


def test_level_1_attribute():
    assert calculate_attribute(100.0, 1) == 100.0


def test_level_10_attribute():
    # 8 levels @ +1% (1.01^8 = 1.0828567), level 10 @ +11% -> 120.19709
    val = calculate_attribute(100.0, 10)
    assert abs(val - 120.19709) < 1e-4


def test_level_25_attribute():
    # Compounding levels with 10th and 25th milestone bursts -> 191.32219
    val = calculate_attribute(100.0, 25)
    assert abs(val - 191.32219) < 1e-4


def test_dungeon_lord_hp_scaling():
    lord = DungeonLord(level=1, hp_base=100.0)
    assert lord.hp_max == 100.0

    lord.level = 10
    assert abs(lord.hp_max - 120.19709) < 1e-4


def test_invader_essence_reward():
    reward = calculate_invader_essence_reward(invader_level=2, invader_count=3, settlement_reputation=20)
    assert reward == 180
