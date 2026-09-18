# WAR ROOM PLAN — Issue #3: Export presets (Linux + Web) with one-command build

Goal: committed `export_presets.cfg` (Web + Linux), a `make export-all` one-command
build into `builds/`, a runnable Linux binary + loadable Web build, and a README
link to a playable web build. Branch: `war-room-issue-3`.

Checklist:

- [x] Inspect repo layout, git state, gods/dotnet availability (start of session)
- [x] Create minimal Godot Main scene (wires existing GameManager C# logic) and fix `project.godot` (main_scene, app icon)
- [x] Write `export_presets.cfg` with Web + Linux desktop presets
- [x] Write `Makefile` with `export-all` target + `make setup` (self-host godot/dotnet/templates if missing)
- [x] Add `builds/` to `.gitignore` (outputs are artifacts, but export_presets.cfg is committed)
- [x] Update README: one-command build instructions + web build link (itch.io/GH Pages)
- [x] Add GitHub Actions workflow for web build deploy (GH Pages artifact) so the link is live
- [x] Run `pytest tests/ -v` (Python tooling sanity)
- [x] Fix Makefile to download MONO export templates (standard templates lack `_mono` variants → C# export fails), add exclude_filter so `.tools/.venv/python tooling` don't bloat the pck
- [x] Remove leftover C# build artifacts (DungeonLord.Tests/, tests_cs/ bin+obj-only) + gitignore bin/obj
- [x] Fix C# compile errors (game assembly had never compiled: GD0102 exports on non-Godot types, CS0426 nested-type refs, API mismatches, leftover tests_cs/ obj artifacts globbed into build)
- [x] Attempt real export: run `make export-all` (setup downloads godot/dotnet/templates completed)
- [x] Fix malformed `.gitignore` trailing line (`graphify-out/cache//templates.tpz` -> `graphify-out/cache/`)
- [ ] Re-run `make setup` detached (prior `.tools/` was gone at start of this session) — godot+dotnet+templates
- [ ] Run `make export-all` for real and inspect `builds/`
- [ ] Verify `builds/dungeon_lord.x86_64` runs (headless smoke test)
- [ ] Verify Web build artifacts + served page load (static check via curl)
- [ ] Final cleanup: remove this plan file, final commit referencing #3, push branch

Progress notes:

- Godot project root is repo ROOT (project.godot + DungeonLord.csproj at root); `dungeon_master/` is the Python tooling package.
- No .tscn scenes exist yet; GameManager auto-constructs every subsystem, so a single root node with GameManager suffices as Main scene.
- Godot/.NET not installed on this machine → `make setup` must bootstrap them.
- Internet available. `~/.nuget/packages/godot.net.sdk/4.2.0` cached → `dotnet build` should restore offline.
- Download sizes: Godot mono linux zip 55MB, mono export templates tpz 860MB, dotnet SDK 217MB.
- A partial `.tools/dotnet` (sdk only, no muxer) exists from a prior attempt — Makefile re-runs installer to complete it.