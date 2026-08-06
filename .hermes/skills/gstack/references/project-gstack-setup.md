# Project gstack Setup Patterns

## Overview
This reference documents the common patterns for making gstack skills available in a project. Choose one approach per project.

---

## Pattern 1: Global Hermes Installation (Default)
**Status:** ✅ Works immediately, zero config
**Location:** `~/.hermes/skills/gstack/`
**Scope:** All projects in this Hermes profile

**Pros:**
- No repo changes needed
- Skills always up to date with `hermes skills upgrade` or `gstack-upgrade`
- No repo bloat

**Cons:**
- Skills not pinned to project — different machines may have different versions
- No project-specific skill customizations

**When to use:** Solo projects, quick experiments, or when you trust the global version.

---

## Pattern 2: Git Submodule (Recommended for Teams)
**Status:** ✅ Version-pinned, shareable
**Setup:**
```bash
# From project root
git submodule add https://github.com/<owner>/gstack.git .hermes/skills/gstack
git commit -m "chore: add gstack as submodule"
```

**Update:**
```bash
cd .hermes/skills/gstack && git pull origin main
cd ../.. && git add .hermes/skills/gstack && git commit -m "chore: update gstack submodule"
```

**Pros:**
- Exact version pinned in repo
- All contributors get same skills
- Can track custom forks

**Cons:**
- Adds ~200MB+ to clone (shallow submodule helps)
- Requires submodule init on fresh clone: `git submodule update --init --recursive`

**When to use:** Team projects, CI/CD, when skill version must be reproducible.

---

## Pattern 3: Vendored Copy (Deprecated)
**Status:** ⚠️ Discouraged — gstack router detects and warns
**Location:** `.hermes/skills/gstack/` (copied, not symlinked, not a submodule)

**Problems:**
- Bloats repo (200MB+)
- Hard to update — manual copy/paste
- Diverges from upstream
- gstack preamble detects this (`VENDORED_GSTACK: yes`) and prompts migration

**Migration (auto-offered by gstack):**
```bash
git rm -r .hermes/skills/gstack/
echo '.hermes/skills/gstack/' >> .gitignore
# Run gstack-team-init (see Pattern 4)
```

---

## Pattern 4: Team Mode (gstack-team-init)
**Status:** ✅ Modern recommended approach
**Setup:**
```bash
# After removing vendored copy or starting fresh
cd ~/.hermes/skills/gstack && ./setup --team
# Or: gstack-team-init required
```

**What it does:**
- Creates `.gbrain-source` pin file in project root
- Configures shared artifacts sync across machines
- Each developer runs setup once

**Pros:**
- No repo bloat
- Cross-machine artifact sync (CEO plans, designs, reports)
- Skills stay in global location but project-scoped

**Cons:**
- Requires gbrain setup (optional but recommended)
- Each developer must run setup

---

## Nebula Writer 2 Current State (as of this session)

| Indicator | Value |
|-----------|-------|
| Global gstack | ✅ Installed at `~/.hermes/skills/gstack/` |
| Local gstack dir | ❌ Does not exist |
| Git submodule | ❌ Not configured |
| `.gitmodules` | ❌ Not present |
| Vendored copy | ❌ Not present |
| Docs reference | `E:\\my project folder\\Nebula-Writer-2\\gstack\\` (stale Windows path) |
| PROJECT_STATE.md note | "Clean up gstack — It's 200MB+ in repo; consider git submodule or separate repo" |

**Recommendation for this project:** Pattern 2 (git submodule) or Pattern 4 (team mode) — the repo already references gstack in docs and the maintainer noted the 200MB concern.

---

## Session Update: gstack + graphify Setup (2026-08-03)

**What was done:**
- Verified global gstack at `~/.hermes/skills/gstack/` is functional (80+ skills)
- Added gstack skill routing rules to `AGENTS.md` (both project root and `nebula writer 2/`)
- Installed graphify CLI via `uv tool install graphifyy` (v0.9.18)
- Registered graphify skill for Hermes: `graphify hermes install` → updates AGENTS.md
- Built knowledge graph: `graphify . --code-only` + `graphify cluster-only .` → `graphify-out/` (1,314 nodes, 2,506 edges, 93 communities)
- Added `graphify-out/` to `.gitignore`

**Resulting AGENTS.md additions:**
```markdown
## graphify
[...rules for query/path/explain/update...]

## Skill routing
[...13 routing rules for gstack skills...]
```

**Verification:** Core imports work, graphify queries return structured results, gstack skills available globally.

---

## Session Update: QA Testing & Bug Fixes (2026-08-03)

**Ran `/skill qa` on Nebula Writer 2** — systematically tested all API endpoints and fixed 9 bugs:

### Bugs Fixed
1. **Missing `timedelta` import** — `main.py:9`
2. **Async AI write status missing `task_id`** — response validation error
3. **Sync functions calling async methods** — `/api/ai/rewrite`, `/api/ai/describe`, `/api/ai/show-not-tell`
4. **Missing `json` import** — `/api/templates` endpoint
5. **MemorySystem crash without Supabase** — try/except fallback in `conversation.py`
6. **Duplicate dict key `rate_limit`** — `models.py:959`
7. **Triple `close()` method in postgres_db.py** — consolidated to one
8. **Redefined methods in supabase_db.py** — `get_lookahead_cards`, `add_lookahead_card`
9. **Unused `ebooklib` import** — `exporter.py`

### Verification Results
All 16 API endpoints working:
- Core: health, entities, chapters, stats, export (mermaid/json)
- Templates, plot-threads, foreshadowing, world-rules
- AI: async write (~26s), rewrite, describe, show-not-tell
- Chat (stream=false), onboarding codex generation

### Code Quality
- `ruff check . --fix --unsafe-fixes` → 0 errors (was 203)
- Fixed: W293, I001, F541, F841, F401, F811, F601, W292

### References Created
- `gstack-qa/references/nebula-writer-2-qa-session.md` — full bug list, verification commands
- `gstack-qa/references/nebula-writer-2-graphify-setup.md` — graphify installation + usage

---

## Quick Commands Reference

```bash
# Check current gstack mode in any project
/skill gstack
# Look for: VENDORED_GSTACK, REPO_MODE, GBrain configured

# Add as submodule (run from project root)
git submodule add https://github.com/garryslist/gstack.git .hermes/skills/gstack

# Init submodule on fresh clone
git submodule update --init --recursive

# Upgrade global gstack
/skill gstack-upgrade

# Migrate from vendored to team mode (if VENDORED_GSTACK=yes)
# gstack router will prompt automatically, or run manually:
git rm -r .hermes/skills/gstack/
echo '.hermes/skills/gstack/' >> .gitignore
cd ~/.hermes/skills/gstack && ./setup --team
```