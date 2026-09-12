"""Seeded procedural dungeon generation for Dungeon Lord.

Deterministic and portable: this module is the Python mirror of the C#
``DungeonGenerator`` (Scripts/DungeonGenerator.cs). Both layers share the
xorshift32 RNG (seeded via splitmix32), the same placement/corridor
algorithm, and the same ``data/dungeon_generation.toml`` parameters, so a
given seed yields an identical dungeon grid in Python and C#.
"""

from __future__ import annotations

import os
from dataclasses import dataclass, field
from typing import Dict, List, Optional, Tuple

try:
    import tomllib
except ImportError:  # pragma: no cover - Python < 3.11 fallback
    import tomli as tomllib  # type: ignore

MASK32 = 0xFFFFFFFF

TILE_EMPTY = "empty"
TILE_ROOM = "room"
TILE_CORRIDOR = "corridor"

Room = Tuple[int, int, int, int]  # (x, y, width, height)
TileKey = Tuple[int, int, int]    # (x, y, floor)


def _splitmix32(state: int) -> int:
    state = (state + 0x9E3779B9) & MASK32
    state = ((state ^ (state >> 16)) * 0x85EBCA6B) & MASK32
    state = ((state ^ (state >> 13)) * 0xC2B2AE35) & MASK32
    return (state ^ (state >> 16)) & MASK32


class SeededRandom:
    """xorshift32 PRNG seeded via splitmix32.

    Bit-exact across Python and C# (mirrors SeededRandom in DungeonGenerator.cs).
    """

    def __init__(self, seed: int) -> None:
        state = _splitmix32(seed & MASK32)
        self._state: int = state if state != 0 else 0x9E3779B9

    def next_u32(self) -> int:
        x = self._state
        x ^= (x << 13) & MASK32
        x ^= x >> 17
        x ^= (x << 5) & MASK32
        self._state = x & MASK32
        return self._state

    def next_int(self, min_inclusive: int, max_exclusive: int) -> int:
        width = max_exclusive - min_inclusive
        if width <= 0:
            return min_inclusive
        return min_inclusive + (self.next_u32() % width)


@dataclass
class RoomConfig:
    min_width: int
    max_width: int
    min_height: int
    max_height: int
    target_per_floor: int
    placement_attempts: int
    margin: int
    spacing: int


@dataclass
class CorridorConfig:
    width: int


@dataclass
class GenerationConfig:
    width: int
    height: int
    floors: int
    default_seed: int
    rooms: RoomConfig
    corridors: CorridorConfig
    room_weights: List[Tuple[str, int]] = field(default_factory=list)

    @classmethod
    def from_toml(cls, path: str) -> "GenerationConfig":
        with open(path, "rb") as fh:
            raw = tomllib.load(fh)

        gen = raw["generator"]
        rooms = raw["rooms"]
        corr = raw["corridors"]
        weights = raw["rooms"]["weights"]

        return cls(
            width=int(gen["width"]),
            height=int(gen["height"]),
            floors=int(gen["floors"]),
            default_seed=int(gen["default_seed"]),
            rooms=RoomConfig(
                min_width=int(rooms["min_width"]),
                max_width=int(rooms["max_width"]),
                min_height=int(rooms["min_height"]),
                max_height=int(rooms["max_height"]),
                target_per_floor=int(rooms["target_per_floor"]),
                placement_attempts=int(rooms["placement_attempts"]),
                margin=int(rooms["margin"]),
                spacing=int(rooms["spacing"]),
            ),
            corridors=CorridorConfig(width=int(corr["width"])),
            room_weights=[(str(k), int(v)) for k, v in weights.items()],
        )


@dataclass
class GeneratedDungeon:
    width: int
    height: int
    floors: int
    tiles: Dict[TileKey, Tuple[str, Optional[str]]]

    def signature(self) -> str:
        """Canonical string form; identical grids have identical signatures."""
        parts: List[str] = []
        for z in range(self.floors):
            for y in range(self.height):
                for x in range(self.width):
                    info = self.tiles.get((x, y, z))
                    if info is None:
                        continue
                    room_id = info[1] or ""
                    parts.append(f"{x},{y},{z}:{info[0]}:{room_id}")
        return "|".join(parts)

    def walkable_per_floor(self, floor: int) -> List[TileKey]:
        return [
            (x, y, floor)
            for (x, y, z), info in self.tiles.items()
            if z == floor and info[0] in (TILE_ROOM, TILE_CORRIDOR)
        ]


