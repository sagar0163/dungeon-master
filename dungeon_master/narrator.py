"""AI Narrator Bridge.

Adheres strictly to the prompt contract in SPEC.md:
The AI Narrator NEVER decides rules or outcomes. It only receives structured
combat log events and produces 2-3 sensory sentences of immersive narration.
"""

from typing import Optional
from dungeon_master.models import CombatLogEntry


class AINarrator:
    def __init__(self, api_key: Optional[str] = None):
        self.api_key = api_key

    def build_prompt(self, entry: CombatLogEntry) -> str:
        """Constructs strict AI DM prompt contract."""
        return (
            "SYSTEM: You are a D&D 5e DM. You NEVER decide outcomes.\n"
            f"EVENT: {entry.entity} attempted {entry.action} against {entry.target}.\n"
            f"RESULT: hit={entry.hit}, crit={entry.is_crit}, damage={entry.damage}, target_hp_after={entry.target_hp_after}.\n"
            "INSTRUCTION: Output 2-3 sensory sentences narrating this outcome. Do not mention numerical dice rolls or mechanics."
        )

    def narrate_event(self, entry: CombatLogEntry) -> str:
        """Generates immersive narration for a combat log entry.

        Provides a clean local template fallback for offline-capable core (NFR-6).
        """
        if entry.action == "Attack":
            if entry.hit:
                if entry.is_crit:
                    if entry.target_hp_after <= 0:
                        return (
                            f"With a lethal strike of terrifying precision, {entry.entity}'s weapon shatters "
                            f"{entry.target}'s defenses! The blow lands with crushing force, felling {entry.target} instantly."
                        )
                    return (
                        f"In a burst of martial brilliance, {entry.entity} finds a vital flaw in {entry.target}'s armor. "
                        f"The weapon bites deep, drawing a howl of agonizing pain!"
                    )
                else:
                    if entry.target_hp_after <= 0:
                        return (
                            f"{entry.entity}'s attack drives home, striking {entry.target} full in the chest. "
                            f"The creature crumples to the ground, motionless."
                        )
                    return (
                        f"{entry.entity} surges forward and lands a solid blow on {entry.target}. "
                        f"{entry.target} stumbles back, grimacing as pain shoots through its body."
                    )
            else:
                return (
                    f"{entry.entity} lashes out at {entry.target}, but the swing whistles harmlessly through empty air. "
                    f"{entry.target} swiftly dodges out of harm's way."
                )

        return f"{entry.entity} performs {entry.action} targeting {entry.target}."
