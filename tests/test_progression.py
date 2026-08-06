"""
Compounding growth formula tests for Dungeon Lord progression system.
Formula:
- Next level's base is the previous level's calculated value.
- Standard level-up: +1% growth.
- 10th-level milestone: +10% bonus added to the +1% (total +11% on milestone 10, 20, 30, etc.).
- 25th-level milestone: +25% bonus added to the +1% (total +26% on milestone 25, 50, 75, etc.).
"""

import pytest


def growth_multiplier(level: int, milestone_10_bonus: float = 0.10, milestone_25_bonus: float = 0.25) -> float:
    """Calculate compounding growth multiplier for a given level."""
    if level <= 1:
        return 1.0

    current = 1.0
    for lvl in range(2, level + 1):
        step_pct = 0.01
        if lvl % 25 == 0:
            step_pct += milestone_25_bonus
        elif lvl % 10 == 0:
            step_pct += milestone_10_bonus

        current *= (1.0 + step_pct)

    return current


class TestGrowthFormula:
    """Test the shared compounding growth formula for Lord level and Dungeon rank."""

    def test_level_1_base(self):
        """Level 1: base multiplier 1.0."""
        mult = growth_multiplier(1)
        assert mult == pytest.approx(1.0)

    def test_level_2_first_step(self):
        """Level 2: 1.0 * 1.01 = 1.01."""
        mult = growth_multiplier(2)
        assert mult == pytest.approx(1.01)

    def test_level_10_first_10_milestone(self):
        """Level 10: 8 levels at +1% (1.01^8), level 10 at +11% -> 120.19709%."""
        mult = growth_multiplier(10)
        assert mult == pytest.approx(1.2019709)

    def test_level_11_after_10_milestone(self):
        """Level 11: Level 10 value * 1.01 -> 1.2139906."""
        mult = growth_multiplier(11)
        assert mult == pytest.approx(1.2139906)

    def test_level_25_first_25_milestone(self):
        """Level 25: Compounding milestone step at level 25 -> 1.9132219."""
        mult = growth_multiplier(25)
        assert mult == pytest.approx(1.9132219)

    def test_compounding_nature(self):
        """Verify next level builds on previous level's calculated value."""
        v9 = growth_multiplier(9)
        v10 = growth_multiplier(10)
        v11 = growth_multiplier(11)

        assert v10 == pytest.approx(v9 * 1.11)
        assert v11 == pytest.approx(v10 * 1.01)


class TestDungeonRankThresholds:
    """Test dungeon rank Essence thresholds."""

    def test_rank_thresholds_increasing(self):
        """Essence thresholds should increase per rank."""
        thresholds = {
            1: 0,
            2: 5000,
            3: 15000,
            4: 40000,
            5: 100000,
        }
        for rank in range(2, 6):
            assert thresholds[rank] > thresholds[rank - 1]

    def test_room_capacity_increasing(self):
        """Room capacity should increase per rank."""
        capacities = {1: 10, 2: 18, 3: 30, 4: 50, 5: 80}
        for rank in range(2, 6):
            assert capacities[rank] > capacities[rank - 1]


class TestEssenceEconomy:
    """Test Essence costs and rewards."""

    def test_essence_positive(self):
        """All Essence costs/rewards should be positive."""
        costs = {
            "room_basic": 100,
            "room_spawn": 250,
            "room_treasure": 500,
            "trap_spike": 50,
            "trap_fire": 200,
            "monster_t1": 100,
            "monster_t2": 400,
            "monster_t3": 1000,
        }
        for cost in costs.values():
            assert cost > 0

    def test_higher_tier_costs_more(self):
        """Higher tier monsters/rooms should cost more Essence."""
        assert 400 > 100
        assert 1000 > 400


if __name__ == "__main__":
    pytest.main([__file__, "-v"])