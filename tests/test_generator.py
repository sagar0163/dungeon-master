"""
Seeded procedural dungeon generation tests for Dungeon Lord.

Mirrored by the C# headless harness in tests_cs/ (same RNG, same algorithm,
same TOML config) so both layers must agree on seed determinism.
"""

import os
import subprocess

import pytest

from dungeon_master.generator import DungeonGenerator, GenerationConfig, floor_rooms_connected

REPO_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CONFIG_PATH = os.path.join(REPO_ROOT, "data", "dungeon_generation.toml")
CS_BINARY = os.path.join(REPO_ROOT, "tests_cs", "bin", "Debug", "net8.0", "DungeonLord.Tests")


@pytest.fixture(scope="module")
def generator() -> DungeonGenerator:
    return DungeonGenerator(GenerationConfig.from_toml(CONFIG_PATH))


@pytest.fixture(scope="module")
def config() -> GenerationConfig:
    return GenerationConfig.from_toml(CONFIG_PATH)


class TestTomlConfig:
    """Generator parameters come from TOML — no hardcoded numbers."""

    def test_config_loads_from_toml(self, config):
        assert config.width == 32
        assert config.height == 32
        assert config.floors == 3
        assert config.default_seed == 1337

    def test_room_params_from_toml(self, config):
        assert config.rooms.min_width == 3
        assert config.rooms.max_width == 8
        assert config.rooms.min_height == 3
        assert config.rooms.max_height == 8
        assert config.rooms.target_per_floor == 8
        assert config.rooms.placement_attempts == 200
        assert config.rooms.margin == 2
        assert config.rooms.spacing == 1

    def test_corridor_params_from_toml(self, config):
        assert config.corridors.width == 1

    def test_room_weights_from_toml(self, config):
        weights = dict(config.room_weights)
        assert weights["spawn"] == 3
        assert weights["treasure"] == 2
        assert weights["barracks"] == 2
        assert weights["shrine"] == 1
        assert weights["trap_room"] == 1
        assert weights["boss"] == 1


class TestSeedDeterminism:
    """Same seed -> identical grid (NFR-4: deterministic replay)."""

    def test_same_seed_same_grid(self, generator):
        a = generator.generate(1337)
        b = generator.generate(1337)
        assert a.signature() == b.signature()

    def test_same_seed_every_call(self, generator):
        sigs = {generator.generate(42).signature() for _ in range(3)}
        assert len(sigs) == 1

    def test_different_seed_different_grid(self, generator):
        a = generator.generate(12345)
        b = generator.generate(54321)
        assert a.signature() != b.signature()

    def test_empty_grid_ignored_by_signature(self, generator):
        d = generator.generate(7)
        assert d.signature()  # non-trivial grid produced


class TestConnectivity:
    """Generated dungeon must be one connected component per floor."""

    @pytest.mark.parametrize("seed", [1, 1337, 2024, 99991])
    def test_all_floors_connected(self, generator, seed):
        dungeon = generator.generate(seed)
        for floor in range(dungeon.floors):
            assert floor_rooms_connected(dungeon, floor), f"floor {floor} not connected for seed {seed}"

    def test_rooms_placed(self, generator):
        dungeon = generator.generate(1337)
        room_count = sum(1 for info in dungeon.tiles.values() if info[0] == "room")
        assert room_count > 0
        corridor_count = sum(1 for info in dungeon.tiles.values() if info[0] == "corridor")
        assert corridor_count > 0

    def test_rooms_within_margin(self, generator):
        dungeon = generator.generate(1337)
        margin = 2
        for (x, y, _z), info in dungeon.tiles.items():
            if info[0] == "room":
                assert x >= margin and y >= margin
                assert x < dungeon.width - margin and y < dungeon.height - margin


class TestCrossLayerAgreement:
    """Python and C# generators must produce the same grid for the same seed.

    Runs the C# headless harness if it has been built (tests_cs/); skipped
    otherwise so the Python suite stays runnable without a .NET build.
    """

    @pytest.mark.skipif(not os.path.exists(CS_BINARY), reason="C# test harness not built")
    @pytest.mark.parametrize("seed", [1, 1337, 2024, 99991])
    def test_matches_csharp_signature(self, generator, seed):
        result = subprocess.run(
            [CS_BINARY, "--signature", str(seed)],
            cwd=REPO_ROOT,
            capture_output=True,
            text=True,
            timeout=30,
        )
        assert result.returncode == 0, result.stderr
        cs_sig = result.stdout.strip().split("SIG:", 1)[1]
        python_sig = generator.generate(seed).signature()
        assert cs_sig == python_sig, f"C# and Python grids differ for seed {seed}"


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
