# BRD: Dungeon Lord — Hybrid Dungeon Management & Grid-Crawler Game Engine

## 1. Executive Summary
*Dungeon Lord* is a hybrid game combining dungeon management with first-person grid-crawling. The player controls a monster-filled dungeon and alternates between two connected perspectives on the same underlying dungeon:
- **Builder Mode**: Top-down / isometric view for constructing rooms, corridors, traps, and monster garrisons.
- **Crawl Mode**: First-person grid-based crawler view for walking the dungeon, fighting invaders directly, and possessing garrisoned monsters.

Defeating adventuring parties ("invaders") yields **Essence**, the primary resource used to expand and upgrade the dungeon. The Dungeon Lord and Dungeon each level independently using a shared percentage-based growth formula.

Version 1 scope is strictly contained to the dungeon itself (single-player, dungeon-only). Overworld exploration and multiplayer are deferred to future phases.

---

## 2. Core Pillars & Design Influences
Design influences include LitRPG subgenres (e.g. *Dungeon Core / Dungeon Lord*, *The Divine Dungeon*) and classic grid-crawlers (*Dungeon Keeper*, *Legend of Grimrock*).

| Pillar | Description |
|---|---|
| **Dual Perspective, One Dungeon** | Builder Mode and Crawl Mode render the same grid data model. What you build top-down is what you walk in first-person. |
| **Real Personal Stakes** | As an embodied Lord, the player can fight, take damage, and die in first-person combat—not just manage abstractly. |
| **Two-Track Progression** | The Lord levels personally; the Dungeon ranks separately. Both matter and share the growth formula. |
| **Contained Replayable Loop** | Dungeon-only scope: build → defend → harvest Essence → expand → repeat. |

---

## 3. Player Roles & Modes

### 3.1 Dungeon Lord (Player Character)
- Physical body with personal stats, equipment, and combat abilities.
- Explores dungeon in Crawl Mode, fighting invaders directly.
- Issues building/management commands via Builder Mode or in-dungeon interface.
- Vulnerable to death in combat.

### 3.2 Builder Mode (Top-Down / Isometric)
- Grid-based placement of rooms, corridors, traps, and monster spawn points.
- Essence expenditure for construction, upgrades, and monster garrisons.
- View dungeon-wide metrics: rank, Essence storage, room capacity, settlement reputation.

### 3.3 Crawl Mode (First-Person Grid Crawler)
- Discrete tile-based movement and turning (Dungeon Master / Legend of Grimrock style).
- Direct real-time combat as the Dungeon Lord against invaders.
- Uses identical grid data model as Builder Mode (zero separate level duplication).

### 3.4 Possession Mode (Secondary Ability)
- Allows the Lord to temporarily see/act through a garrisoned monster's senses.
- Useful for scouting, remote defense, and tactical observation.

---

## 4. Systems Architecture & Progression Formula

### 4.1 Shared Growth Formula
Both the Dungeon Lord and Dungeon Rank follow a unified additive formula to keep long-term scaling predictable and tuneable:

$$\text{attribute} = \text{base} \times (1 + 0.01 \times \text{level} + \text{milestone\_bonus}(\text{level}))$$

- **Every level**: $+1\%$ to all attributes (accumulates linearly).
- **Every 10th level**: $+10\%$ milestone bonus.
- **Every 25th level**: $+25\%$ milestone bonus (replaces the 10-level milestone on shared levels).

### 4.2 Essence Economy & Dungeon Rank
- **Essence**: Primary currency harvested from defeated invaders. Spent on building, room upgrades, trap placement, and monster summoning.
- **Dungeon Rank**: Unlocks new room types, higher monster tier caps, and floor expansions. Rank-up thresholds are Essence-gated.

### 4.3 Invader Simulation
- Adventuring parties generated based on settlement reputation and dungeon rank.
- Pathfinding: Grid-based pathfinding through the shared dungeon data model.
- Difficulty & Rewards: Settlement hostility scales invader strength, yielding higher Essence upon defeat.

---

## 5. Technical Stack

| Area | Choice |
|---|---|
| **Engine** | Godot 4 |
| **Language** | C# |
| **Data Model** | Single shared grid structure (`GridMap` / `TileMap`) |
| **Movement** | Discrete tile movement & turning, tweened between cells |
| **Leveling System** | Config-driven, shared formula module |