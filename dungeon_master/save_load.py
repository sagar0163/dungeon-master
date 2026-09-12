import os
import sqlite3
from typing import Dict, Optional, Tuple

from dungeon_master.models import DungeonLord, DungeonRank, DungeonTile, MonsterInstance

DB_PATH = "dungeon_state.db"
SCHEMA_VERSION = 1


class SchemaVersionError(Exception):
    """Raised when a save file has an incompatible schema version."""
    def __init__(self, saved_version: int, expected_version: int):
        self.saved_version = saved_version
        self.expected_version = expected_version
        super().__init__(
            f"Save file schema v{saved_version} is not compatible "
            f"(expected v{expected_version}). Migration not yet implemented."
        )


def init_db(db_path: str = DB_PATH):
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS metadata (
            key TEXT PRIMARY KEY,
            value TEXT
        )
    ''')
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS lord_state (
            id INTEGER PRIMARY KEY,
            data TEXT
        )
    ''')
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS rank_state (
            id INTEGER PRIMARY KEY,
            data TEXT
        )
    ''')
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS grid_state (
            key TEXT PRIMARY KEY,
            data TEXT
        )
    ''')
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS monster_state (
            id TEXT PRIMARY KEY,
            data TEXT
        )
    ''')
    cursor.execute("INSERT OR IGNORE INTO metadata (key, value) VALUES ('schema_version', ?)", (str(SCHEMA_VERSION),))
    conn.commit()
    conn.close()


def _get_schema_version(conn: sqlite3.Connection) -> Optional[int]:
    cursor = conn.cursor()
    cursor.execute("SELECT value FROM metadata WHERE key = 'schema_version'")
    row = cursor.fetchone()
    if row is None:
        return None
    return int(row[0])


def has_save(db_path: str = DB_PATH) -> bool:
    if not os.path.exists(db_path):
        return False
    conn = sqlite3.connect(db_path)
    version = _get_schema_version(conn)
    conn.close()
    return version is not None


def delete_save(db_path: str = DB_PATH):
    if os.path.exists(db_path):
        os.remove(db_path)

def save_game(lord: DungeonLord, rank: DungeonRank, grid: Dict[str, DungeonTile], monsters: Dict[str, MonsterInstance], db_path: str = DB_PATH):
    init_db(db_path)
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    cursor.execute("DELETE FROM lord_state")
    cursor.execute("INSERT INTO lord_state (id, data) VALUES (1, ?)", (lord.model_dump_json(),))

    cursor.execute("DELETE FROM rank_state")
    cursor.execute("INSERT INTO rank_state (id, data) VALUES (1, ?)", (rank.model_dump_json(),))

    cursor.execute("DELETE FROM grid_state")
    for key, tile in grid.items():
        cursor.execute("INSERT INTO grid_state (key, data) VALUES (?, ?)", (key, tile.model_dump_json()))

    cursor.execute("DELETE FROM monster_state")
    for mid, monster in monsters.items():
        cursor.execute("INSERT INTO monster_state (id, data) VALUES (?, ?)", (mid, monster.model_dump_json()))

    conn.commit()
    conn.close()

def load_game(db_path: str = DB_PATH) -> Tuple[DungeonLord, DungeonRank, Dict[str, DungeonTile], Dict[str, MonsterInstance]]:
    if not os.path.exists(db_path):
        return DungeonLord(), DungeonRank(), {}, {}

    conn = sqlite3.connect(db_path)
    try:
        saved_version = _get_schema_version(conn)
        if saved_version is not None and saved_version != SCHEMA_VERSION:
            raise SchemaVersionError(saved_version, SCHEMA_VERSION)

        cursor = conn.cursor()

        cursor.execute("SELECT data FROM lord_state WHERE id=1")
        row = cursor.fetchone()
        lord = DungeonLord.model_validate_json(row[0]) if row else DungeonLord()

        cursor.execute("SELECT data FROM rank_state WHERE id=1")
        row = cursor.fetchone()
        rank = DungeonRank.model_validate_json(row[0]) if row else DungeonRank()

        grid: Dict[str, DungeonTile] = {}
        cursor.execute("SELECT key, data FROM grid_state")
        for row in cursor.fetchall():
            grid[row[0]] = DungeonTile.model_validate_json(row[1])

        monsters: Dict[str, MonsterInstance] = {}
        cursor.execute("SELECT id, data FROM monster_state")
        for row in cursor.fetchall():
            monsters[row[0]] = MonsterInstance.model_validate_json(row[1])
    finally:
        conn.close()

    return lord, rank, grid, monsters
