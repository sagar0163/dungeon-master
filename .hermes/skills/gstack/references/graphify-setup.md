# graphify Setup for Hermes Projects

## Overview
This reference documents the pattern for setting up graphify knowledge graph in a Hermes project. graphify converts codebases into queryable knowledge graphs (entities, relationships, communities) that agents can query instead of raw grep.

---

## Installation

```bash
# Install CLI (recommended: isolated via uv)
uv tool install graphifyy

# Verify
graphify --version
# graphify 0.9.x

# Register skill for Hermes
graphify hermes install
# Writes graphify rules to AGENTS.md
```

---

## Building the Knowledge Graph

```bash
# From project root

# Option A: Code-only (no LLM API key needed)
graphify . --code-only
# AST extraction only, skips docs/papers/images

# Option B: Full extraction (requires LLM API key)
# Set GEMINI_API_KEY, OPENAI_API_KEY, ANTHROPIC_API_KEY, etc.
graphify .

# Generate community labels + reports
graphify cluster-only .
# Creates GRAPH_REPORT.md, graph.html, updates graph.json
```

---

## Output Structure (`graphify-out/`)

| File | Purpose |
|------|---------|
| `graph.json` | Full graph (nodes, edges, communities) |
| `GRAPH_REPORT.md` | Human-readable architecture summary |
| `graph.html` | Interactive graph visualization |
| `.graphify_analysis.json` | Extraction metadata |
| `.graphify_labels.json` | Community labels |
| `cache/` | Incremental update cache |
| `wiki/` | (Optional) Generated wiki if enabled |

**Add to `.gitignore`:**
```
graphify-out/
```

---

## Querying the Graph

```bash
# Natural language query → scoped subgraph
graphify query "How does the LLM fallback chain work?"

# Path between two symbols
graphify path "AIWriter" "QualityEngine"

# Explain a concept
graphify explain "NarrativeStateEngine"
```

---

## AGENTS.md Rules (Auto-added by `graphify hermes install`)

```markdown
## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

When the user types `/graphify`, use the installed graphify skill or instructions before doing anything else.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- Dirty graphify-out/ files are expected after hooks or incremental updates; dirty graph files are not a reason to skip graphify. Only skip graphify if the task is about stale or incorrect graph output, or the user explicitly says not to use it.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
```

---

## Maintenance

```bash
# After code changes - incremental AST update (fast, no API cost)
graphify update .

# Full rebuild
graphify . --code-only && graphify cluster-only .

# Check graph health
graphify query "project structure"
```

---

## Nebula Writer 2 Session (2026-08-03)

**Commands run:**
```bash
cd /home/sagar-jadhav/Documents/my project/nebula writer 2
uv tool install graphifyy          # v0.9.18
graphify hermes install            # Updated AGENTS.md
graphify . --code-only             # 1,314 nodes, 2,506 edges
graphify cluster-only .            # 93 communities, GRAPH_REPORT.md
echo "graphify-out/" >> .gitignore # Added to ignore
```

**Verification:**
- Core imports work
- `graphify query "..."` returns structured subgraphs
- Graph HTML report viewable in browser