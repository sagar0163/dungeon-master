# SPEC: Dungeon Lord — Hybrid Dungeon-Builder + First-Person Crawler

## Vision
**Dungeon Lord** — A single-player game where you play as an **embodied Dungeon Lord** who both **builds** the dungeon (Builder Mode, top-down) and **crawls** it (Crawl Mode, first-person grid-based). The dungeon defends against AI-controlled adventuring parties ("invaders"). Defeating invaders yields **Essence** to expand/upgrade. Two progression tracks (Lord level + Dungeon rank) share one growth formula.

---

## Core Pillars
1. **Dual Perspective** — Same dungeon grid, two views: Builder (strategy) ↔ Crawl (action)
2. **Embodied Lord** — Physical body, personal combat, can die, not a bodiless core
3. **Invader-Driven** — Settlement sends parties; you defend/intercept in real time
4. **Essence Economy** — Single resource from kills → building, upgrading, summoning
5. **Shared Growth Formula** — Lord level + Dungeon rank use identical percentage-based scaling

---

## Functional Requirements

### 1. Dual-Mode Architecture
- **FR-1.1**: **Builder Mode** — Top-down/isometric grid editor: place rooms, corridors, traps, monster spawn points, spend Essence, assign/level garrisoned monsters, view dungeon stats (rank, Essence storage, room capacity, reputation)
- **FR-1.2**: **Crawl Mode** — First-person, **discrete tile-based** movement/turning (Dungeon Master / Legend of Grimrock style), NOT free-roaming. Full combat as Lord. Same grid data as Builder — no separate level design.
- **FR-1.3**: **Seamless Switch** — Toggle between modes anytime (hotkey/UI). State persists: Lord position, monster positions, trap states, Essence, time.
- **FR-1.4**: **Possession (Secondary)** — Lord temporarily sees/acts through garrisoned monster's senses. Limited duration. Optional utility for scouting/remote defense. Not required to progress.

### 2. Dungeon Grid System (Single Source of Truth)
- **FR-2.1**: Grid-based dungeon: rooms, corridors, traps, monster spawns = data entries on grid
- **FR-2.2**: Same grid renders **both** Builder top-down AND Crawl first-person view
- **FR-2.3**: Rooms have types: Entrance, Spawn, Treasure, Trap, Boss, Shrine, Barracks, etc. Each type has Essence cost, capacity, unlock rank
- **FR-2.4**: Corridors connect rooms, have width, can hold traps
- **FR-2.5**: Multi-floor: new floors unlock at dungeon rank thresholds, expand total buildable area
- **FR-2.6**: Grid coordinates: (x, y, floor) — used for pathfinding, line-of-sight, combat positioning

### 3. Essence Economy
- **FR-3.1**: Essence harvested from **defeated invaders** (scale with invader strength + dungeon reputation)
- **FR-3.2**: Essence spent on: room construction, room upgrades, trap placement/upgrades, monster summoning, monster leveling, Lord equipment
- **FR-3.3**: Essence storage capacity capped by dungeon rank (upgradeable)
- **FR-3.4**: Essence income rate displayed in Builder Mode HUD

### 4. Dungeon Rank & Progression
- **FR-4.1**: Discrete dungeon ranks (separate from Lord level). Each rank unlocks: new room types, higher monster tiers, additional floor/room capacity
- **FR-4.2**: Rank-up gated by **Essence thresholds** (config-driven, not XP)
- **FR-4.3**: Rank-up grants: +room capacity, +Essence storage, +monster tier cap, new room/trap types

### 5. Monsters & Garrisons
- **FR-5.1**: Summon monsters by spending Essence (cost scales with tier)
- **FR-5.2**: Assign to guard specific rooms/corridors (spawn points)
- **FR-5.3**: Monsters **level independently** of dungeon, using **shared growth formula** (Section 10)
- **FR-5.4**: Monster stats: HP, damage, speed, abilities, AI behavior (patrol, guard, ambush)
- **FR-5.5**: Monster types per tier: Tier 1 (goblin, rat, skeleton), Tier 2 (hobgoblin, wraith, ogre), Tier 3 (troll, vampire, lich), etc.
- **FR-5.6**: Possession: Lord takes direct control of monster (see FR-1.4)

### 6. Invader Simulation (AI Parties)
- **FR-6.1**: Invader parties generated from **settlement reputation + dungeon rank**
- **FR-6.2**: Party composition: classes (fighter, wizard, cleric, rogue), levels, equipment, tactics
- **FR-6.3**: Pathfinding: A* through dungeon grid, trigger traps, engage monsters/Lord
- **FR-6.4**: Combat: real-time in Crawl Mode, auto-resolve in Builder Mode (or fast-forward)
- **FR-6.5**: Settlement reputation: background system only (v1). Town not visitable. Scales invader frequency/strength
- **FR-6.6**: Invader goals: reach dungeon core, loot treasure, kill Lord, destroy spawn points

