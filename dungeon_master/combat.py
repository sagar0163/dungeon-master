"""Turn-based Combat Manager enforcing turn structure and logging events."""

from typing import Dict, List, Optional, Union
from dungeon_master.dice import DiceEngine, default_dice_engine
from dungeon_master.models import Character, CombatLogEntry, CombatState, InitiativeEntry, Monster, TurnState
from dungeon_master.rules import resolve_attack, roll_initiative


class CombatManager:
    def __init__(self, dice_engine: DiceEngine = default_dice_engine):
        self.dice_engine = dice_engine
        self.entities: Dict[str, Union[Character, Monster]] = {}
        self.state = CombatState()

    def start_combat(self, participants: List[Union[Character, Monster]], seed: Optional[int] = None) -> CombatState:
        """Starts combat, rolls initiative for all participants, orders turn queue."""
        self.entities = {p.id: p for p in participants}
        self.state = CombatState(round=1, current_turn_index=0)

        init_entries = []
        for p in participants:
            init_val = roll_initiative(p, dice_engine=self.dice_engine, seed=seed)
            p_type = "pc" if isinstance(p, Character) else "npc"
            init_entries.append(
                InitiativeEntry(
                    entity_id=p.id,
                    name=p.name,
                    initiative=init_val,
                    entity_type=p_type,
                    turn_state=TurnState(),
                )
            )

        # Sort by initiative descending
        init_entries.sort(key=lambda x: x.initiative, reverse=True)
        self.state.initiative_order = init_entries
        return self.state

    def get_current_entity(self) -> Optional[Union[Character, Monster]]:
        if not self.state.initiative_order:
            return None
        current_entry = self.state.initiative_order[self.state.current_turn_index]
        return self.entities.get(current_entry.entity_id)

    def execute_attack_turn(
        self,
        target_id: str,
        attack_index: int = 0,
        adv_mode: Optional[str] = None,
        seed: Optional[int] = None,
    ) -> CombatLogEntry:
        """Executes an attack action for the entity whose turn it currently is."""
        attacker = self.get_current_entity()
        if not attacker:
            raise ValueError("No active turn entity found.")

        target = self.entities.get(target_id)
        if not target:
            raise ValueError(f"Target entity {target_id} not found.")

        hit, is_crit, damage, atk_res, dmg_res = resolve_attack(
            attacker=attacker,
            target=target,
            attack_index=attack_index,
            adv_mode=adv_mode,
            dice_engine=self.dice_engine,
            seed=seed,
        )

        log_entry = CombatLogEntry(
            turn=self.state.current_turn_index + 1,
            round=self.state.round,
            entity=attacker.name,
            action="Attack",
            target=target.name,
            roll=atk_res.total,
            ac=target.ac,
            hit=hit,
            is_crit=is_crit,
            damage_roll=dmg_res.expression if dmg_res else "",
            damage=damage,
            target_hp_after=target.hp.current,
        )
        self.state.log.append(log_entry)
        return log_entry

    def next_turn(self) -> InitiativeEntry:
        """Advances combat turn pointer to next participant."""
        if not self.state.initiative_order:
            raise ValueError("Combat initiative order is empty.")

        self.state.current_turn_index += 1
        if self.state.current_turn_index >= len(self.state.initiative_order):
            self.state.current_turn_index = 0
            self.state.round += 1

        next_entry = self.state.initiative_order[self.state.current_turn_index]
        next_entry.turn_state = TurnState()  # Reset action economy
        return next_entry
