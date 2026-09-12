import os

import pytest
from fastapi.testclient import TestClient

import dungeon_master.main as main
from dungeon_master.models import DungeonLord, DungeonRank, DungeonTile, MonsterInstance, TileType
from dungeon_master.save_load import (
    SchemaVersionError,
    delete_save,
    has_save,
    init_db,
    load_game,
    save_game,
)

TEST_DB = "test_dungeon_state.db"


@pytest.fixture(autouse=True)
def reset_module_state():
    main.dungeon_lord = DungeonLord()
    main.dungeon_rank = DungeonRank()
    main.grid_store = {}
    main.monster_store = {}
    yield


@pytest.fixture(autouse=True)
def cleanup_db():
    yield
    if os.path.exists(TEST_DB):
        os.remove(TEST_DB)


def full_state():
    lord = DungeonLord(
        id="lord-fixed-1",
        name="SaveTester",
        level=7,
        hp_base=500.0,
        hp_current=237.5,
        attack_base=22.0,
        defense_base=18.0,
        x=5,
        y=3,
        z=1,
        facing="south",
    )
    rank = DungeonRank(rank=3, essence=1075, essence_capacity_base=1000.0, settlement_reputation=-20, wave_count=12)
    tile1 = DungeonTile(x=0, y=0, z=0, tile_type=TileType.ROOM, room_id="room1")
    tile1.garrisoned_monsters.append("mon1")
    tile1.garrisoned_monsters.append("mon2")
    tile2 = DungeonTile(x=3, y=4, z=1, tile_type=TileType.TRAP, trap_id="spike_trap")
    grid = {"0,0,0": tile1, "3,4,1": tile2}
    m1 = MonsterInstance(id="mon1", name="Goblin King", level=4, hp=88.0, x=0, y=0, z=0)
    m2 = MonsterInstance(id="mon2", name="Troll", level=6, hp=420.5, x=11, y=7, z=2)
    monsters = {"mon1": m1, "mon2": m2}
    return lord, rank, grid, monsters


def state_dump(lord, rank, grid, monsters):
    return (
        lord.model_dump(),
        rank.model_dump(),
        {k: v.model_dump() for k, v in grid.items()},
        {k: v.model_dump() for k, v in monsters.items()},
    )


def test_save_load_round_trip():
    lord, rank, grid, monsters = full_state()
    before = state_dump(lord, rank, grid, monsters)

    save_game(lord, rank, grid, monsters, db_path=TEST_DB)
    l_lord, l_rank, l_grid, l_monsters = load_game(db_path=TEST_DB)

    assert state_dump(l_lord, l_rank, l_grid, l_monsters) == before


def test_round_trip_all_fields_and_multi_tile():
    lord, rank, grid, monsters = full_state()

    save_game(lord, rank, grid, monsters, db_path=TEST_DB)
    l_lord, l_rank, l_grid, l_monsters = load_game(db_path=TEST_DB)

    assert l_lord.hp_current == 237.5
    assert l_lord.attack_base == 22.0
    assert l_lord.defense_base == 18.0
    assert l_lord.z == 1
    assert l_lord.facing == "south"

    assert l_rank.settlement_reputation == -20
    assert l_rank.wave_count == 12
    assert l_rank.essence_capacity_base == 1000.0

    assert set(l_grid) == {"0,0,0", "3,4,1"}
    assert l_grid["3,4,1"].tile_type == TileType.TRAP
    assert l_grid["3,4,1"].trap_id == "spike_trap"
    assert l_grid["0,0,0"].garrisoned_monsters == ["mon1", "mon2"]

    assert set(l_monsters) == {"mon1", "mon2"}
    assert l_monsters["mon2"].hp == 420.5
    assert l_monsters["mon2"].z == 2


def test_load_without_save_returns_defaults():
    lord, rank, grid, monsters = load_game(db_path=TEST_DB)
    assert lord.id != "lord-fixed-1"
    assert rank.wave_count == 0
    assert grid == {}
    assert monsters == {}


