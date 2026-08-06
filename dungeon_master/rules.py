"""Core game rules and shared leveling formula for Dungeon Lord."""

def calculate_milestone_bonus(level: int) -> float:
    """Calculates milestone percentage bonus.
    - Every 10th level: +10%
    - Every 25th level: +25% (replaces 10-level milestone on shared levels e.g. 50, 75)
    """
    bonus = 0.0
    for lvl in range(1, level + 1):
        if lvl % 25 == 0:
            bonus += 0.25
        elif lvl % 10 == 0:
            bonus += 0.10
    return bonus


def calculate_attribute(base_value: float, level: int) -> float:
    """Shared percentage-based growth formula for Dungeon Lord and Dungeon Rank:
    attribute = base * (1 + 0.01 * (level - 1) + milestone_bonus(level))
    """
    if level <= 1:
        return base_value
    linear_factor = 0.01 * (level - 1)
    milestone_bonus = calculate_milestone_bonus(level)
    return base_value * (1.0 + linear_factor + milestone_bonus)


def calculate_invader_essence_reward(invader_level: int, invader_count: int, settlement_reputation: int) -> int:
    """Calculates Essence yield upon defeating an invader party."""
    base_essence = 25 * invader_count * invader_level
    rep_multiplier = 1.0 + (settlement_reputation / 100.0)
    return int(base_essence * rep_multiplier)
