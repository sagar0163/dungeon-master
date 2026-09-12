# Dungeon Lord

**Hybrid dungeon-builder + first-person grid crawler** — Dungeon Keeper × Legend of Grimrock.

## Game Concept
Play as an **embodied Dungeon Lord** who:
- **Builds** the dungeon in **Builder Mode** (top-down/isometric grid editor)
- **Crawls** the dungeon in **Crawl Mode** (first-person, discrete tile-based movement)
- Defends against **AI adventuring parties** ("invaders") that pathfind through your dungeon
- Harvests **Essence** from defeated invaders to expand/upgrade
- Progresses via **shared growth formula** (Lord level + Dungeon rank)

## Architecture
| Layer | Technology |
|-------|------------|
| **Engine** | Godot 4 (C#) |
| **Grid** | 3D array (x, y, floor) — single source for both modes |
| **State** | SQLite (local) — dungeon, entities, progression, saves |
| **AI** | Behavior trees + A* pathfinding (no LLM for gameplay) |
| **Config** | TOML — all balance numbers, formulas, unlock tables |
| **Python Tooling** | Data validation, proc-gen helpers, unit tests |

## Project Structure
```
dungeon_master/
├── BRD.md              # Business Requirements (from Dungeon_Lord_BRD.docx)
├── SPEC.md             # Technical specification
├── AGENTS.md           # Agent instructions + coordinated workflows
├── project.godot       # Godot 4 project (C#) — repo root is the Godot project
├── DungeonLord.csproj  # .NET 8 project
├── export_presets.cfg  # Export presets: Linux desktop + Web (WASM)
├── Makefile            # One-command build: `make export-all`
├── Scenes/             # Godot scenes (Main, etc.)
├── Scripts/            # C# scripts (Builder, Crawl, Grid, Essence, Leveling)
├── dungeon_master/     # Python tooling package
├── tools/              # Python tooling (config validation, etc.)
├── tests/              # Unit tests (progression, grid, dice, combat, rules)
└── .venv/              # Python virtual environment
```

## Play Online (Web Build)

**▶️ Play Dungeon Lord in your browser:** <https://sagar0163.github.io/dungeon-master/>

A static Web (WASM) build is produced by CI on every push to `main` and
deployed to GitHub Pages (see `.github/workflows/web-build.yml`). It runs the
full C# game with tile-based grid movement — place rooms in **Builder Mode**
(keys `1`–`5` to choose a tool, **Enter** to place, **Tab** to switch modes),
then crawl your dungeon with **WASD / Arrow keys** (90° turns, discrete tiles).

The same build can be served locally after exporting:

```bash
make serve        # http://localhost:8080
```

## Quick Start

### Prerequisites
- **Godot 4.2+** (C# edition) — https://godotengine.org/download (only needed to open the editor)
- **.NET 8 SDK** — https://dotnet.microsoft.com/download
- **make**, **curl**, **unzip** (for the one-command build)
- **Python 3.10+** (for tooling/tests)

### One-Command Build (Linux + Web)
`export_presets.cfg` is committed with **Linux desktop** and **Web (WASM)**
presets. The Makefile is self-hosting: it downloads Godot, the .NET SDK and the
export templates into `.tools/` on first use — no manual editor step needed.

```bash
make setup          # first time only: bootstraps Godot + .NET + templates
make export-all     # exports builds/dungeon_lord.x86_64 + builds/web/

# Or individual platforms:
make export-linux
make export-web
```

### Run the Game
```bash
# Run the exported Linux desktop build
make run            # == ./builds/dungeon_lord.x86_64

# Serve the Web build locally
make export-web
make serve          # http://localhost:8080

# Open in Godot editor (development)
godot --path . --editor
```

### Run Tests (Python Tooling)
```bash
make test           # or: source .venv/bin/activate && pytest tests/ -v
```

### Graphify (Knowledge Graph)
```bash
graphify update .
graphify query "show me the grid module"
```

## Core Systems (Implemented in C#)

| Script | Purpose |
|--------|---------|
| `DungeonGrid.cs` | 3D grid (32×32×3), tile types, connections |
| `BuilderController.cs` | Top-down mode: place rooms, traps, spawns, spend Essence |
| `CrawlController.cs` | First-person grid movement, combat, possession |
| `EssenceManager.cs` | Essence economy: earn from kills, spend on building |
| `LevelingEngine.cs` | Shared growth formula with milestone bonuses |

## Growth Formula (Config-Driven)
```
attribute = base × (1 + 0.01 × level + milestone_bonus)

milestone_bonus:
  - Every 10th level: +10% × count
  - Every 25th level: +25% × count (replaces 10-level on shared)
  - Additive, not multiplicative
  - All percentages in TOML config
```

## Skills Available (via AGENTS.md)
- **gstack**: `/office-hours`, `/plan-eng-review`, `/qa`, `/review`, `/ship`, etc.
- **speckit**: `/spec`, `/plan`, `/tasks`, `/implement`, git hooks
- **graphify**: `graphify query`, `graphify path`, `graphify explain`
- **ponytail**: `/ponytail [lite|full|ultra]`, `/ponytail-review`, `/ponytail-audit`

## Development Workflow
1. `/office-hours` → `/plan-ceo-review` → `/spec` (shape the feature)
2. `/plan-eng-review` → `/plan-design-review` → `speckit-plan` → `speckit-tasks` (plan)
3. `/ponytail full` + `speckit-implement` per task (code minimally)
4. `/qa` → `/investigate` → `/review` → `/ponytail-audit` (validate)
5. `/ship` → `/context-save` → `/ponytail-gain` (ship & learn)

## Status
- ✅ BRD/SPEC/AGENTS aligned
- ✅ Godot 4 C# project structure
- ✅ Core C# scripts (Grid, Builder, Crawl, Essence, Leveling)
- ✅ Export presets: Linux + Web; `make export-all` one-command build
- ✅ Web build deployed to GitHub Pages (playable in browser)
- ✅ Python tooling + 41 passing tests (progression, grid, dice, combat, rules)
- ✅ Graphify knowledge graph
- ✅ All skills installed (gstack, speckit, ponytail)
- ✅ GitHub synced: https://github.com/sagar0163/dungeon-master

## Next Steps
1. Verify the exported Web build in a browser (see **Play Online**)
2. Verify the exported Linux build (`make run`)
3. Continue gameplay content: more room types, traps, monster behaviors
4. Wire real SQLite persistence
<!-- keep -->
