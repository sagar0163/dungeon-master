import os
import pytest
from dungeon_master.models import DungeonLord, DungeonRank, DungeonTile, MonsterInstance, TileType
from dungeon_master.save_load import save_game, load_game, init_db

TEST_DB = "test_dungeon_state.db"

@pytest.fixture(autouse=True)
def run_around_tests():
    yield
    if os.path.exists(TEST_DB):
        os.remove(TEST_DB)

def test_save_load_round_trip():
    lord = DungeonLord(name="SaveTester", level=10, hp_base=500.0, x=5, y=5, z=0, facing="south")
    rank = DungeonRank(rank=3, essence=1000, wave_count=5)
    
    tile1 = DungeonTile(x=0, y=0, tile_type=TileType.ROOM, room_id="room1")
    tile1.garrisoned_monsters.append("mon1")
    grid = {"0,0,0": tile1}
    
    mon1 = MonsterInstance(id="mon1", name="Goblin King", hp=100.0, x=0, y=0, z=0)
    monsters = {"mon1": mon1}
    
    save_game(lord, rank, grid, monsters, db_path=TEST_DB)
    
    l_lord, l_rank, l_grid, l_monsters = load_game(db_path=TEST_DB)
    
    assert l_lord.id == lord.id
    assert l_lord.name == "SaveTester"
    assert l_lord.level == 10
    assert l_lord.x == 5
    assert l_lord.facing == "south"
    
    assert l_rank.rank == 3
    assert l_rank.essence == 1000
    assert l_rank.wave_count == 5
    
    assert "0,0,0" in l_grid
    assert l_grid["0,0,0"].tile_type == TileType.ROOM
    assert "mon1" in l_grid["0,0,0"].garrisoned_monsters
    
    assert "mon1" in l_monsters
    assert l_monsters["mon1"].name == "Goblin King"
    assert l_monsters["mon1"].hp == 100.0
