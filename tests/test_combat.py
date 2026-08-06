import pytest
from dungeon_master.models import DungeonLord, DungeonTile, InvaderParty, TileType
from dungeon_master.rules import calculate_invader_essence_reward


def test_dungeon_lord_garrison_and_combat():
    lord = DungeonLord(name="Lord Malakor", level=5)
    tile = DungeonTile(x=2, y=3, z=0, tile_type=TileType.ROOM, room_id="armory")
    tile.garrisoned_monsters.append("skeleton_warrior_1")

    assert tile.tile_type == TileType.ROOM
    assert "skeleton_warrior_1" in tile.garrisoned_monsters

    invaders = InvaderParty(name="Hero Party", count=4, avg_level=3)
    reward = calculate_invader_essence_reward(
        invader_level=invaders.avg_level,
        invader_count=invaders.count,
        settlement_reputation=50,
    )
    # base = 25 * 4 * 3 = 300. rep mult = 1.50 -> 450
    assert reward == 450
