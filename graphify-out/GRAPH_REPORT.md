# Graph Report - dungeon_master  (2026-08-06)

## Corpus Check
- 6 files · ~657 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 26 nodes · 21 edges · 6 communities (4 shown, 2 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `c5b94bce`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SPEC: Dungeon Master — AI-Powered TTRPG Assistant
- Functional Requirements
- Dungeon Master
- __init__.py
- dungeon-master

## God Nodes (most connected - your core abstractions)
1. `SPEC: Dungeon Master — AI-Powered TTRPG Assistant` - 7 edges
2. `Functional Requirements` - 6 edges
3. `Dungeon Master` - 3 edges
4. `Dungeon Master - AI-powered TTRPG assistant` - 1 edges
5. `Features` - 1 edges
6. `Setup` - 1 edges
7. `Vision` - 1 edges
8. `Core User Journey` - 1 edges
9. `1. World Architect (Campaign Creation)` - 1 edges
10. `2. Session Engine (Runtime Play)` - 1 edges

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Import Cycles
- None detected.

## Communities (6 total, 2 thin omitted)

### Community 0 - "SPEC: Dungeon Master — AI-Powered TTRPG Assistant"
Cohesion: 0.29
Nodes (6): Acceptance Criteria, Core User Journey, Non-Functional Requirements, SPEC: Dungeon Master — AI-Powered TTRPG Assistant, Technical Approach, Vision

### Community 2 - "Functional Requirements"
Cohesion: 0.33
Nodes (6): 1. World Architect (Campaign Creation), 2. Session Engine (Runtime Play), 3. State Management & Continuity, 4. Improvisation Tools, 5. Export & Tooling, Functional Requirements

### Community 3 - "Dungeon Master"
Cohesion: 0.50
Nodes (3): Dungeon Master, Features, Setup

## Knowledge Gaps
- **13 isolated node(s):** `dungeon-master`, `Features`, `Setup`, `Vision`, `Core User Journey` (+8 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **2 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SPEC: Dungeon Master — AI-Powered TTRPG Assistant` connect `SPEC: Dungeon Master — AI-Powered TTRPG Assistant` to `Functional Requirements`?**
  _High betweenness centrality (0.170) - this node is a cross-community bridge._
- **Why does `Functional Requirements` connect `Functional Requirements` to `SPEC: Dungeon Master — AI-Powered TTRPG Assistant`?**
  _High betweenness centrality (0.150) - this node is a cross-community bridge._
- **What connects `dungeon-master`, `Features`, `Setup` to the rest of the system?**
  _13 weakly-connected nodes found - possible documentation gaps or missing edges._