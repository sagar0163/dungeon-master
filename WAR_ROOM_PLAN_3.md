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
- [ ] Attempt real export: download dotnet/godot/templates, run `make export-all`, fix any C# compile errors
- [ ] Verify `builds/dungeon_lord.x86_64` runs (headless smoke test) — if Godot unavailable, verify preset/script correctness statically
- [ ] Final cleanup: remove this plan file, final commit referencing #3, push branch

Progress notes:

- Godot project root is repo ROOT (project.godot + DungeonLord.csproj at root); `dungeon_master/` is the Python tooling package.
- No .tscn scenes exist yet; GameManager auto-constructs every subsystem, so a single root node with GameManager suffices as Main scene.
- Godot/.NET not installed on this machine → `make setup` must bootstrap them.
- Internet available. `~/.nuget/packages/godot.net.sdk/4.2.0` cached → `dotnet build` should restore offline.
- Download sizes: Godot mono linux zip 55MB, mono export templates tpz 860MB, dotnet SDK 217MB.
- A partial `.tools/dotnet` (sdk only, no muxer) exists from a prior attempt — Makefile re-runs installer to complete it.