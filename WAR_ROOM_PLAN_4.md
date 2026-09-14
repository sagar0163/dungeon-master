# Issue #4: Vertical Slice - Builder<->Crawl Loop on a Single Grid

## Context

The prior attempt wrote the three `.tscn` scenes as hand-rolled XML, which is NOT a
valid Godot text-scene format (Godot 4 uses `[gd_scene]`/`[node]` syntax) and referenced
non-existent assets (`res://assets/GridTile.tscn`, `icon.svg`, a non-existent
`ModeSwitcher` script). `dotnet build` also fails with 27 pre-existing errors across the
engine scripts (stale `DungeonGrid.TileType` / `InvaderAI.InvaderParty` /
`ItemDatabase.ItemData` references, `[Export]` on non-Godot C# types -> GD0102, missing
`using DungeonLord.Scripts.UI` in GameManager, static-type `LevelingEngine` export).

So both checked items 1-6 and the "dotnet build works" item were NOT actually true. Redo.

## Checklist (ordered by dependency)

### A. Make the engine compile (`dotnet build` green)
- [x] Fix stale nested-type refs: `DungeonGrid.TileType` -> `TileType`, `InvaderAI.InvaderParty` -> `InvaderParty`, `ItemDatabase.ItemData` -> `ItemData`
- [x] Fix GD0102 `[Export]` on non-Godot types; drop static `LevelingEngine` export; stop `AddChild`ing plain classes
- [x] Add `using DungeonLord.Scripts.UI;` to GameManager
- [x] dotnet build (also fixed Godot3->4 API drift: Key.PageUp, Button.Alignment, events on `?.`, SetDisabled, ThemeFontSize)

### B. Runnable vertical-slice scene
- [ ] Remove bad `config/icon` from project.godot; point main scene at GameRoot
- [ ] Rewrite GameRoot.tscn, BuilderMode.tscn, CrawlMode.tscn as valid Godot 4 tscn (real node names, C# ext_resource refs)
- [ ] Add GameRoot.cs: owns shared DungeonGrid(8x8x1) + EssenceManager, wires controllers, Tab mode switch <100ms
- [ ] GridVisual.cs: render grid tiles (room/trap colored) from the single DungeonGrid in both modes
- [ ] Lord spawns at most recently built room on Crawl start; tile-by-tile movement + 90deg turn works
- [ ] Builder: place one room + one trap spending Essence (default trap type preset)
- [ ] Verify tile placed top-down is physically present first-person (single data model)

### C. Docs & verification
- [ ] Add GIF of build->switch->walk loop to README quick start
- [ ] Run existing Python tests - all pass
- [ ] Final: remove plan file, commit, push

