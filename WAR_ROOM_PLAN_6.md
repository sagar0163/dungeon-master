# WAR_ROOM_PLAN_6.md — Issue #6: Seeded procedural dungeon generation

## Status: Final cleanup & commit remaining

### Already completed (prior commits)
- [x] TOML generator params (`data/dungeon_generation.toml`)
- [x] Python seeded dungeon generator (`dungeon_master/generator.py`)
- [x] Python determinism tests (`tests/test_generator.py`) — 14 pass, 4 skip (no dotnet)
- [x] C# `DungeonGenerator` + `SeededRandom` mirroring Python (`Scripts/DungeonGenerator.cs`)
- [x] C# TOML config loader (`Scripts/Config/TomlFile.cs`, `DungeonGenerationConfig.cs`)
- [x] C# headless test harness (`tests_cs/Program.cs`)
- [x] BuilderHUD Generate button wired + GameManager.RegenerateDungeon
- [x] .gitignore updated for tests_cs build artifacts

### Remaining subtasks
- [ ] Untrack `.godot/mono/temp/` build artifacts from git (14 tracked files)
- [ ] Add `.godot/mono/temp/` to .gitignore
- [ ] Run full Python test suite — confirm green
- [ ] Build C# headless test harness (if dotnet available) and run it
- [ ] Cross-layer agreement test: Python vs C# signatures match
- [ ] Commit cleanup, delete plan file, final commit, push
