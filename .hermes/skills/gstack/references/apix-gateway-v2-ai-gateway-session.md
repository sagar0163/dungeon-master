# APIX Gateway v2.0 AI Gateway Session — Learnings & Patterns

**Date:** 2026-07-12  
**Project:** APIX Gateway (Node.js/Express, 35+ plugins)  
**Milestone:** T20-04b llm-token-counter plugin (v2.0 AI Gateway wedge)

---

## Session Summary

Completed full gstack workflow for v2.0 AI Gateway first plugin:
`/office-hours → /plan-eng-review → /spec → implementation → /review → /qa → /plan-ceo-review`

**Delivered:** `llm-token-counter` plugin (490 lines) with:
- SSE parsing (OpenAI, Anthropic, Azure, Cohere, generic)
- Incremental tiktoken counting (partial UTF-8 handling)
- Budget enforcement (reject/truncate/passthrough modes)
- AIContext propagation for downstream plugins
- Response headers for observability

---

## Key Learnings for gstack Workflow

### 1. gstack-qa Adaptation for Backend/API Projects

**Issue:** gstack-qa skill is designed for browser-based web apps (UI, forms, navigation). API Gateway has no UI — it's a proxy service.

**Adaptation used:**
- Ran existing unit/integration test suite (`npm test`) as primary verification
- Verified plugin loads via `pluginManager.getPlugin('llm-token-counter')`
- Confirmed tiktoken integration works (`"Hello world" = 2 tokens`)
- Gateway startup test: `timeout 10 node src/index.js` — all 35+ plugins load

**Recommendation:** Add "backend service mode" to gstack-qa that:
- Skips browser/browse steps
- Runs test suite as primary verification
- Validates service startup + plugin loading
- Checks health endpoints if present

---

### 2. gstack-review Found Config/Doc Issues, Not Code Bugs

**Findings (3 informational):**
1. Missing Joi validation schema for new plugin config in `src/utils/config.js`
2. Route config completely replaced (4 routes → 2 routes) — verify intentional
3. Budget enforcement uses `totalTokens` not `completionTokens` — may exceed budget prematurely

**Pattern:** For plugin-based architectures, review should also check:
- Config schema registration
- Route/plugin reference consistency
- Backward compatibility of config changes

---

### 3. Package.json Conflict Resolution During Rebase

**Issue:** Remote main had `redis: ^5.11.0`, local had `redis: ^4.7.0 + tiktoken: ^1.0.22`. Rebase conflicted.

**Resolution:** Accept remote's newer redis, keep local's tiktoken addition. Manual edit of package.json resolved cleanly.

**Tip:** For plugin additions, commit package.json changes atomically with plugin code to minimize rebase.

---

### 4. CEO Review Validated Wedge Strategy

**Office-hours output** identified Marcus (Healthtech VP Eng, HIPAA, SOC2 Q3, $150K budget) as reference customer. Wedge scope locked to 7 plugins for HIPAA/SOC2 teams — explicitly excluding RAG, fine-tuning, RBAC, HA.

**Value:** Forced specificity prevented scope creep. Kong AI Gateway threat analysis (6-9 month window) created urgency.

---

## Reusable Patterns

### Plugin Implementation Template (for future AI plugins)

```javascript
// src/plugins/builtins/llm-*.js
export default {
  name: 'llm-*',
  version: '1.0.0',
  phase: 'postProxy', // or 'preProxy'
  priority: 100,
  defaultOptions: { /* ... */ },
  async handler(req, res, next) {
    // 1. Extract provider/model from headers
    // 2. Initialize AIContext on req.aiContext
    // 3. Override res.write/res.end for streaming interception
    // 4. Update AIContext incrementally
    // 5. Enforce budgets/limits with response headers
    // 6. Call next()
  }
}
```

### AIContext Schema (Standardized for v2.0)

```typescript
interface AIContext {
  provider: 'openai' | 'anthropic' | 'azure' | 'cohere' | 'unknown';
  model: string;
  promptTokens: number;
  completionTokens: number;
  totalTokens: number;
  costUSD: number;
  cached: boolean;
  injectionDetected: boolean;
  piiRedacted: boolean;
  modelFallback: { from: string; to: string; reason: string } | null;
  firstTokenLatencyMs?: number;
}
```

### Test Verification Checklist for AI Plugins

- [ ] Plugin loads via `pluginManager.getPlugin('name')`
- [ ] tiktoken encodes/decodes correctly
- [ ] SSE parser handles: partial chunks, `[DONE]`, malformed JSON, provider formats
- [ ] Budget enforcement returns 429 with `Retry-After` header
- [ ] AIContext propagated to `req.aiContext`
- [ ] Response headers include budget/token info
- [ ] Core test suite passes (no regressions)

---

## Commands Reference

```bash
# Run core tests (skip Redis-dependent)
npm test -- --exclude test/gateway.test.js --exclude test/circuit-breaker-fix.test.js --exclude test/load-balancer-hardened.test.js

# Verify plugin loads
node -e "
import { pluginManager } from './src/plugins/index.js';
await pluginManager.loadBuiltInPlugins();
pluginManager.enable('llm-token-counter');
console.log(pluginManager.getPlugin('llm-token-counter') ? 'LOADED' : 'MISSING');
"

# Lint new files only
npx eslint src/plugins/builtins/llm-token-counter.js eslint.config.js

# Gateway startup test
timeout 10 node src/index.js
```

---

## Session Artifacts

| Artifact | Location |
|----------|----------|
| CEO design doc | `~/.gstack/projects/apix-gateway/office-hours-ai-gateway-v2.md` |
| Spec (3 files) | `.gstack/specs/llm-token-counter/{spec.md,tasks.md,acceptance.md}` |
| Implementation | `src/plugins/builtins/llm-token-counter.js` |
| Config | `plugins.json` (llm-token-counter section) |
| Review output | `/tmp/gstack-review-final.md` |

---

## Next Steps (for /ship)

1. Resolve rebase conflicts (package.json done)
2. Add Joi schema to `src/utils/config.js` for llm-token-counter
3. Document budget logic decision (totalTokens vs completionTokens) in CHANGELOG
4. Run `/ship` — will create PR, run CI, merge to main