### 7. Dungeon Reset Cycle
- **FR-7.1**: After dungeon cleared OR cooldown period: traps reset, monster garrisons respawn, loot repopulates
- **FR-7.2**: Allows repeated invader waves — not one-time playthrough
- **FR-7.3**: Reset configurable: timer-based, wave-based, or manual (Builder Mode button)

### 8. Lord Character (Player Avatar)
- **FR-8.1**: Physical stats: HP, damage, speed, armor, resistances
- **FR-8.2**: Equipment slots: weapon, armor, accessory, consumables
- **FR-8.3**: Abilities: basic attack, dodge/block, special (unlock per level), possession
- **FR-8.4**: Level progression: uses **shared growth formula** (Section 10)
- **FR-8.5**: Death consequences: **open item** — run failure vs penalty-and-respawn (decide in tuning)

### 9. Combat System (Crawl Mode)
- **FR-9.1**: Grid-based positioning (same grid as dungeon). Discrete tiles.
- **FR-9.2**: Real-time with **turn-based feel**: Lord acts → invaders/monsters react (or simultaneous with initiative)
- **FR-9.3**: Actions: move (1 tile), attack, ability, use item, possess, switch mode
- **FR-9.4**: Line of sight / fog of war in Crawl Mode
- **FR-9.5**: Damage types, resistances, status effects (stun, slow, poison, fear)
- **FR-9.6**: Auto-combat option for Builder Mode (simulate with same rules)

### 10. Shared Growth Formula (Core Math)
```
attribute = base × (1 + 0.01 × level + milestone_bonus(level))

milestone_bonus(level):
  - Every 10th level: +10% × milestone_count
  - Every 25th level: +25% × milestone_count (replaces 10-level milestone on shared levels)
  - Growth is ADDITIVE (not multiplicative/compounding)
  - All percentages config-driven (tunable without code changes)
```
- **Applies to**: Lord level (personal stats) + Dungeon rank (room capacity, Essence storage, monster tier cap)
- **Open item**: Exact stacking at shared 10/25 thresholds (e.g., level 100) — finalize during tuning

### 11. Session Structure (v1 Scope)
- **FR-11.1**: Single dungeon, endless wave survival (no campaign story)
- **FR-11.2**: Win condition: survive X waves / reach Dungeon Rank Y
- **FR-11.3**: Loss condition: Lord death (per FR-8.5) OR dungeon core destroyed
- **FR-11.4**: Session save/load: full state (grid, Lord, monsters, Essence, rank, reputation, wave count)

---

## Non-Functional Requirements

| NFR | Target |
|-----|--------|
| **NFR-1** | 60 FPS in Crawl Mode (first-person) |
| **NFR-2** | <100ms mode switch (Builder ↔ Crawl) |
| **NFR-3** | <500ms invader pathfinding (A* on grid) |
| **NFR-4** | Deterministic combat replay (same seed = same outcome) |
| **NFR-5** | Offline single-player (no required server) |
| **NFR-6** | Config-driven balance (no hardcoded numbers in logic) |

---

## Acceptance Criteria (Playable v1)

- [ ] **Builder Mode**: Place 3 room types, 2 trap types, assign 2 monster types, spend Essence, see dungeon stats
- [ ] **Crawl Mode**: Walk grid in first-person, turn 90°, move tile-by-tile, attack invader, take damage, use possession
- [ ] **Switch Modes**: Hotkey toggles Builder ↔ Crawl in <100ms, Lord position preserved
- [ ] **Invader Wave**: Party spawns at entrance, pathfinds to core, triggers trap, fights monster, Lord can intercept
- [ ] **Essence Loop**: Kill invader → gain Essence → spend on room upgrade → see capacity increase
- [ ] **Rank Up**: Accumulate Essence → hit threshold → rank up → new room type unlocked
- [ ] **Monster Leveling**: Garrisoned monster kills invader → gains XP → levels up → stats increase per formula
- [ ] **Lord Leveling**: Lord kills invader → gains XP → levels up → stats increase per formula
- [ ] **Reset Cycle**: Clear dungeon → wait cooldown → traps reset, monsters respawn, loot reappears
- [ ] **Save/Load**: Full state persisted, reload → identical grid, Lord, monsters, Essence, wave count
- [ ] **Config Tuning**: Change growth percentages in config → restart → new scaling works without code changes

