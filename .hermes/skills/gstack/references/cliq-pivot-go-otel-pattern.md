# cliq Pivot: Go PTY Wrapper + OTel Token Tracking

## Context
The `cliq` project pivoted from "ad injection during AI wait time" to "unified AI token observability." This reference captures the implementation pattern for future similar pivots.

## Core Pattern: PTY Wrapper → OTel Spans

### Architecture
```
User runs:  cliq run -- claude -p "write code"
            │
            ▼
        PTY intercepts stdin/stdout
            │
            ▼
    Parse streaming JSON for token usage (per agent)
            │
            ▼
    Emit OTel spans via OTLP HTTP → local collector
```

### Key Files Created
| File | Purpose |
|------|---------|
| `wrapper/wrapper.go` | PTY wrapper with OTel emission, 9 agent parsers |
| `detect/detect.go` | Auto-detects agent from command line |
| `config/config.go` | OTel endpoint + collector config |
| `cmd/run.go` | Simplified CLI entry point |

### Agent Parsers (9 implemented)
| Agent | Regex Pattern | Token Fields |
|-------|---------------|--------------|
| Claude Code | `"usage":{"input_tokens":N,"output_tokens":N,"cache_read_tokens":N}` | input, output, cache_read, cache_write |
| Aider | `Tokens: N (N in, N out)` | total, input, output |
| Ollama | `"eval_count":N,"prompt_eval_count":N` | output, input |
| llama.cpp | `eval_count=N prompt_eval_count=N` | output, input |
| Codex/OpenCode/Gemini | Generic JSON `usage` field | input, output |

### OTel Span Definitions
```go
// Session lifecycle
agent.session.start   {cliq.command, cliq.agent}
agent.session.end     {cliq.status}
agent.idle.start/end  {no attrs}

// Per-turn token usage
agent.turn            {gen_ai.system, gen_ai.request.model, 
                       gen_ai.usage.input_tokens, gen_ai.usage.output_tokens,
                       gen_ai.usage.cache_read_tokens, gen_ai.usage.cache_write_tokens}
```

### Dependencies (go.mod)
```go
go.opentelemetry.io/otel v1.27.0
go.opentelemetry.io/otel/exporters/otlp/otlptrace/otlptracehttp v1.27.0
go.opentelemetry.io/otel/sdk v1.27.0
go.opentelemetry.io/otel/semconv v1.27.0
```

### Build & Test
```bash
go mod tidy
go build -o cliq .
./cliq run -- echo "test"  # CI/non-interactive passthrough
```

## Pivot Lessons

### What to Delete (Week 1 of Sprint)
- `ad/` — all ad network integrations
- `backend/cashfree.go`, `backend/server.go` — payout infrastructure
- `cmd/auth.go`, `cmd/balance.go`, `cmd/withdraw.go` — auth/payout CLI
- `renderer/`, `tracker/`, `client/` — ad rendering + impression tracking

### What to Keep
- `wrapper/` PTY logic (core asset)
- `detect/` agent detection (core asset)  
- `terminal/` raw mode handling (core asset)
- `config/` config loading (adapt for OTel)

### OTel Version Hell (Avoid)
The `go.opentelemetry.io/otel/semconv` package has **no standalone module** — it's embedded in `go.opentelemetry.io/otel`. Import as:
```go
semconv "go.opentelemetry.io/otel/semconv/v1.24.0"  // or v1.27.0
```
Not as a separate `go.mod` require line. The `semconv/v1.x.y` path is internal to the otel module.

## Validation Sprint Integration
This pivot was driven by gstack CEO/Eng reviews. The **validation sprint** (see `references/gstack-validation-sprint-pattern.md`) must run before implementing the full Eng Review architecture (ClickHouse, TUI, VS Code ext, etc.).