class DungeonGenerator:
    """Generates a connected sealed dungeon on a 3D grid (x, y, floor).

    All randomness flows through a single ``SeededRandom`` created from the
    provided seed, so the same seed always produces the identical grid.
    """

    def __init__(self, config: GenerationConfig) -> None:
        self._config = config

    @staticmethod
    def load_config(path: Optional[str] = None) -> GenerationConfig:
        config_path = path or os.path.join(
            os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
            "data",
            "dungeon_generation.toml",
        )
        return GenerationConfig.from_toml(config_path)

    def generate(self, seed: int) -> GeneratedDungeon:
        cfg = self._config
        rng = SeededRandom(seed)
        tiles: Dict[TileKey, Tuple[str, Optional[str]]] = {}

        for z in range(cfg.floors):
            rooms = self._place_rooms(rng, z)
            for room in rooms:
                self._carve_room(rng, room, z, tiles)
            for i in range(1, len(rooms)):
                self._connect(rng, rooms[i - 1], rooms[i], z, tiles)

        return GeneratedDungeon(cfg.width, cfg.height, cfg.floors, tiles)

    # -- room placement ---------------------------------------------------

    def _place_rooms(self, rng: SeededRandom, floor: int) -> List[Room]:
        cfg = self._config
        rooms: List[Room] = []
        attempts = 0

        while attempts < cfg.rooms.placement_attempts and len(rooms) < cfg.rooms.target_per_floor:
            attempts += 1

            w = rng.next_int(cfg.rooms.min_width, cfg.rooms.max_width + 1)
            h = rng.next_int(cfg.rooms.min_height, cfg.rooms.max_height + 1)

            x_max = cfg.width - cfg.rooms.margin - w
            y_max = cfg.height - cfg.rooms.margin - h
            if x_max < cfg.rooms.margin or y_max < cfg.rooms.margin:
                continue

            x = rng.next_int(cfg.rooms.margin, x_max + 1)
            y = rng.next_int(cfg.rooms.margin, y_max + 1)

            candidate = (x, y, w, h)
            if any(self._overlaps(candidate, other) for other in rooms):
                continue

            rooms.append(candidate)

        rooms.sort(key=lambda r: (r[1], r[0]))
        return rooms

    def _overlaps(self, a: Room, b: Room) -> bool:
        spacing = self._config.rooms.spacing
        return (
            a[0] - spacing < b[0] + b[2]
            and b[0] - spacing < a[0] + a[2]
            and a[1] - spacing < b[1] + b[3]
            and b[1] - spacing < a[1] + a[3]
        )

    # -- carving ----------------------------------------------------------

    def _carve_room(self, rng: SeededRandom, room: Room, floor: int, tiles: Dict[TileKey, Tuple[str, Optional[str]]]) -> None:
        x, y, w, h = room
        room_id = self._pick_weighted(rng, self._config.room_weights)
        for dx in range(w):
            for dy in range(h):
                tiles[(x + dx, y + dy, floor)] = (TILE_ROOM, room_id)

    def _connect(self, rng: SeededRandom, a: Room, b: Room, floor: int, tiles: Dict[TileKey, Tuple[str, Optional[str]]]) -> None:
        ax, ay = a[0] + a[2] // 2, a[1] + a[3] // 2
        bx, by = b[0] + b[2] // 2, b[1] + b[3] // 2
        cw = max(1, self._config.corridors.width)

        horizontal_first = (rng.next_u32() & 1) == 0
        if horizontal_first:
            self._carve_h_line(ay, ax, bx, cw, floor, tiles)
            self._carve_v_line(bx, ay, by, cw, floor, tiles)
        else:
            self._carve_v_line(ax, ay, by, cw, floor, tiles)
            self._carve_h_line(by, ax, bx, cw, floor, tiles)

    def _carve_h_line(self, row: int, x0: int, x1: int, cw: int, floor: int, tiles: Dict[TileKey, Tuple[str, Optional[str]]]) -> None:
        lo, hi = (x0, x1) if x0 <= x1 else (x1, x0)
        offset = -(cw // 2)
        for dx in range(lo, hi + 1):
            for d in range(cw):
                self._set_corridor(dx, row + offset + d, floor, tiles)

    def _carve_v_line(self, col: int, y0: int, y1: int, cw: int, floor: int, tiles: Dict[TileKey, Tuple[str, Optional[str]]]) -> None:
        lo, hi = (y0, y1) if y0 <= y1 else (y1, y0)
        offset = -(cw // 2)
        for dy in range(lo, hi + 1):
            for d in range(cw):
                self._set_corridor(col + offset + d, dy, floor, tiles)

    def _set_corridor(self, x: int, y: int, floor: int, tiles: Dict[TileKey, Tuple[str, Optional[str]]]) -> None:
        if not (0 <= x < self._config.width and 0 <= y < self._config.height):
            return
        current = tiles.get((x, y, floor))
        if current is not None and current[0] == TILE_ROOM:
            return  # never downgrade a room tile
        tiles[(x, y, floor)] = (TILE_CORRIDOR, None)

    # -- helpers ----------------------------------------------------------

    @staticmethod
    def _pick_weighted(rng: SeededRandom, weights: List[Tuple[str, int]]) -> str:
        total = sum(w for _, w in weights)
        if weights and total > 0:
            roll = rng.next_u32() % total
            acc = 0
            for key, weight in weights:
                acc += weight
                if roll < acc:
                    return key
        return weights[-1][0] if weights else "spawn"


def floor_rooms_connected(dungeon: GeneratedDungeon, floor: int) -> bool:
    """True if every walkable tile (room/corridor) on a floor is reachable from any other."""
    walkable = set(dungeon.walkable_per_floor(floor))
    if not walkable:
        return True  # a fully empty floor is trivially connected

    start = next(iter(walkable))
    seen = {start}
    stack = [start]
    while stack:
        x, y, z = stack.pop()
        for nx, ny in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
            neighbor = (nx, ny, z)
            if neighbor in walkable and neighbor not in seen:
                seen.add(neighbor)
                stack.append(neighbor)

    return seen == walkable