---

## Technical Architecture

| Layer | Technology | Responsibility |
|-------|------------|----------------|
| **Core Engine** | Python (or Rust) + **Godot / Unity / Custom** | Grid, pathfinding, combat rules, progression math |
| **Grid System** | Custom 3D grid (x, y, floor) | Single source for Builder + Crawl rendering |
| **State Store** | SQLite (local) / PostgreSQL (future) | Dungeon grid, entities, progression, session |
| **AI/Pathfinding** | A* on grid + behavior trees | Invader parties, monster AI, patrol routes |
| **Renderer** | **Godot 4** (recommended) or Unity | Builder: orthographic/isometric. Crawl: first-person grid |
| **Config** | TOML/JSON | All balance numbers, growth formulas, unlock tables |

### Recommended Stack: **Godot 4 + GDScript/C#**
- Native 2D/3D grid rendering (Builder top-down + Crawl first-person)
- Built-in navigation (A* on GridMap)
- Single codebase, single executable, no external server needed
- Export to Windows/Linux/macOS/Web
- Open source, no royalties

---

## Data Models (Core)

### Dungeon Grid Cell
```json
{
  "x": 5, "y": 3, "floor": 0,
  "type": "room",
  "room_type": "spawn",
  "trap": null,
  "monster_spawn": {"monster_id": "goblin_t1", "level": 3, "max_count": 2, "current_count": 2},
  "loot": ["essence_shard", "iron_key"],
  "connections": [{"dir": "north", "target": [5,2,0]}, {"dir": "east", "target": [6,3,0]}]
}
```

### Lord State
```json
{
  "position": {"x": 5, "y": 3, "floor": 0, "facing": "north"},
  "level": 5,
  "xp": 1240,
  "stats": {"hp": 85, "max_hp": 85, "damage": 18, "speed": 5.2, "armor": 12},
  "equipment": {"weapon": "iron_sword", "armor": "leather_vest", "accessory": "essence_amulet"},
  "abilities": ["basic_attack", "dodge", "essence_strike", "possession"],
  "essence_carried": 450
}
```

### Monster Instance
```json
{
  "id": "goblin_001",
  "template": "goblin_t1",
  "level": 3,
  "position": {"x": 7, "y": 3, "floor": 0},
  "assignment": "guard_room_5_3",
  "stats": {"hp": 28, "max_hp": 28, "damage": 6, "speed": 4.5},
  "ai_state": "patrol",
  "possessable": true
}
```

### Invader Party
```json
{
  "id": "party_012",
  "members": [
    {"class": "fighter", "level": 3, "role": "tank"},
    {"class": "wizard", "level": 3, "role": "dps"},
    {"class": "cleric", "level": 2, "role": "healer"}
  ],
  "target": "dungeon_core",
  "path": [[1,0,0], [2,0,0], [3,0,0], ...],
  "state": "advancing",
  "reputation_modifier": 1.2
}
```

### Progression Config (TOML)
```toml
[growth]
base_multiplier = 0.01          # +1% per level
milestone_10_bonus = 0.10       # +10% every 10 levels
milestone_25_bonus = 0.25       # +25% every 25 levels
stacking_rule = "replace_25"    # "add" | "replace_10" | "replace_25" | "max"

[dungeon_ranks]
1 = {essence_threshold: 0, room_capacity: 10, essence_storage: 1000, monster_tier_cap: 1}
2 = {essence_threshold: 5000, room_capacity: 18, essence_storage: 2500, monster_tier_cap: 2}
3 = {essence_threshold: 15000, room_capacity: 30, essence_storage: 5000, monster_tier_cap: 3}
```

---

## AI/Invader Behavior (No LLM for Gameplay)

**Rule-based behavior trees, not LLM:**
- Invaders: Pathfind → Avoid known traps → Focus fire Lord → Retreat if <25% HP
- Monsters: Patrol route → Guard spawn → Aggro on sight → Flee if <15% HP
- Lord possession: Direct control, monster AI disabled

**LLM only for:** Optional flavor text (dungeon logs, monster descriptions) — NOT mechanics.

---

## Out of Scope (v1)
- Overworld/town visits (settlement is background only)
- Multiplayer (single-player first)
- Campaign story / scripted quests
- Fog of war in Builder Mode (full visibility)
- Free-roam movement in Crawl Mode (grid-only)
- Modding API
- Voice/TTS

---

## Success Metric
**Player can: Build a 3-floor dungeon → Survive 10 invader waves → Reach Dungeon Rank 3 → All using only grid-based Builder/Crawl modes, with Essence loop and shared growth formula driving progression.**