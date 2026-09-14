# WAR_ROOM_PLAN_6 — Seeded procedural dungeon generation

Task: Issue #6 — deterministic, TOML-configurable procedural dungeon gen, wired into Builder HUD.

## Prior-attempt status (committed, verified)

- [x] TOML generator params (`data/dungeon_generation.toml`) — source of truth, no hardcoded numbers
- [x] Python `DungeonGenerator` + `SeededRandom` (xorshift32/splitmix32) mirroring C# bit-for-bit
- [x] Python determinism/connectivity/config tests (`tests/test_generator.py`)
- [x] C# `DungeonGenerator` + config classes (pure C#, no Godot dep)
- [x] C# headless harness `tests_cs/` mirroring Python tests + `--signature` mode (used by cross-layer test)
- [x] `Generate` button wired: `BuilderHUD.OnGenerateRequested` → `GameManager.OnGenerateRequested` → `RegenerateDungeon(seed)` → all consumers re-pointed
- [x] **Cross-layer agreement now VERIFIED on this machine** (`/tmp/opencode/dotnet/dotnet`): C# harness 45/45 checks pass; Python cross-layer tests 4/4 seeds agree between Python and C# signatures

## This session (resume work)

- [x] Fix #6-introduced compile bug in `GameManager.cs`: `DungeonGrid.TileType.X` → `TileType.X`
      (property/class name collision = color-color rule violation => CS1061; confirmed via minimal repro).
      Affects `RegenerateDungeon` (line ~353) and `FindFirstWalkableTile` (line ~385), both added by #6.
- [x] Rebuild full Godot C# project; confirm no NEW errors attributable to #6 (pre-existing
      GD0102/CS0426/CS0722 debt in InvaderAI/InventoryHUD/CrawlHUD/DungeonResetCycle etc. stays out of scope,
      also broken on main@HEAD which has 52 errors)
- [x] Verify C# harness still builds/runs (45/45) and full `pytest tests/` passes (54/54 incl.
      the 4 cross-layer agreement tests now that dotnet is available at /tmp/opencode/dotnet)
- [ ] Final pass: commit, remove this plan file, push
      - [x] commit GameManager.cs fix
      - [ ] rm plan file + final commit + push

## Notes

- dotnet lives at `/tmp/opencode/dotnet/dotnet` (no system-wide dotnet). Export `DOTNET_ROOT=/tmp/opencode/dotnet`
  before `dotnet` commands. NuGet cache already has Godot.NET.Sdk/4.2.0.
- Project's primary test command (AGENTS.md): `pytest tests/` via `.venv`.
- The wider Godot project (InvaderAI `DungeonGrid.TileType`, `InvaderParty`/`ItemData` nested-type refs,
  `[Export]` non-Godot types, static `LevelingEngine`) did not compile on `main` either — pre-existing, out of scope.