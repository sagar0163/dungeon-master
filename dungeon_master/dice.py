"""Deterministic Dice Engine for D&D 5e SRD rules.

Supports expressions like:
- "1d20+5"
- "2d6+3"
- "1d20adv" / "1d20dis"
- "4d6kh3" (drop lowest)
- "d20"
"""

import random
import re
from typing import List, Optional, Tuple
from pydantic import BaseModel, Field


class RollResult(BaseModel):
    expression: str
    total: int
    rolls: List[int]
    modifier: int = 0
    advantage_mode: Optional[str] = None  # "advantage", "disadvantage", None
    is_crit: bool = False
    is_fumble: bool = False


class DiceEngine:
    def __init__(self, seed: Optional[int] = None):
        self._rng = random.Random(seed)

    def set_seed(self, seed: Optional[int]) -> None:
        self._rng = random.Random(seed)

    def roll(self, expression: str, seed: Optional[int] = None) -> RollResult:
        """Roll dice according to standard notation or special modifiers."""
        if seed is not None:
            rng = random.Random(seed)
        else:
            rng = self._rng

        expr = expression.strip().lower()
        adv_mode = None

        if "adv" in expr:
            adv_mode = "advantage"
            expr = expr.replace("adv", "")
        elif "dis" in expr:
            adv_mode = "disadvantage"
            expr = expr.replace("dis", "")

        # Parse keep highest notation e.g., 4d6kh3
        kh_match = re.match(r"^(\d*)d(\d+)kh(\d+)([+-]\d+)?$", expr)
        if kh_match:
            count = int(kh_match.group(1)) if kh_match.group(1) else 1
            sides = int(kh_match.group(2))
            keep = int(kh_match.group(3))
            mod = int(kh_match.group(4)) if kh_match.group(4) else 0

            raw_rolls = [rng.randint(1, sides) for _ in range(count)]
            sorted_rolls = sorted(raw_rolls, reverse=True)[:keep]
            total = sum(sorted_rolls) + mod
            return RollResult(
                expression=expression,
                total=total,
                rolls=raw_rolls,
                modifier=mod,
                advantage_mode=adv_mode,
            )

        # Standard notation e.g., 1d20+5 or d20
        match = re.match(r"^(\d*)d(\d+)([+-]\d+)?$", expr)
        if not match:
            # Simple integer modifier fallback
            try:
                val = int(expr)
                return RollResult(expression=expression, total=val, rolls=[val], modifier=val)
            except ValueError:
                raise ValueError(f"Invalid dice expression: {expression}")

        count = int(match.group(1)) if match.group(1) else 1
        sides = int(match.group(2))
        mod = int(match.group(3)) if match.group(3) else 0

        if adv_mode and count == 1 and sides == 20:
            roll1 = rng.randint(1, 20)
            roll2 = rng.randint(1, 20)
            chosen = max(roll1, roll2) if adv_mode == "advantage" else min(roll1, roll2)
            total = chosen + mod
            return RollResult(
                expression=expression,
                total=total,
                rolls=[roll1, roll2],
                modifier=mod,
                advantage_mode=adv_mode,
                is_crit=(chosen == 20),
                is_fumble=(chosen == 1),
            )

        raw_rolls = [rng.randint(1, sides) for _ in range(count)]
        total = sum(raw_rolls) + mod

        is_crit = (count == 1 and sides == 20 and raw_rolls[0] == 20)
        is_fumble = (count == 1 and sides == 20 and raw_rolls[0] == 1)

        return RollResult(
            expression=expression,
            total=total,
            rolls=raw_rolls,
            modifier=mod,
            advantage_mode=adv_mode,
            is_crit=is_crit,
            is_fumble=is_fumble,
        )


# Global default instance
default_dice_engine = DiceEngine()
