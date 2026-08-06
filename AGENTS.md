# AGENTS.md — Dungeon Master Project

## Project Overview
**Dungeon Lord** — Hybrid dungeon-builder + first-person grid crawler (Dungeon Keeper × Legend of Grimrock).
- **Builder Mode**: Top-down/isometric grid editor — place rooms, traps, monster spawns, spend Essence
- **Crawl Mode**: First-person, **discrete tile-based** movement (Dungeon Master / Legend of Grimrock style)
- **Single dungeon grid** — same data renders both modes
- **Invader Simulation**: AI parties pathfind through dungeon, trigger traps, fight monsters/Lord
- **Essence Economy**: Harvested from kills → building, upgrading, summoning
- **Dual Progression**: Lord level + Dungeon rank share **one growth formula** (additive, milestone-based)
- **Godot 4 recommended** — native grid rendering, A* pathfinding, single executable

## Quick Commands
```bash
# Godot 4 (recommended)
cd /home/sagar-jadhav/Documents/my\ project/dungeon_master
godot --path . --editor

# Or run exported build
./builds/dungeon_lord.x86_64

# Tests (Python tooling for data validation)
source .venv/bin/activate
pytest tests/ -v

# Graphify
graphify update .
graphify query "question"
```

## graphify

This project has a knowledge graph at `graphify-out/` with god nodes, community structure, and cross-file relationships.

When the user types `/graphify`, use the installed graphify skill or instructions before doing anything else.

Rules:
- For codebase questions, first run `graphify query "<question>"` when `graphify-out/graph.json` exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- Dirty graphify-out/ files are expected after hooks or incremental updates; dirty graph files are not a reason to skip graphify. Only skip graphify if the task is about stale or incorrect graph output, or the user explicitly says not to use it.
- If `graphify-out/wiki/index.md` exists, use it for broad navigation instead of raw source browsing.
- Read `graphify-out/GRAPH_REPORT.md` only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).

## Skill routing

When the user's request matches an available skill, invoke it via the Skill tool. When in doubt, invoke the skill.

Key routing rules:
- Product ideas/brainstorming → invoke /office-hours
- Strategy/scope → invoke /plan-ceo-review
- Architecture → invoke /plan-eng-review
- Design system/plan review → invoke /design-consultation or /plan-design-review
- Full review pipeline → invoke /autoplan
- Bugs/errors → invoke /investigate
- QA/testing site behavior → invoke /qa or /qa-only
- Code review/diff check → invoke /review
- Visual polish → invoke /design-review
- Ship/deploy/PR → invoke /ship or /land-and-deploy
- Save progress → invoke /context-save
- Resume context → invoke /context-restore
- Author a backlog-ready spec/issue → invoke /spec
- **Minimal/lazy coding (YAGNI, stdlib-first, delete over add)** → invoke /ponytail [lite|full|ultra]
- Code review with ponytail lens → invoke /ponytail-review
- Audit for over-engineering → invoke /ponytail-audit
- Technical debt analysis → invoke /ponytail-debt
- What to add back (ponytail gain) → invoke /ponytail-gain
- Ponytail help/usage → invoke /ponytail-help

## Project Structure
```
dungeon_master/
├── BRD.md                     # Business Requirements (source of truth)
├── SPEC.md                    # Technical specification
├── AGENTS.md                  # This file
├── pyproject.toml             # Python tooling config (data validation, tests)
├── requirements.txt           # Python deps
├── .gitignore
├── README.md
├── graphify-out/              # Knowledge graph
├── .hermes/skills/gstack/     # Vendored gstack skills
├── .hermes/skills/ponytail/   # Ponytail minimalist coding skills
├── .specify/                  # Specify/speckit skills
├── dungeon_lord/              # Godot 4 project (main game)
│   ├── project.godot
│   ├── scenes/
│   │   ├── builder/           # Builder Mode scenes
│   │   ├── crawl/             # Crawl Mode scenes
│   │   └── ui/                # Shared UI
│   ├── scripts/
│   │   ├── grid/              # Grid system, pathfinding
│   │   ├── entities/          # Lord, monsters, invaders
│   │   ├── progression/       # Growth formula, ranks
│   │   ├── economy/           # Essence, costs
│   │   ├── ai/                # Behavior trees, invader logic
│   │   └── config/            # TOML config loading
│   ├── assets/
│   └── addons/
├── tools/                     # Python tooling
│   ├── validate_config.py     # TOML schema validation
│   ├── generate_data.py       # Proc-gen helpers
│   └── test_progression.py    # Growth formula tests
└── tests/
    ├── test_progression.py    # Growth formula unit tests
    └── test_grid.py           # Grid/pathfinding tests
```

