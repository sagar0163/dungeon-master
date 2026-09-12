# WAR ROOM PLAN — Issue #6: Seeded procedural dungeon generation

Branch: `war-room-issue-6` (base `main`). Repo root is a Godot 4 (C#) project with Python tooling/tests.

## Context notes
- `DungeonGrid` (C#, `Scripts/DungeonGrid.cs`) is pure C# (no Godot) — good, testable headless.
- Whole Godot C# project does NOT currently compile at `main` (pre-existing errors in PossessionManager/GameManager/etc.). Out of scope; my new code must nonetheless be pure/self-consistent.
- `dotnet` SDK 8.0.425 available at `/tmp/opencode/dotnet/dotnet` (shared runtime 8.0.31 present).
- Python: 36 tests pass with `PYTHONPATH=.`; Python 3.12 (stdlib `tomllib` available, `tomli` NOT installed in venv).

## Design (shared between both layers)
- Deterministic RNG: **xorshift32**, seeded via **splitmix32** — identical implementation in Python and C#.
- Algorithm: per floor, place target rooms (random size/pos from TOML params, no overlap with spacing), assign RoomId from TOML weighted room-type table, then L-shaped corridors connect consecutive sorted rooms (corridor width from TOML). Same seed -> identical grid in both languages.
- Config source of truth: `data/dungeon_generation.toml`. Python loads via `tomllib`; C# has a tiny dependency-free TOML-subset reader.
- Canonical grid signature (string) used by both layers to compare determinism and cross-layer agreement.

## Checklist
- [x] 1. `data/dungeon_generation.toml` — generator + room + corridor params (no hardcoded numbers in code)
- [x] 2. Python `dungeon_master/generator.py` — SeededRandom, config, DungeonGenerator
- [x] 3. Python tests `tests/test_generator.py` — seed determinism, diff-seed diff, per-floor connectivity, TOML config values, cross-layer signature check (runs C# binary if built)
- [x] 4. C# `Scripts/Config/DungeonGenerationConfig.cs` (config + minimal TOML reader)
- [x] 5. C# `Scripts/DungeonGenerator.cs` — mirrors Python algorithm, writes to `DungeonGrid`
- [x] 6. C# standalone test `tests_cs/` (console, no packages) mirroring Python tests + `--signature <seed>` mode; exclude dir from Godot build
- [x] 7. Build & run C# test, run full Python suite, fix failures
- [ ] 8. Wire "Generate" into BuilderHUD (`GenerateDungeonButton`) + GameManager regen
- [ ] 9. Final pass: graphify update, clean up plan file, final commit + push

## Commands
```bash
# Python tests
PYTHONPATH=. .venv/bin/pytest tests/ -q
# C# test harness
PATH=/tmp/opencode/dotnet:$PATH DOTNET_ROOT=/tmp/opencode/dotnet dotnet run --project tests_cs
```