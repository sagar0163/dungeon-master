# WAR ROOM PLAN — Issue #9: Unit-test the C# core

Goal: C# test project proving the engine math (grid, leveling, essence, A*) matches the
Python suite (41 passing tests). Tests must run via `dotnet test` and be wired into CI.

## Environment facts (verified)
- dotnet 8.0.425 lives at `/tmp/opencode/dotnet/dotnet` (DOTNET_ROOT=/tmp/opencode/dotnet). Not on PATH.
- NuGet cache at `~/.nuget/packages` (godot.net.sdk 4.2.0 present); network to nuget.org works.
- `DungeonLord.csproj` (Godot.NET.Sdk) currently does NOT build — 27 pre-existing compile
  errors (CS0426 nested-type refs, GD0102 exports). Fixing the whole game is out of scope.
- The 4 core modules to test: `Scripts/LevelingEngine.cs` (pure C#), `Scripts/DungeonGrid.cs`
  (pure C#), `Scripts/EssenceManager.cs` (pure C#), and InvaderAI's A* (currently Godot-coupled
  and on a broken file).

## Checklist

- [ ] Extract InvaderAI's A* core into Godot-free `Scripts/Pathfinding.cs` (same numbers/heuristic),
      and refactor `InvaderAI.FindPath` to delegate to it (also fix its scope-carrying TileType errors).
- [ ] Add `DungeonLord.Tests/` project (NUnit) that <Compile>-links the four pure C# core files.
- [ ] Mirrored tests: LevelingEngine growth formula + milestone stacking (Python test_progression/rules values).
- [ ] Mirrored tests: DungeonGrid coordinates/bounds (Python test_grid TestGrid3D).
- [ ] Mirrored tests: EssenceManager earn/spend/cap + rank-up capacity growth.
- [ ] Mirrored tests: A* pathfinding (straight corridor, obstacle, stairs/multi-floor, unreachable -> empty)
      (Python test_grid TestAStarPathfinding).
- [ ] Exclude DungeonLord.Tests from the Godot csproj glob so the game build is not polluted.
- [ ] `dotnet test` passes (both Debug and, if used, CI path).
- [ ] Add GitHub Actions CI workflow: pytest (Python 41 suite) + dotnet test.
- [ ] Run Python suite locally to confirm it still passes (41 tests).
- [ ] Cleanup: remove WAR_ROOM_PLAN_9.md, final commit, push branch.