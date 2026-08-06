# gstack Validation Sprint Pattern

## Context
After running gstack CEO/Eng reviews on a project pivot, the output is architecture docs (CEO_REVIEW.md, ENG_REVIEW.md). The **critical next step** is NOT implementation — it's a **30-day validation sprint** to prove willingness-to-pay before building.

## Pattern
```
CEO Review → Eng Review → SPRINT.md + BUYER_LIST.md → Validation → Go/No-Go → Implementation
```

## Sprint Structure (30 days)

| Week | Focus | Gate |
|------|-------|------|
| 1 | Minimal prototype + buyer list | Prototype emits valid OTel; 10 buyers identified |
| 2 | 10 buyer calls | 5/10 say "I'd pay $500/mo" |
| 3 | Framework outreach + VS Code spike | 3/5 frameworks respond; status bar works |
| 4 | Metrics review | All kill criteria measurable |

## Kill Criteria (any = stop)
- 0/10 buyers express willingness to pay at any price
- OTel emission unreliable (>5% span loss under load)
- Unit economics fail at $500/mo (CAC > 6-month payback)
- 0 framework partnership responses
- Collector can't handle 10K spans/sec on modest hardware

## Sprint Artifacts
- `SPRINT.md` — week-by-week plan with timeboxes
- `SPRINT_TRACKER.md` — daily tracking with gates
- `BUYER_LIST.md` — 10 target buyers tiered by pain level
- `VALIDATION.md` — scorecard for buyer calls

## Common Mistake (Anti-Pattern)
> **Building the Eng Review architecture before validation**
> 
> The Eng Review produces a beautiful 1,700-line spec. The temptation is to start coding it. **Don't.** The spec assumes the product is viable. The sprint proves it.

## When to Use
- After any gstack CEO/Eng review that produces a pivot or new product direction
- When the team wants to "start building" but hasn't proven willingness-to-pay
- When the eng review spec is >500 lines (over-engineered for validation)

## gstack Skill Integration
This pattern fits between `/plan-eng-review` and `/ship` in the gstack workflow:
```
/plan-ceo-review → /plan-eng-review → [VALIDATION SPRINT] → /ship
```

The validation sprint is the "Plan" phase of Plan → Build → Ship, but focused on **market validation** not technical planning.