def test_load_after_delete_returns_defaults():
    lord, rank, grid, monsters = full_state()
    save_game(lord, rank, grid, monsters, db_path=TEST_DB)

    assert has_save(TEST_DB)
    delete_save(TEST_DB)
    assert not has_save(TEST_DB)
    assert not os.path.exists(TEST_DB)

    l_lord, l_rank, l_grid, l_monsters = load_game(db_path=TEST_DB)
    assert l_lord.id != "lord-fixed-1"
    assert l_grid == {}


def test_schema_version_mismatch_raises():
    lord, rank, grid, monsters = full_state()
    save_game(lord, rank, grid, monsters, db_path=TEST_DB)

    init_db(TEST_DB)
    conn = __import__("sqlite3").connect(TEST_DB)
    conn.execute("UPDATE metadata SET value='999' WHERE key='schema_version'")
    conn.commit()
    conn.close()

    with pytest.raises(SchemaVersionError) as excinfo:
        load_game(db_path=TEST_DB)
    assert excinfo.value.saved_version == 999
    assert excinfo.value.expected_version == 1


@pytest.fixture
def temp_db_path(monkeypatch, tmp_path):
    path = str(tmp_path / "api_test.db")
    monkeypatch.setattr(main, "DB_PATH", path)
    yield path
    if os.path.exists(path):
        os.remove(path)


def test_save_status_endpoint(temp_db_path):
    client = TestClient(main.app)
    assert client.get("/game/save_status").json() == {"has_save": False}

    lord, rank, grid, monsters = full_state()
    save_game(lord, rank, grid, monsters, db_path=temp_db_path)

    assert client.get("/game/save_status").json() == {"has_save": True}


def test_autosave_on_quit(temp_db_path):
    client = TestClient(main.app)
    client.post("/dungeon/defeat_invader", json={"invader_level": 2, "invader_count": 3})
    res = client.post("/game/quit")
    assert res.status_code == 200

    assert has_save(temp_db_path)
    l_lord, l_rank, l_grid, l_monsters = load_game(db_path=temp_db_path)
    assert l_rank.essence == 200 + 165


def test_autosave_on_mode_switch(temp_db_path):
    client = TestClient(main.app)
    client.post("/dungeon/build", json={"x": 5, "y": 5, "z": 0, "tile_type": "room", "room_id": "throne_room"})
    res = client.post("/game/mode_switch", json={"new_mode": "crawl"})
    assert res.status_code == 200
    assert "autosaved" in res.json()["message"]

    l_lord, l_rank, l_grid, l_monsters = load_game(db_path=temp_db_path)
    assert l_grid["5,5,0"].tile_type == TileType.ROOM
    assert l_rank.essence == 200 - 50


def test_startup_auto_load(temp_db_path):
    lord, rank, grid, monsters = full_state()
    save_game(lord, rank, grid, monsters, db_path=temp_db_path)

    with TestClient(main.app) as client:
        assert client.get("/game/save_status").json() == {"has_save": True}
        assert main.dungeon_lord.id == "lord-fixed-1"
        assert main.dungeon_rank.wave_count == 12
        assert "3,4,1" in main.grid_store


def test_api_save_then_load_round_trip(temp_db_path):
    client = TestClient(main.app)
    client.post("/dungeon/defeat_invader", json={"invader_level": 2, "invader_count": 3})
    client.post("/dungeon/build", json={"x": 5, "y": 5, "z": 0, "tile_type": "room", "room_id": "throne_room"})

    res = client.post("/game/save")
    assert res.status_code == 200

    before_lord = main.dungeon_lord.model_dump()
    before_rank = main.dungeon_rank.model_dump()

    main.dungeon_lord = DungeonLord(level=99)
    main.dungeon_rank = DungeonRank(rank=99, wave_count=999)
    main.grid_store = {}

    client.post("/game/load")
    assert main.dungeon_lord.model_dump() == before_lord
    assert main.dungeon_rank.model_dump() == before_rank
    assert "5,5,0" in main.grid_store