## Architecture Notes
- **Engine**: Godot 4 (GDScript or C#) — single codebase, native grid rendering, built-in A*
- **Grid**: 3D array (x, y, floor) — single source for Builder (orthographic) + Crawl (first-person)
- **State**: SQLite (local) — dungeon grid, entities, progression, session saves
- **AI**: Behavior trees + A* pathfinding (no LLM for gameplay mechanics)
- **Config**: TOML — all balance numbers, growth formulas, unlock tables (tunable without code changes)
- **Progression**: Shared additive formula with milestone bonuses (config-driven)
- **LLM**: Optional only for flavor text (logs, descriptions) — NEVER for mechanics

## Coordinated Workflow — How Skills Complement Each Other

### Phase 1: Discover & Shape (Strategy → Spec)
```
/office-hours          # Brainstorm dungeon mechanics, get clarity
  ↓
/plan-ceo-review       # Lock scope: what's in v1 (single dungeon, no overworld)
  ↓
/spec                  # Write SPEC.md with FRs/NFRs (speckit-specify)
  ↓
graphify update .      # Index new spec into knowledge graph
```

### Phase 2: Plan & Architect (Design → Tasks)
```
/plan-eng-review       # Architecture review: grid system, Godot scenes, progression math
  ↓
/plan-design-review    # UI/UX for Builder Mode (grid editor) + Crawl Mode (first-person HUD)
  ↓
speckit-plan           # Break spec into implementation plan
  ↓
speckit-tasks          # Generate atomic, ordered task list
  ↓
graphify query "show me the grid module"  # Verify context before coding
```

### Phase 3: Implement (Code with Ponytail Discipline)
```
/ponytail full         # Activate YAGNI/stdlib-first mode for ALL coding
  ↓
# For each task:
speckit-implement      # Implement single task (ponytail enforces minimal diff)
  ↓
graphify query "how does grid connect to pathfinding?"  # Check impact before changes
  ↓
/ponytail-review       # Self-review: did I over-build? delete unused code
```

### Phase 4: Validate & Harden (QA → Review)
```
/qa                    # Test the running game (Godot, grid movement, combat, pathfinding)
  ↓
/investigate           # If bugs: root-cause, not symptom
  ↓
/review                # Pre-merge diff review (ponytail lens + gstack review)
  ↓
/ponytail-audit        # Scan for over-engineering, unused abstractions
  ↓
/ponytail-debt         # Document deliberate simplifications (ponytail: comments)
```

### Phase 5: Ship & Learn
```
/ship                  # Merge, export build, tag
  ↓
/context-save          # Save session context for next feature
  ↓
/ponytail-gain         # What to add back when metrics demand it
```

### Daily Loop (Continuous)
```
graphify query "..."   # Start any coding session: understand first
/ponytail full         # Keep minimalist discipline active
# ... code ...
graphify update .      # End of session: refresh knowledge graph
```

### Skill Complement Map

| Need | Primary | Complement | Why |
|------|---------|------------|-----|
| Understand codebase | `graphify query` | `/investigate` | Graph gives structure; investigate gives runtime behavior |
| Plan feature | `/plan-eng-review` | `speckit-plan` | Eng review = architecture; speckit = task breakdown |
| Write minimal code | `/ponytail full` | `speckit-implement` | Ponytail = philosophy; speckit = execution pattern |
| Review PR | `/review` | `/ponytail-review` | gstack = process; ponytail = "delete more" lens |
| Find bugs | `/investigate` | `graphify path A B` | Investigate = debug flow; graphify = static dependencies |
| Avoid over-engineering | `/ponytail-audit` | `/plan-eng-review` | Audit = retrospective; eng review = prospective |

### Ponytail Intensity by Phase

| Phase | Intensity | Reason |
|-------|-----------|--------|
| Spec/Plan | `lite` | Explore options, don't prematurely constrain |
| Implement | `full` (default) | Enforce ladder, stdlib-first, shortest diff |
| Refactor/Legacy | `ultra` | Delete aggressively, challenge every abstraction |
| Security/Auth | `full` + explicit | Never lazy on trust boundaries, crypto, validation |
| Tests | `full` | One assert-based check per non-trivial logic; YAGNI on test frameworks |

### Graphify + Ponytail Synergy
- **Before coding**: `graphify query "what touches user auth?"` → understand → `/ponytail full` → code minimally
- **After coding**: `graphify update .` → graph stays current for next session
- **Refactoring**: `graphify path "old_module" "new_module"` → see ripple → `/ponytail-audit` → delete dead code

### Speckit + gstack Bridge
- `speckit-specify` → writes SPEC.md (gstack reads for context)
- `speckit-tasks` → emits tasks (gstack `/autoplan` can consume)
- `speckit-git-commit` → auto-commits at each stage (gstack `/ship` expects clean history)