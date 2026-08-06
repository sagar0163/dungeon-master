"""Core Pydantic data models for Dungeon Lord hybrid management & grid-crawler game."""

from enum import Enum
from typing import Dict, List, Optional
from uuid import uuid4
from pydantic import BaseModel, ConfigDict, Field


class TileType(str, Enum):
    EMPTY = "empty"
    CORRIDOR = "corridor"
    ROOM = "room"
    TRAP = "trap"
    SPAWN_POINT = "spawn_point"
    LORD_CHAMBER = "lord_chamber"


class DungeonTile(BaseModel):
    x: int
    y: int
    z: int = 0
    tile_type: TileType = TileType.EMPTY
    room_id: Optional[str] = None
    trap_id: Optional[str] = None
    garrisoned_monsters: List[str] = Field(default_factory=list)


class DungeonLord(BaseModel):
    id: str = Field(default_factory=lambda: str(uuid4()))
    name: str = "Dungeon Lord"
    level: int = 1
    hp_base: float = 100.0
    hp_current: float = 100.0
    attack_base: float = 15.0
    defense_base: float = 10.0

    @property
    def hp_max(self) -> float:
        from dungeon_master.rules import calculate_attribute
        return calculate_attribute(self.hp_base, self.level)

    @property
    def attack_power(self) -> float:
        from dungeon_master.rules import calculate_attribute
        return calculate_attribute(self.attack_base, self.level)


class DungeonRank(BaseModel):
    rank: int = 1
    essence: int = 200
    essence_capacity_base: float = 1000.0
    settlement_reputation: int = 10  # Low hostility

    @property
    def essence_capacity(self) -> float:
        from dungeon_master.rules import calculate_attribute
        return calculate_attribute(self.essence_capacity_base, self.rank)


class InvaderParty(BaseModel):
    id: str = Field(default_factory=lambda: str(uuid4()))
    name: str = "Adventuring Party"
    count: int = 3
    avg_level: int = 1
    total_hp: float = 120.0
    essence_reward: int = 50
