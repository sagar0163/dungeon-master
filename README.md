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
├── project.godot       # Godot 4 project (C#)
├── DungeonLord.csproj  # .NET 8 project
├── Scripts/            # C# scripts (Builder, Crawl, Grid, Essence, Leveling)
├── dungeon_lord/       # Godot scenes/assets structure
│   ├── scenes/         # Builder, Crawl, UI scenes
│   ├── scripts/        # Grid, entities, progression, economy, AI, config
│   └── assets/
├── tools/              # Python tooling (config validation, etc.)
├── tests/              # Unit tests (progression, grid, dice, combat, rules)
└── .venv/              # Python virtual environment
```

## Quick Start

### Prerequisites
- **Godot 4.2+** (C# edition) — https://godotengine.org/download
- **.NET 8 SDK** — https://dotnet.microsoft.com/download
- **Python 3.10+** (for tooling/tests)

### Run the Game
```bash
cd /home/sagar-jadhav/Documents/my\ project/dungeon_master

# Open in Godot editor
godot --path . --editor

# Or build and run export (after building in editor)
./builds/dungeon_lord.x86_64
```

### Run Tests (Python Tooling)
```bash
cd /home/sagar-jadhav/Documents/my\ project/dungeon_master
source .venv/bin/activate
pytest tests/ -v
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
- ✅ Python tooling + 41 passing tests (progression, grid, dice, combat, rules)
- ✅ Graphify knowledge graph
- ✅ All skills installed (gstack, speckit, ponytail)
- ✅ GitHub synced: https://github.com/sagar0163/dungeon-master

## Next Steps
1. Install Godot 4.2+ C# edition
2. Open `project.godot` in Godot editor
3. Build first scene: Builder Mode grid editor
4. Implement tile placement + Essence spending
5. Add Crawl Mode camera + grid movement
6. Hook up invader pathfinding (A* on grid)