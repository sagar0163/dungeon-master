# SPEC: Dungeon Master — AI-Powered TTRPG Assistant

## Vision
An AI-powered Dungeon Master for tabletop RPGs that handles dynamic story generation, NPC management, combat encounter balancing, and world state persistence across sessions.

## Core User Journey
1. **Campaign Setup**: User describes setting/concept → AI builds persistent World Codex (locations, factions, NPCs, lore, timeline)
2. **Session Play**: User provides scene/encounter prompts → AI generates narrative, manages NPCs, runs combat
3. **State Persistence**: World state auto-saved between sessions with full continuity
4. **Improvisation**: User throws curveballs → AI adapts world state and narrates consequences
5. **Export**: Session logs, campaign notes, character sheets exportable

## Functional Requirements

### 1. World Architect (Campaign Creation)
- **FR-1.1**: Natural language campaign concept → structured World Codex (locations, factions, NPCs, lore, timeline, magic systems)
- **FR-1.2**: Context-aware Q&A about world state
- **FR-1.3**: Proactive suggestions (plot hooks, faction movements, rumor generation)

### 2. Session Engine (Runtime Play)
- **FR-2.1**: Scene narration with full Codex context
- **FR-2.2**: NPC dialogue and behavior (personalities, goals, secrets)
- **FR-2.3**: Combat encounter management (initiative, HP, abilities, balancing)
- **FR-2.4**: Dice rolling integration with narrative results

### 3. State Management & Continuity
- **FR-3.1**: Persistent world state across sessions (PostgreSQL + pgvector)
- **FR-3.2**: Timeline and causality tracking
- **FR-3.3**: Ripple checker: detects contradictions (NPC knowledge, location changes, faction relations)
- **FR-3.4**: "What breaks if X happens?" impact analysis

### 4. Improvisation Tools
- **FR-4.1**: Alternative narrative branches for any scene
- **FR-4.2**: "Yes, and..." / "No, but..." response modes
- **FR-4.3**: Random tables integration (encounters, loot, names, rumors)

### 5. Export & Tooling
- **FR-5.1**: Session log export (Markdown, PDF)
- **FR-5.2**: Campaign wiki export (HTML)
- **FR-5.3**: Character sheet export (JSON, PDF)
- **FR-5.4**: Mermaid relationship graphs for factions/NPCs

## Non-Functional Requirements
- **NFR-1**: Sub-10s scene generation
- **NFR-2**: Sub-3s combat turn resolution
- **NFR-3**: Zero hallucination of Codex facts
- **NFR-4**: <500ms state query latency

## Acceptance Criteria
- [ ] User can go from "fantasy city intrigue campaign" to playable session 1
- [ ] User can say "I talk to the guard" → AI runs NPC with personality
- [ ] User can say "I attack the guard" → AI runs combat with balanced encounter
- [ ] User can change "the king is dead" → system shows what breaks
- [ ] Export produces usable campaign notes

## Technical Approach
- FastAPI + LangGraph pipeline (extend Nebula-Writer patterns)
- Supabase PostgreSQL + pgvector (reuse Nebula-Writer infra)
- React/Vue frontend (extend Nebula-Writer comment UI for dice/chat)
- LLM fallback chain: Mistral → Gemini → OpenAI