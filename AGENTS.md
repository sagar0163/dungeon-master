# AGENTS.md — Dungeon Master Project

## Project Overview
AI-powered Dungeon Master for tabletop RPGs. FastAPI backend with LangGraph pipelines, Supabase PostgreSQL + pgvector, React frontend.

## Quick Commands
```bash
# Backend
cd /home/sagar-jadhav/Documents/my\ project/dungeon_master
source .venv/bin/activate
python -m dungeon_master.main

# Tests
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

## Project Structure
```
dungeon_master/
├── SPEC.md                    # Requirements specification
├── pyproject.toml             # Python project config
├── requirements.txt           # Dependencies
├── .gitignore
├── README.md
├── graphify-out/              # Knowledge graph
├── .hermes/skills/gstack/     # Vendored gstack skills
├── dungeon_master/
│   ├── __init__.py
│   └── main.py               # FastAPI app
└── tests/
    └── test_main.py
```

## Architecture Notes
- Reuses Nebula-Writer patterns: FastAPI + LangGraph, Supabase + pgvector
- Three-loop architecture planned: Generation → Validation → Continuity
- LLM fallback: Mistral → Gemini → OpenAI
- State: PostgreSQL (structured) + pgvector (semantic search)