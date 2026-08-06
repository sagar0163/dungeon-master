import pytest
from fastapi.testclient import TestClient
from dungeon_master.main import app

client = TestClient(app)


def test_root():
    response = client.get("/")
    assert response.status_code == 200
    data = response.json()
    assert data["name"] == "Dungeon Lord Engine API"
    assert data["version"] == "0.1.0"


def test_health():
    response = client.get("/health")
    assert response.status_code == 200
    assert response.json() == {"status": "healthy"}


def test_get_dungeon_lord():
    res = client.get("/dungeon/lord")
    assert res.status_code == 200
    data = res.json()
    assert data["name"] == "Dungeon Lord"
    assert data["level"] == 1


def test_build_tile_and_defeat_invader():
    # Test defeating invader for essence
    inv_res = client.post("/dungeon/defeat_invader", json={"invader_level": 2, "invader_count": 3})
    assert inv_res.status_code == 200
    assert inv_res.json()["essence_gained"] == 165

    # Test building tile
    build_res = client.post(
        "/dungeon/build",
        json={"x": 5, "y": 5, "z": 0, "tile_type": "room", "room_id": "throne_room"},
    )
    assert build_res.status_code == 200
    assert build_res.json()["status"] == "success"