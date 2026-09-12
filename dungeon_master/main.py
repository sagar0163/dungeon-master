"""FastAPI REST API for Dungeon Lord Hybrid Management & Grid-Crawler Engine."""

from typing import Dict, Optional

from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel

from dungeon_master.models import DungeonLord, DungeonRank, DungeonTile, MonsterInstance, TileType
from dungeon_master.rules import calculate_invader_essence_reward
from dungeon_master.save_load import DB_PATH, delete_save, has_save, load_game, save_game

load_dotenv()

app = FastAPI(title="Dungeon Lord API", version="0.1.0")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Shared Dungeon State
dungeon_lord = DungeonLord()
dungeon_rank = DungeonRank()
grid_store: Dict[str, DungeonTile] = {}
monster_store: Dict[str, MonsterInstance] = {}


@app.on_event("startup")
def _on_startup():
    global dungeon_lord, dungeon_rank, grid_store, monster_store
    if has_save(DB_PATH):
        dungeon_lord, dungeon_rank, grid_store, monster_store = load_game(DB_PATH)


@app.on_event("shutdown")
def _on_shutdown():
    save_game(dungeon_lord, dungeon_rank, grid_store, monster_store, DB_PATH)


class BuildTileRequest(BaseModel):
    x: int
    y: int
    z: int = 0
    tile_type: TileType
    room_id: Optional[str] = None
    trap_id: Optional[str] = None


class InvaderDefeatRequest(BaseModel):
    invader_level: int = 1
    invader_count: int = 3


@app.get("/")
def root():
    return {"name": "Dungeon Lord Engine API", "version": "0.1.0", "status": "running"}


@app.get("/health")
def health():
    return {"status": "healthy"}


@app.get("/dungeon/lord", response_model=DungeonLord)
def get_dungeon_lord():
    return dungeon_lord


@app.get("/dungeon/rank", response_model=DungeonRank)
def get_dungeon_rank():
    return dungeon_rank


@app.post("/dungeon/build")
def build_tile(req: BuildTileRequest):
    tile_key = f"{req.x},{req.y},{req.z}"
    cost = 50  # 50 Essence per tile
    if dungeon_rank.essence < cost:
        raise HTTPException(status_code=400, detail="Insufficient Essence")

    dungeon_rank.essence -= cost
    tile = DungeonTile(
        x=req.x,
        y=req.y,
        z=req.z,
        tile_type=req.tile_type,
        room_id=req.room_id,
        trap_id=req.trap_id,
    )
    grid_store[tile_key] = tile
    return {"status": "success", "tile": tile, "remaining_essence": dungeon_rank.essence}


@app.post("/dungeon/defeat_invader")
def defeat_invader(req: InvaderDefeatRequest):
    reward = calculate_invader_essence_reward(
        invader_level=req.invader_level,
        invader_count=req.invader_count,
        settlement_reputation=dungeon_rank.settlement_reputation,
    )
    cap = dungeon_rank.essence_capacity
    dungeon_rank.essence = min(int(cap), dungeon_rank.essence + reward)
    return {"essence_gained": reward, "total_essence": dungeon_rank.essence}




class ModeSwitchRequest(BaseModel):
    new_mode: str
@app.post("/game/mode_switch")
def mode_switch(req: ModeSwitchRequest):
    save_game(dungeon_lord, dungeon_rank, grid_store, monster_store, DB_PATH)
    return {"status": "success", "message": f"Switched to {req.new_mode} and autosaved"}


@app.post("/game/quit")
def quit_game():
    save_game(dungeon_lord, dungeon_rank, grid_store, monster_store, DB_PATH)
    return {"status": "success", "message": "Autosaved and quit"}


@app.post("/game/save")
def save_game_endpoint():
    save_game(dungeon_lord, dungeon_rank, grid_store, monster_store, DB_PATH)
    return {"status": "success"}


@app.post("/game/load")
def load_game_endpoint():
    global dungeon_lord, dungeon_rank, grid_store, monster_store
    dungeon_lord, dungeon_rank, grid_store, monster_store = load_game(DB_PATH)
    return {"status": "success"}


@app.get("/game/save_status")
def save_status():
    return {"has_save": has_save(DB_PATH)}


@app.post("/game/new")
def new_game():
    global dungeon_lord, dungeon_rank, grid_store, monster_store
    delete_save(DB_PATH)
    dungeon_lord = DungeonLord()
    dungeon_rank = DungeonRank()
    grid_store = {}
    monster_store = {}
    return {"status": "success"}


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("dungeon_master.main:app", host="0.0.0.0", port=8000, reload=True)
