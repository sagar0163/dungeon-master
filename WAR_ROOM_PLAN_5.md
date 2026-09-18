# War Room Plan — Issue #5: CI automated build + test gate

Prior attempt added `.github/workflows/ci.yml`, README badge, httpx+PYTHONPATH for pytest. Remaining
problem: `dotnet build DungeonLord.csproj` fails (C# Godot project never compiles — 26 errors,
needs Godot runtime types/out-of-spec scripts). CI gate cannot be green until this is fixed.

- [x] Add GitHub Actions workflow (pytest + dotnet + godot headless) that runs on PRs and push to main
- [x] Add README CI status badge
- [x] Install httpx for pytest; PYTHONPATH for fastapi app import
- [ ] Merge latest `main` into `war-room-issue-5` and resolve `.github/workflows/ci.yml` + README conflicts
- [ ] Fix C# build so `dotnet build DungeonLord.csproj` succeeds (GD0102 exports, InvaderParty/TileType/ItemData refs, BuilderHUD/CrawlHUD usings, static LevelingEngine)
- [ ] Verify locally: `dotnet build DungeonLord.csproj`, `pytest tests/ -v`, godot headless smoke where tooling allows
- [ ] Delete plan file + final commit referencing #5