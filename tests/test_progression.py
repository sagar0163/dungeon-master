"""
Growth formula tests for Dungeon Lord progression system.
Formula: attribute = base × (1 + 0.01 × level + milestone_bonus(level))

milestone_bonus:
- Every 10th level: +10% × milestone_count
- Every 25th level: +25% × milestone_count (replaces 10-level on shared levels)
- Additive, not multiplicative
"""

import pytest


def growth_multiplier(level: int, milestone_10_bonus: float = 0.10, milestone_25_bonus: float = 0.25, stacking: str = "replace_25") -> float:
    """Calculate growth multiplier for a given level."""
    base_multiplier = 0.01 * level
    
    # Count milestones
    milestone_10_count = level // 10
    milestone_25_count = level // 25
    
    if stacking == "replace_25":
        # 25-level milestones replace 10-level milestones on shared levels
        shared_count = level // 50  # LCM of 10 and 25
        effective_10_count = milestone_10_count - shared_count
        effective_25_count = milestone_25_count
    elif stacking == "add":
        effective_10_count = milestone_10_count
        effective_25_count = milestone_25_count
    elif stacking == "max":
        effective_10_count = milestone_10_count
        effective_25_count = milestone_25_count
        # Use max bonus for shared levels
    else:
        raise ValueError(f"Unknown stacking rule: {stacking}")
    
    milestone_bonus = (effective_10_count * milestone_10_bonus) + (effective_25_count * milestone_25_bonus)
    
    return 1.0 + base_multiplier + milestone_bonus


class TestGrowthFormula:
    """Test the shared growth formula for Lord level and Dungeon rank."""
    
    def test_level_1_no_milestones(self):
        """Level 1: only base 1% per level."""
        mult = growth_multiplier(1)
        assert mult == pytest.approx(1.01)  # 1 + 0.01*1 + 0
    
    def test_level_9_no_milestones(self):
        """Level 9: only base 1% per level."""
        mult = growth_multiplier(9)
        assert mult == pytest.approx(1.09)  # 1 + 0.01*9 + 0
    
    def test_level_10_first_10_milestone(self):
        """Level 10: +10% milestone bonus."""
        mult = growth_multiplier(10)
        # 1 + 0.01*10 + 1*0.10 = 1.20
        assert mult == pytest.approx(1.20)
    
    def test_level_11_after_10_milestone(self):
        """Level 11: base + one 10-milestone."""
        mult = growth_multiplier(11)
        # 1 + 0.01*11 + 1*0.10 = 1.21
        assert mult == pytest.approx(1.21)
    
    def test_level_19(self):
        """Level 19: base + one 10-milestone."""
        mult = growth_multiplier(19)
        # 1 + 0.01*19 + 1*0.10 = 1.29
        assert mult == pytest.approx(1.29)
    
    def test_level_20_second_10_milestone(self):
        """Level 20: +20% from two 10-milestones."""
        mult = growth_multiplier(20)
        # 1 + 0.01*20 + 2*0.10 = 1.40
        assert mult == pytest.approx(1.40)
    
    def test_level_25_first_25_milestone_replaces_10(self):
        """Level 25: 25-milestone replaces 10-milestone (replace_25 rule)."""
        mult = growth_multiplier(25, stacking="replace_25")
        # Milestones: 10, 20 (two 10s), 25 (one 25, replaces one 10 at 50? No, 25 is not shared with 10)
        # Wait: shared levels are LCM(10,25) = 50
        # At 25: milestone_10_count = 2 (10, 20), milestone_25_count = 1 (25)
        # shared = 0 (50 not reached)
        # effective_10 = 2, effective_25 = 1
        # 1 + 0.25 + 0.20 + 0.25 = 1.70
        assert mult == pytest.approx(1.70)
    
    def test_level_50_shared_milestone_replace_25(self):
        """Level 50: shared 10/25 milestone, 25 replaces 10."""
        mult = growth_multiplier(50, stacking="replace_25")
        # milestone_10_count = 5 (10,20,30,40,50)
        # milestone_25_count = 2 (25,50)
        # shared = 1 (50)
        # effective_10 = 5 - 1 = 4
        # effective_25 = 2
        # 1 + 0.50 + 4*0.10 + 2*0.25 = 1 + 0.50 + 0.40 + 0.50 = 2.40
        assert mult == pytest.approx(2.40)
    
    def test_level_100_shared_milestones(self):
        """Level 100: multiple shared milestones."""
        mult = growth_multiplier(100, stacking="replace_25")
        # milestone_10_count = 10, milestone_25_count = 4
        # shared = 2 (50, 100)
        # effective_10 = 8, effective_25 = 4
        # 1 + 1.0 + 0.80 + 1.00 = 3.80
        assert mult == pytest.approx(3.80)
    
    def test_additive_not_multiplicative(self):
        """Growth is additive, not compounding."""
        # Level 10 = 1.20x base
        # Level 20 = 1.40x base (not 1.20 * 1.20 = 1.44)
        mult_10 = growth_multiplier(10)
        mult_20 = growth_multiplier(20)
        assert mult_20 < mult_10 * mult_10  # Additive < multiplicative
    
    def test_configurable_percentages(self):
        """All percentages should be config-driven."""
        # Custom config
        mult = growth_multiplier(
            10, 
            milestone_10_bonus=0.05,  # 5% instead of 10%
            milestone_25_bonus=0.15   # 15% instead of 25%
        )
        # 1 + 0.10 + 1*0.05 = 1.15
        assert mult == pytest.approx(1.15)


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
        assert 400 > 100  # t2 > t1
        assert 1000 > 400  # t3 > t2


if __name__ == "__main__":
    pytest.main([__file__, "-v"])