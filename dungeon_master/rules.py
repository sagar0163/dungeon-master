"""Core game rules and compounding leveling formula for Dungeon Lord."""

def calculate_attribute(base_value: float, level: int) -> float:
    """Compounding percentage-based growth formula for Dungeon Lord and Dungeon Rank:
    - Next level's base is the previous level's calculated value.
    - Standard level-up: +1% growth.
    - 10th-level milestone: +10% bonus added to the +1% (total +11% on milestone 10, 20, 30, etc.).
    - 25th-level milestone: +25% bonus added to the +1% (total +26% on milestone 25, 50, 75, etc., replacing 10th bonus).
    - Milestone bonuses apply only on that specific level step.
    """
    if level <= 1:
        return base_value

    current = float(base_value)
    for lvl in range(2, level + 1):
        step_pct = 0.01
        if lvl % 25 == 0:
            step_pct += 0.25
        elif lvl % 10 == 0:
            step_pct += 0.10
        current *= (1.0 + step_pct)

    return current


def calculate_invader_essence_reward(invader_level: int, invader_count: int, settlement_reputation: int) -> int:
    """Calculates Essence yield upon defeating an invader party."""
    base_essence = 25 * invader_count * invader_level
    rep_multiplier = 1.0 + (settlement_reputation / 100.0)
    return int(base_essence * rep_multiplier)
