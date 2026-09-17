# Graph Report - dungeon-master  (2026-09-12)

## Corpus Check
- 302 files · ~2,610,925 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2642 nodes · 5172 edges · 196 communities (146 shown, 50 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 116 edges (avg confidence: 0.74)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `c7572cf4`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SPEC: Dungeon Master — AI-Powered TTRPG Assistant
- main.py
- Functional Requirements
- Dungeon Master
- __init__.py
- dungeon-master
- cookie-import-browser.ts
- BrowseClient
- gstack-memory-ingest.ts
- gstack-gbrain-sync.ts
- browser-skill-commands.ts
- BrowserManager
- Core QA Patterns
- token-registry.ts
- meta-commands.ts
- server.ts
- domain-skill-commands.ts
- write-commands.ts
- cli.ts
- security.ts
- cdp-bridge.ts
- TabSession
- content-security.ts
- server-embedder-terminal-port.test.ts
- security-classifier.ts
- terminal-agent.ts
- BRD: Dungeon Master — AI-Powered TTRPG Game Engine
- SKILL.md
- buildFetchHandler
- mkdirSecure
- buffers.ts
- read-commands.ts
- cdp-inspector.ts
- socks-bridge.ts
- browser-manager.ts
- Coordinated Workflow — How Skills Complement Each Other
- gstack-brain-context-load.ts
- activity.ts
- network-capture.ts
- security-bench-ensemble.test.ts
- gstack-gbrain-supabase-provision
- cli.ts
- variants.ts
- gstack-global-discover.ts
- security-sidecar-client.ts
- stealth.ts
- compare-board.test.ts
- cookie-import-browser.test.ts
- APIX Gateway v2.0 AI Gateway Session — Learnings & Patterns
- Core Pattern: PTY Wrapper → OTel Spans
- Project gstack Setup Patterns
- gstack-gbrain-source-wireup
- config.ts
- pty-session-cookie.ts
- security-bench-ensemble-live.test.ts
- SKILL.md
- gstack-developer-profile
- proxy-config.ts
- sse-session-cookie.ts
- iterate.ts
- url-validation.ts
- gstack-brain-sync
- Plan: Snapshot Dropdown/Autocomplete Interactive Element Detection
- sanitize.ts
- xvfb.ts
- security-audit-r2.test.ts
- auth.ts
- .handoff
- resolveClaudeCommand
- security-bunnative.ts
- terminal-agent-control.ts
- gstack-gbrain-repo-policy
- memory-command.ts
- requireApiKey
- memory.ts
- graphify Setup for Hermes Projects
- gstack Validation Sprint Pattern
- gstack-detach
- pair-agent-tunnel-eval.test.ts
- gallery.ts
- Ponytail
- gstack-brain-reader
- gstack-gbrain-install
- gstack-question-preference
- security-bench.test.ts
- Ponytail Help
- SKILL.md
- SKILL.md
- SKILL.md
- SKILL.md
- gstack-codex-probe
- gstack-config
- bun-polyfill.cjs
- find-browse.ts
- pair-agent-e2e.test.ts
- gstack-artifacts-url
- file-drop.test.ts
- gstack-update-check.test.ts
- server-sanitize-surrogates.test.ts
- watchdog.test.ts
- Auto-Commit Changes
- Initialize Git Repository
- Detect Git Remote URL
- Validate Feature Branch
- from-file-path-validation.test.ts
- sidebar-tabs.test.ts
- terminal-agent-integration.test.ts
- serve.ts
- ponytail-audit.md
- Ponytail Gain
- ponytail-review.md
- SKILL.md
- SKILL.md
- SKILL.md
- gstack-first-task-detect
- gstack-gbrain-lib.sh
- gstack-gbrain-mcp-verify
- gstack-gbrain-supabase-verify
- gstack-relink
- CrawlHUD
- cli-setsid-daemonize.test.ts
- dual-listener.test.ts
- learnings-injection.test.ts
- security-classifier-download-cleanup.test.ts
- security-sidepanel-dom.test.ts
- server-auth.test.ts
- sidepanel-restart-dispose.test.ts
- telemetry.test.ts
- terminal-agent-keepalive.test.ts
- terminal-agent-watchdog.test.ts
- welcome-page.test.ts
- prototype.ts
- ponytail-debt.md
- SKILL.md
- gstack-learnings-search
- gstack-repo-mode
- gstack-settings-hook
- gstack-telemetry-log
- gstack-uninstall
- gstack-update-check
- build.test.ts
- cli-supervisor.test.ts
- gstack-config.test.ts
- security-bunnative.test.ts
- security-source-contracts.test.ts
- server-pty-lease-routes.test.ts
- sidebar-integration.test.ts
- sidebar-security.test.ts
- sidepanel-patient-autoconnect.test.ts
- sidepanel-reattach.test.ts
- terminal-agent-detach-reattach.test.ts
- terminal-agent-internal-handler.test.ts
- terminal-agent-session-routing.test.ts
- chrome-cdp
- dev-setup
- dev-teardown
- gstack-analytics
- gstack-artifacts-init
- gstack-brain-enqueue
- gstack-brain-restore
- gstack-brain-uninstall
- gstack-builder-profile
- gstack-codex-session-import
- gstack-community-dashboard
- bun-polyfill.test.ts
- pty-inject-scan.test.ts
- server-flush-trackers.test.ts
- server-tmp-state-path.test.ts
- sidebar-ux.test.ts
- state-ttl.test.ts
- calculate_attribute
- meta-commands.ts
- cdp-bridge.ts
- pty-session-lease.ts
- main
- media-extract.ts
- security-review-flow.test.ts
- DungeonLord.csproj
- sanitize.ts
- dungeon-lord-tools
- requireApiKey
- memory.ts

## God Nodes (most connected - your core abstractions)
1. `BrowserManager` - 98 edges
2. `buildFetchHandler()` - 64 edges
3. `GameManager` - 61 edges
4. `handleWriteCommand()` - 40 edges
5. `BuilderController` - 40 edges
6. `CrawlHUD` - 39 edges
7. `handleMetaCommand()` - 36 edges
8. `CrawlController` - 31 edges
9. `AbilityHUD` - 31 edges
10. `handleReadCommand()` - 30 edges

## Surprising Connections (you probably didn't know these)
- `test_advantage_roll()` --calls--> `DiceEngine`  [INFERRED]
  tests/test_dice.py → dungeon_master/dice.py
- `test_deterministic_seed_replay()` --calls--> `DiceEngine`  [INFERRED]
  tests/test_dice.py → dungeon_master/dice.py
- `test_disadvantage_roll()` --calls--> `DiceEngine`  [INFERRED]
  tests/test_dice.py → dungeon_master/dice.py
- `test_keep_highest_roll()` --calls--> `DiceEngine`  [INFERRED]
  tests/test_dice.py → dungeon_master/dice.py
- `test_standard_dice_roll()` --calls--> `DiceEngine`  [INFERRED]
  tests/test_dice.py → dungeon_master/dice.py

## Import Cycles
- None detected.

## Communities (196 total, 50 thin omitted)

### Community 0 - "SPEC: Dungeon Master — AI-Powered TTRPG Assistant"
Cohesion: 0.06
Nodes (24): AbilitySlot, Control, DungeonLord.Scripts.UI, GridContainer, bool, Dictionary, List, AbilityCategory (+16 more)

### Community 1 - "main.py"
Cohesion: 0.04
Nodes (48): 10. Compare environments, 11. Show screenshots to the user, 12. Render local HTML (no HTTP server needed), 13. Retina screenshots (deviceScaleFactor), 14. Offline render mode (rasterize your own HTML/JSON, zero network), 1. Verify a page loads correctly, 2. Test a user flow, 3. Verify an action worked (+40 more)

### Community 2 - "Functional Requirements"
Cohesion: 0.05
Nodes (81): Board, boardExpiredHtml(), boardMutex, boards, BoardState, defaultDaemonScript(), ensureDaemon(), EnsureDaemonOptions (+73 more)

### Community 3 - "Dungeon Master"
Cohesion: 0.12
Nodes (15): Architecture, Core Systems (Implemented in C#), Development Workflow, Dungeon Lord, Game Concept, Graphify (Knowledge Graph), Growth Formula (Config-Driven), Next Steps (+7 more)

### Community 4 - "__init__.py"
Cohesion: 0.07
Nodes (28): 10. Shared Growth Formula (Core Math), 11. Session Structure (v1 Scope), 1. Dual-Mode Architecture, 2. Dungeon Grid System (Single Source of Truth), 3. Essence Economy, 4. Dungeon Rank & Progression, 5. Monsters & Garrisons, 6. Invader Simulation (AI Parties) (+20 more)

### Community 5 - "dungeon-master"
Cohesion: 0.08
Nodes (19): growth_multiplier(), Compounding growth formula tests for Dungeon Lord progression system. Formula: -, Higher tier monsters/rooms should cost more Essence., Calculate compounding growth multiplier for a given level., Test the shared compounding growth formula for Lord level and Dungeon rank., Level 1: base multiplier 1.0., Level 2: 1.0 * 1.01 = 1.01., Level 10: 8 levels at +1% (1.01^8), level 10 at +11% -> 120.19709%. (+11 more)

### Community 6 - "cookie-import-browser.ts"
Cohesion: 0.06
Nodes (63): BROWSER_REGISTRY, BrowserInfo, BrowserMatch, BrowserPlatform, CdpCookie, cdpSameSite(), CHROME_PATHS_WIN, chromiumEpochToUnix() (+55 more)

### Community 7 - "BrowseClient"
Cohesion: 0.06
Nodes (11): browse, BrowseClient, BrowseClientError, BrowseClientOptions, defaultStateFile(), LazyBrowseClient, parseIntegerEnvValue(), resolveBrowseAuth() (+3 more)

### Community 8 - "gstack-memory-ingest.ts"
Cohesion: 0.08
Nodes (25): 1. Initialize Analysis Context, 2. Load Artifacts (Progressive Disclosure), 3. Build Semantic Models, 4. Detection Passes (Token-Efficient Analysis), 5. Severity Assignment, 6. Produce Compact Analysis Report, 7. Provide Next Actions, 8. Offer Remediation (+17 more)

### Community 9 - "gstack-gbrain-sync.ts"
Cohesion: 0.14
Nodes (9): Action, Dictionary, Vector3I, CrafterConfig, CrafterData, CrafterNPC, CraftingJob, RoomType (+1 more)

### Community 10 - "browser-skill-commands.ts"
Cohesion: 0.14
Nodes (24): BuildEnvOptions, buildSpawnEnv(), CappedRead, formatUsage(), handleList(), handleRm(), handleRun(), handleShow() (+16 more)

### Community 12 - "Core QA Patterns"
Cohesion: 0.18
Nodes (9): Cell, HashSet, Combatant, IEnumerable, List, InvaderWaveSimulator, WaveEvent, WaveEventType (+1 more)

### Community 13 - "token-registry.ts"
Cohesion: 0.12
Nodes (27): checkConnectRateLimit(), checkDomain(), checkRate(), checkRateLimit(), connectAttempts, createSetupKey(), createToken(), CreateTokenOptions (+19 more)

### Community 14 - "meta-commands.ts"
Cohesion: 0.18
Nodes (10): RandomNumberGenerator, float, int, IReadOnlyList, List, Vector3I, InvaderAI, InvaderMember (+2 more)

### Community 15 - "server.ts"
Cohesion: 0.07
Nodes (31): AuditEntry, initAuditLog(), writeAuditEntry(), LogEntry, NetworkEntry, BROWSE_PARENT_PID, BROWSE_PORT, browserManager (+23 more)

### Community 16 - "domain-skill-commands.ts"
Cohesion: 0.10
Nodes (45): formatSavedOk(), formatSkillListing(), handleDomainSkillCommand(), handleEdit(), handleList(), handlePromoteToGlobal(), handleRm(), handleRollback() (+37 more)

### Community 17 - "write-commands.ts"
Cohesion: 0.15
Nodes (10): IReadOnlyCollection, Queue, CrafterType, Dictionary, int, IReadOnlyList, List, CraftingManager (+2 more)

### Community 18 - "cli.ts"
Cohesion: 0.11
Nodes (21): buildRestartEnv(), config, extractGlobalFlags(), extractTabId(), generateInstructionBlock(), GlobalFlags, handlePairAgent(), hasFlag() (+13 more)

### Community 19 - "security.ts"
Cohesion: 0.09
Nodes (34): AttemptRecord, ATTEMPTS_LOG, buildTelemetrySpawnCommand(), checkCanaryInStructure(), classifyTranscript(), combineVerdict(), CombineVerdictOpts, DecisionRecord (+26 more)

### Community 20 - "cdp-bridge.ts"
Cohesion: 0.11
Nodes (13): GarrisonState, LootState, bool, Dictionary, float, int, List, Vector3I (+5 more)

### Community 21 - "TabSession"
Cohesion: 0.09
Nodes (14): handleSnapshot(), INTERACTIVE_ROLES, ParsedNode, parseLine(), parseSnapshotArgs(), SNAPSHOT_FLAGS, SnapshotOptions, TabSession (+6 more)

### Community 22 - "content-security.ts"
Cohesion: 0.50
Nodes (4): Enum, Core Pydantic data models for Dungeon Lord hybrid management & grid-crawler game, TileType, str

### Community 23 - "server-embedder-terminal-port.test.ts"
Cohesion: 0.16
Nodes (10): Action, Dictionary, int, List, Vector3I, MonsterProductionManager, ProductionJob, ProductionRecipe (+2 more)

### Community 24 - "security-classifier.ts"
Cohesion: 0.10
Nodes (25): ClassifierStatus, DEBERTA_DIR, DEBERTA_FILES, downloadFile(), ensureDebertaStaged(), ensureTestsavantStaged(), getClassifierStatus(), htmlToPlainText() (+17 more)

### Community 25 - "terminal-agent.ts"
Cohesion: 0.10
Nodes (25): appendToRingBuffer(), BROWSE_SERVER_PORT, buildReplayPayload(), buildServer(), buildTabAwarenessHint(), checkInternalAuth(), CURRENT_GEN, DETACH_WINDOW_MS (+17 more)

### Community 26 - "BRD: Dungeon Master — AI-Powered TTRPG Game Engine"
Cohesion: 0.16
Nodes (6): DungeonLord.Scripts, DungeonLord.Tests, AttackResult, CombatRules, DiceEngine, RollResult

### Community 27 - "SKILL.md"
Cohesion: 0.22
Nodes (5): Test, DungeonGridTests, DungeonGrid, DungeonTile, TileType

### Community 28 - "buildFetchHandler"
Cohesion: 0.29
Nodes (10): Lease, LEASE_TTL_MS, leaseCount(), leases, mintLease(), pruneExpired(), refreshLease(), __resetLeases() (+2 more)

### Community 29 - "mkdirSecure"
Cohesion: 0.20
Nodes (15): appendSecureFile(), mkdirSecure(), __resetWarnedForTests(), restrictDirectoryPermissions(), restrictFilePermissions(), warnIcaclsFailure(), writeSecureFile(), writeSessionState() (+7 more)

### Community 30 - "buffers.ts"
Cohesion: 0.14
Nodes (12): monsterId, Node, position, bool, Dictionary, Direction, float, List (+4 more)

### Community 31 - "read-commands.ts"
Cohesion: 0.38
Nodes (3): compare(), CompareOptions, generateCompareHtml()

### Community 32 - "cdp-inspector.ts"
Cohesion: 0.15
Nodes (17): cdpSessions, compareSpecificity(), computeSpecificity(), detachSession(), getOrCreateSession(), initializedPages, inspectElement(), InspectorResult (+9 more)

### Community 33 - "socks-bridge.ts"
Cohesion: 0.13
Nodes (15): RFC-1929, ServerConfig, BridgeHandle, buildUpstream(), parseConnectRequest(), startSocksBridge(), testUpstream(), UpstreamConfig (+7 more)

### Community 34 - "browser-manager.ts"
Cohesion: 0.12
Nodes (26): getSubscriberCount(), addConsoleEntry(), addDialogEntry(), addNetworkEntry(), consoleBuffer, dialogBuffer, DialogEntry, networkBuffer (+18 more)

### Community 35 - "Coordinated Workflow — How Skills Complement Each Other"
Cohesion: 0.11
Nodes (18): AGENTS.md — Dungeon Master Project, Architecture Notes, Coordinated Workflow — How Skills Complement Each Other, Daily Loop (Continuous), graphify, Graphify + Ponytail Synergy, Phase 1: Discover & Shape (Strategy → Spec), Phase 2: Plan & Architect (Design → Tasks) (+10 more)

### Community 36 - "gstack-brain-context-load.ts"
Cohesion: 0.15
Nodes (8): Func, IEquatable, Dictionary, IEnumerable, List, Cell, Pathfinding, PriorityQueue

### Community 37 - "activity.ts"
Cohesion: 0.11
Nodes (14): activityBuffer, ActivityEntry, ActivitySubscriber, emitActivity(), filterArgs(), getActivityAfter(), getActivityHistory(), SENSITIVE_COMMANDS (+6 more)

### Community 38 - "network-capture.ts"
Cohesion: 0.13
Nodes (7): captureBuffer, CapturedResponse, clearCapture(), createResponseListener(), exportCapture(), SizeCappedBuffer, startCapture()

### Community 39 - "security-bench-ensemble.test.ts"
Cohesion: 0.25
Nodes (7): Fixture, FIXTURE_PATH, FixtureCase, FixtureComponents, REPO_ROOT, SECURITY_LAYER_PATTERNS, securityLayerChanged()

### Community 40 - "gstack-gbrain-supabase-provision"
Cohesion: 0.29
Nodes (16): acquireServerLock(), cleanChromiumProfileLocks(), cleanupLegacyState(), ensureServer(), isServerHealthy(), killOrphanChromium(), killServer(), main() (+8 more)

### Community 41 - "cli.ts"
Cohesion: 0.20
Nodes (15): main(), openBrowser(), parseArgs(), printUsage(), publishToDaemon(), resolveImagePaths(), COMMANDS, daemonStatus() (+7 more)

### Community 42 - "variants.ts"
Cohesion: 0.17
Nodes (11): RFC-7231, briefToPrompt(), DesignBrief, parseBrief(), generateResponsiveVariants(), generateVariant(), STYLE_VARIATIONS, variants() (+3 more)

### Community 43 - "gstack-global-discover.ts"
Cohesion: 0.26
Nodes (3): Test, EssenceManagerTests, EssenceManager

### Community 44 - "security-sidecar-client.ts"
Cohesion: 0.25
Nodes (15): browseRoot(), findSecuritySidecar(), nodeOnPath(), SidecarLocation, getState(), isSidecarAvailable(), PendingRequest, processBuffer() (+7 more)

### Community 45 - "stealth.ts"
Cohesion: 0.27
Nodes (10): applyStealth(), buildGStackLaunchArgs(), buildStealthScript(), extendedModeEnabled(), HostProfile, isExtendedStealthEnabled(), readHostProfile(), STEALTH_IGNORE_DEFAULT_ARGS (+2 more)

### Community 46 - "compare-board.test.ts"
Cohesion: 0.24
Nodes (6): BrowserState, TMP_HOME, TMP_HOME, handleWriteCommand(), FIXTURES_DIR, startTestServer()

### Community 47 - "cookie-import-browser.test.ts"
Cohesion: 0.19
Nodes (12): chromiumEpoch(), createFixtureDb(), createLinuxFixtureDb(), createMacFixtureDb(), encryptCookieValue(), FIXTURE_DB, FIXTURE_DIR, IV (+4 more)

### Community 48 - "APIX Gateway v2.0 AI Gateway Session — Learnings & Patterns"
Cohesion: 0.26
Nodes (6): List, Test, PathfindingTests, x, y, z

### Community 49 - "Core Pattern: PTY Wrapper → OTel Spans"
Cohesion: 0.23
Nodes (13): collectArgsList(), collectStringList(), findNextNonBlank(), listBrowserSkills(), parseFrontmatterFields(), parseScalar(), parseSkillFile(), RawFrontmatter (+5 more)

### Community 50 - "Project gstack Setup Patterns"
Cohesion: 0.17
Nodes (8): GameMode, Dictionary, Direction, List, Vector3I, GameMode, GameSaveData, LordState

### Community 51 - "gstack-gbrain-source-wireup"
Cohesion: 0.18
Nodes (10): DiceEngine, Deterministic Dice Engine for D&D 5e SRD rules.  Supports expressions like: - "1, Roll dice according to standard notation or special modifiers., RollResult, test_advantage_roll(), test_deterministic_seed_replay(), test_disadvantage_roll(), test_keep_highest_roll() (+2 more)

### Community 52 - "config.ts"
Cohesion: 0.27
Nodes (13): resolveNodeServerScript(), resolveServerScript(), BrowseConfig, cleanSingletonLocks(), ensureStateDir(), getGitRoot(), getRemoteSlug(), readVersionHash() (+5 more)

### Community 53 - "pty-session-cookie.ts"
Cohesion: 0.24
Nodes (12): buildPtyClearCookie(), buildPtySetCookie(), extractPtyCookie(), mintPtySessionToken(), pruneExpired(), __resetPtySessions(), revokePtySessionToken(), Session (+4 more)

### Community 54 - "security-bench-ensemble-live.test.ts"
Cohesion: 0.15
Nodes (11): BenchRow, CACHE_DIR, CACHE_FILE, currentSchemaHash(), EVALS_DIR, FIXTURE_PATH, hashFile(), ML_AVAILABLE (+3 more)

### Community 55 - "SKILL.md"
Cohesion: 0.13
Nodes (14): 1. gstack-qa Adaptation for Backend/API Projects, 2. gstack-review Found Config/Doc Issues, Not Code Bugs, 3. Package.json Conflict Resolution During Rebase, 4. CEO Review Validated Wedge Strategy, AIContext Schema (Standardized for v2.0), APIX Gateway v2.0 AI Gateway Session — Learnings & Patterns, Commands Reference, Key Learnings for gstack Workflow (+6 more)

### Community 56 - "gstack-developer-profile"
Cohesion: 0.13
Nodes (14): Agent Parsers (9 implemented), Architecture, Build & Test, cliq Pivot: Go PTY Wrapper + OTel Token Tracking, Context, Core Pattern: PTY Wrapper → OTel Spans, Dependencies (go.mod), Key Files Created (+6 more)

### Community 57 - "proxy-config.ts"
Cohesion: 0.10
Nodes (7): BuilderHUD, CrawlHUD, bool, float, int, InvaderParty, GameManager

### Community 58 - "sse-session-cookie.ts"
Cohesion: 0.16
Nodes (18): buildFetchHandler(), closeTunnel(), emitInspectorEvent(), extractToken(), getTokenInfo(), grantPtyToken(), handleCommand(), handleCommandInternal() (+10 more)

### Community 59 - "iterate.ts"
Cohesion: 0.12
Nodes (20): a_star(), Grid3D, Grid system and pathfinding tests for Dungeon Lord. Grid is 3D: (x, y, floor) wi, Simple 3D grid for testing., Test A* pathfinding on dungeon grid., Path along a straight corridor., Path should go around non-walkable cells., Path between floors via stairs. (+12 more)

### Community 60 - "url-validation.ts"
Cohesion: 0.12
Nodes (26): ARIA_INJECTION_PATTERNS, BLOCKLIST_DOMAINS, cleanupHiddenMarkers(), clearContentFilters(), ContentFilter, ContentFilterResult, datamarkContent(), ensureMarker() (+18 more)

### Community 61 - "gstack-brain-sync"
Cohesion: 0.13
Nodes (14): Bugs Fixed, Code Quality, Nebula Writer 2 Current State (as of this session), Overview, Pattern 1: Global Hermes Installation (Default), Pattern 2: Git Submodule (Recommended for Teams), Pattern 3: Vendored Copy (Deprecated), Pattern 4: Team Mode (gstack-team-init) (+6 more)

### Community 62 - "Plan: Snapshot Dropdown/Autocomplete Interactive Element Detection"
Cohesion: 0.30
Nodes (3): int, Test, InvaderWaveTests

### Community 63 - "sanitize.ts"
Cohesion: 0.08
Nodes (13): DateTime, bool, Dictionary, List, ItemData, ItemDatabase, Dictionary, List (+5 more)

### Community 64 - "xvfb.ts"
Cohesion: 0.24
Nodes (15): toUpstreamConfig(), resolveNgrokAuthtoken(), start(), tmpStatePath(), cleanupXvfb(), isDisplayFree(), isOurXvfb(), pickFreeDisplay() (+7 more)

### Community 65 - "security-audit-r2.test.ts"
Cohesion: 0.17
Nodes (9): AGENT_SRC, BROWSER_MANAGER_SRC, CDP_SRC, EXTENSION_SRC, META_SRC, PATH_SECURITY_SRC, SERVER_SRC, SNAPSHOT_SRC (+1 more)

### Community 66 - "auth.ts"
Cohesion: 0.16
Nodes (8): canDispatchOverTunnel(), ServerHandle, Surface, __testInternals__, TUNNEL_COMMANDS, polyfillPath, AssertHandleFields, makeMinimalConfig()

### Community 67 - ".handoff"
Cohesion: 0.26
Nodes (10): buildSseClearCookie(), buildSseSetCookie(), extractSseCookie(), mintSseSessionToken(), pruneExpired(), __resetSseSessions(), Session, sessions (+2 more)

### Community 68 - "resolveClaudeCommand"
Cohesion: 0.31
Nodes (9): ClaudeCommand, parseOverrideArgs(), resolveClaudeBinary(), resolveClaudeCommand(), stripWrappingQuotes(), checkHaikuAvailable(), checkTranscript(), EMPTY_ENV (+1 more)

### Community 69 - "security-bunnative.ts"
Cohesion: 0.27
Nodes (10): benchClassify(), classify(), ClassifyResult, encodeWordPiece(), getCachedTokenizer(), HFTokenizerConfig, LatencyReport, loadHFTokenizer() (+2 more)

### Community 70 - "terminal-agent-control.ts"
Cohesion: 0.36
Nodes (9): AgentRecord, agentRecordPath(), clearAgentRecord(), killAgentByRecord(), readAgentRecord(), resolveTerminalAgentScript(), spawnTerminalAgent(), writeAgentRecord() (+1 more)

### Community 71 - "gstack-gbrain-repo-policy"
Cohesion: 0.19
Nodes (7): sanitizeBody(), stripLoneSurrogateEscapes(), buildCommandResponse(), createSseEndpoint(), sanitizeReplacer(), SseEndpointConfig, SseSender

### Community 72 - "memory-command.ts"
Cohesion: 0.14
Nodes (13): 1. Executive Summary, 2. Core Pillars & Design Influences, 3.1 Dungeon Lord (Player Character), 3.2 Builder Mode (Top-Down / Isometric), 3.3 Crawl Mode (First-Person Grid Crawler), 3.4 Possession Mode (Secondary Ability), 3. Player Roles & Modes, 4.1 Shared Growth Formula (+5 more)

### Community 73 - "requireApiKey"
Cohesion: 0.43
Nodes (7): buildAccumulatedPrompt(), callFresh(), callWithThreading(), iterate(), IterateOptions, readSession(), updateSession()

### Community 74 - "memory.ts"
Cohesion: 0.10
Nodes (13): BuildTool, InputEvent, InputEventKey, MeshInstance3D, bool, Camera3D, int, string (+5 more)

### Community 75 - "graphify Setup for Hermes Projects"
Cohesion: 0.14
Nodes (13): Artifacts Sync (skill start), Completion Status Protocol, First-run guidance (one-time), Model-Specific Behavioral Patch (claude), Operational Self-Improvement, Plan Mode Safe Operations, Plan Status Footer, Preamble (run first) (+5 more)

### Community 76 - "gstack Validation Sprint Pattern"
Cohesion: 0.31
Nodes (4): float, Test, LevelingEngineTests, LevelingEngine

### Community 77 - "gstack-detach"
Cohesion: 0.32
Nodes (10): ApiKeyResolution, ApiKeySource, configPath(), describeApiKeySource(), matchingCwdEnvFile(), readEnvValue(), resolveApiKey(), resolveApiKeyInfo() (+2 more)

### Community 78 - "pair-agent-tunnel-eval.test.ts"
Cohesion: 0.28
Nodes (6): DaemonHandle, ROOT, SERVER_ENTRY, spawnDaemonWithTunnel(), waitForReady(), waitForTunnelPort()

### Community 79 - "gallery.ts"
Cohesion: 0.33
Nodes (6): escapeHtml(), gallery(), GalleryOptions, generateEmptyGallery(), generateGalleryHtml(), SessionData

### Community 80 - "Ponytail"
Cohesion: 0.28
Nodes (11): spawnSkill(), MetaCommandOpts, DEFAULT_SKILL_SCOPES, generateSpawnId(), mintSkillToken(), MintSkillTokenOptions, revokeSkillToken(), skillClientId() (+3 more)

### Community 82 - "gstack-brain-reader"
Cohesion: 0.30
Nodes (9): SkillCommandContext, commitSkill(), CommitSkillOptions, discardStaged(), generateSpawnId(), stageSkill(), StageSkillOptions, validateSkillName() (+1 more)

### Community 83 - "gstack-gbrain-install"
Cohesion: 0.17
Nodes (11): 1. Auto-enable cursor-interactive scan with `-i` flag, 2. Add popover/portal priority scanning, 3. Remove the `hasRole` skip in cursor-interactive scan, 4. Add dropdown test fixture and tests, Changes, Files Changed, Plan: Snapshot Dropdown/Autocomplete Interactive Element Detection, Problem (+3 more)

### Community 84 - "gstack-question-preference"
Cohesion: 0.27
Nodes (9): CDP_ALLOWLIST, CDP_ALLOWLIST_INDEX, CdpOutput, CdpScope, isCdpMethodAllowed(), lookupCdpMethod(), NOTE: Tracing.start can capture cross-tab data depending on categories., NOTE: Page.navigate is INTENTIONALLY NOT on the allowlist (Codex T2 cat 4). (+1 more)

### Community 85 - "security-bench.test.ts"
Cohesion: 0.29
Nodes (7): BenchRow, CACHE_DIR, CACHE_FILE, fetchDatasetSample(), loadOrFetchRows(), ML_AVAILABLE, MODEL_CACHE

### Community 86 - "Ponytail Help"
Cohesion: 0.25
Nodes (6): build_tile(), BuildTileRequest, defeat_invader(), InvaderDefeatRequest, FastAPI REST API for Dungeon Lord Hybrid Management & Grid-Crawler Engine., DungeonTile

### Community 87 - "SKILL.md"
Cohesion: 0.22
Nodes (7): calculate_invader_essence_reward(), Core game rules and compounding leveling formula for Dungeon Lord., Calculates Essence yield upon defeating an invader party., Turn-based Combat Manager enforcing turn structure and logging events., InvaderParty, test_dungeon_lord_garrison_and_combat(), test_invader_essence_reward()

### Community 88 - "SKILL.md"
Cohesion: 0.20
Nodes (9): AGENTS.md Rules (Auto-added by `graphify hermes install`), Building the Knowledge Graph, graphify Setup for Hermes Projects, Installation, Maintenance, Nebula Writer 2 Session (2026-08-03), Output Structure (`graphify-out/`), Overview (+1 more)

### Community 89 - "SKILL.md"
Cohesion: 0.20
Nodes (9): Common Mistake (Anti-Pattern), Context, gstack Skill Integration, gstack Validation Sprint Pattern, Kill Criteria (any = stop), Pattern, Sprint Artifacts, Sprint Structure (30 days) (+1 more)

### Community 90 - "SKILL.md"
Cohesion: 0.28
Nodes (4): RefEntry, SetContentWaitUntil, makeRefs(), mockRefEntry()

### Community 91 - "gstack-codex-probe"
Cohesion: 0.22
Nodes (8): Boundaries, Intensity, Output, Persistence, Ponytail, Rules, The ladder, When NOT to be lazy

### Community 92 - "gstack-config"
Cohesion: 0.22
Nodes (8): Branch Numbering Mode, Create Feature Branch, Environment Variable Override, Execution, Graceful Degradation, Output, Prerequisites, User Input

### Community 94 - "find-browse.ts"
Cohesion: 0.52
Nodes (5): findExecutable(), getGitRoot(), isExecutable(), locateBinary(), main()

### Community 95 - "pair-agent-e2e.test.ts"
Cohesion: 0.33
Nodes (5): DaemonHandle, ROOT, SERVER_ENTRY, spawnDaemon(), waitForReady()

### Community 96 - "gstack-artifacts-url"
Cohesion: 0.25
Nodes (4): AINarrator, AI Narrator Bridge.  Adheres strictly to the prompt contract in SPEC.md: The AI, Constructs strict AI DM prompt contract., Generates immersive narration for a combat log entry.          Provides a clean

### Community 99 - "server-sanitize-surrogates.test.ts"
Cohesion: 0.33
Nodes (5): fnMatch, jsSrc, sanitizeLoneSurrogates, SERVER_PATH, SERVER_SRC

### Community 101 - "Auto-Commit Changes"
Cohesion: 0.25
Nodes (7): Configure Default Mode, Deactivate, Levels, More, Ponytail Help, Skills, Update

### Community 102 - "Initialize Git Repository"
Cohesion: 0.25
Nodes (7): Anti-Examples: What NOT To Do, Checklist Purpose: "Unit Tests for English", Example Checklist Types & Sample Items, Execution Steps, Post-Execution Checks, Pre-Execution Checks, User Input

### Community 103 - "Detect Git Remote URL"
Cohesion: 0.25
Nodes (7): Key rules, Outline, Phase 0: Outline & Research, Phase 1: Design & Contracts, Phases, Pre-Execution Checks, User Input

### Community 104 - "Validate Feature Branch"
Cohesion: 0.25
Nodes (7): For AI Generation, Outline, Pre-Execution Checks, Quick Guidelines, Section Requirements, Success Criteria Guidelines, User Input

### Community 105 - "from-file-path-validation.test.ts"
Cohesion: 0.40
Nodes (3): META_SRC, ROOT, WRITE_SRC

### Community 106 - "sidebar-tabs.test.ts"
Cohesion: 0.40
Nodes (4): HTML, JS, MANIFEST, TERM_JS

### Community 108 - "serve.ts"
Cohesion: 0.19
Nodes (16): RFC-3986, SAFE_DIRECTORIES, TEMP_ONLY, validateOutputPath(), validateReadPath(), validateTempPath(), isPathWithin(), BLOCKED_IPV6_PREFIXES (+8 more)

### Community 109 - "ponytail-audit.md"
Cohesion: 0.25
Nodes (7): Checklist Format (REQUIRED), Outline, Phase Structure, Pre-Execution Checks, Task Generation Rules, Task Organization, User Input

### Community 110 - "Ponytail Gain"
Cohesion: 0.43
Nodes (4): guardScreenshotBuffer(), guardScreenshotPath(), SCREENSHOT_MAX_DIMENSION_PX, SizeGuardResult

### Community 111 - "ponytail-review.md"
Cohesion: 0.43
Nodes (6): clearDecision(), decisionFileForTab(), excerptForReview(), readDecision(), Verdict, writeDecision()

### Community 112 - "SKILL.md"
Cohesion: 0.33
Nodes (5): net8.0, Microsoft.NET.Test.Sdk (17.9.0), NUnit (3.14.0), NUnit3TestAdapter (4.5.0), Microsoft.NET.Sdk

### Community 113 - "SKILL.md"
Cohesion: 0.33
Nodes (5): Auto-Commit Changes, Behavior, Configuration, Execution, Graceful Degradation

### Community 114 - "SKILL.md"
Cohesion: 0.33
Nodes (5): Customization, Execution, Graceful Degradation, Initialize Git Repository, Output

### Community 115 - "gstack-first-task-detect"
Cohesion: 0.33
Nodes (5): Detect Git Remote URL, Execution, Graceful Degradation, Output, Prerequisites

### Community 116 - "gstack-gbrain-lib.sh"
Cohesion: 0.33
Nodes (5): Execution, Graceful Degradation, Prerequisites, Validate Feature Branch, Validation Rules

### Community 118 - "gstack-gbrain-supabase-verify"
Cohesion: 0.40
Nodes (4): Boundaries, Hunt, Output, Tags

### Community 119 - "gstack-relink"
Cohesion: 0.40
Nodes (4): Boundaries, Honesty boundary, Ponytail Gain, Scoreboard

### Community 120 - "CrawlHUD"
Cohesion: 0.08
Nodes (17): Button, CrawlController, Direction, DungeonGrid, int, InvaderAI, InvaderParty, Label (+9 more)

### Community 123 - "learnings-injection.test.ts"
Cohesion: 0.50
Nodes (3): BIN_DIR, SCRIPT, SCRIPT_PATH

### Community 124 - "security-classifier-download-cleanup.test.ts"
Cohesion: 0.22
Nodes (8): Character, CombatState, CombatManager, Starts combat, rolls initiative for all participants, orders turn queue., Executes an attack action for the entity whose turn it currently is., Advances combat turn pointer to next participant., InitiativeEntry, Monster

### Community 132 - "prototype.ts"
Cohesion: 0.67
Nodes (3): briefs, generateMockup(), main()

### Community 133 - "ponytail-debt.md"
Cohesion: 0.40
Nodes (4): Boundaries, Examples, Format, Scoring

### Community 134 - "SKILL.md"
Cohesion: 0.40
Nodes (4): Outline, Post-Execution Checks, Pre-Execution Checks, User Input

### Community 135 - "gstack-learnings-search"
Cohesion: 0.40
Nodes (4): Outline, Post-Execution Checks, Pre-Execution Checks, User Input

### Community 136 - "gstack-repo-mode"
Cohesion: 0.40
Nodes (4): Outline, Post-Execution Checks, Pre-Execution Checks, User Input

### Community 138 - "gstack-settings-hook"
Cohesion: 0.50
Nodes (3): Boundaries, Output, Scan

### Community 139 - "gstack-telemetry-log"
Cohesion: 0.50
Nodes (3): Outline, Pre-Execution Checks, User Input

### Community 206 - "calculate_attribute"
Cohesion: 0.27
Nodes (8): calculate_attribute(), Compounding percentage-based growth formula for Dungeon Lord and Dungeon Rank:, DungeonLord, DungeonRank, test_dungeon_lord_hp_scaling(), test_level_10_attribute(), test_level_1_attribute(), test_level_25_attribute()

### Community 207 - "meta-commands.ts"
Cohesion: 0.12
Nodes (28): ALL_COMMANDS, allCmds, buildUnknownCommandError(), canonicalizeCommand(), COMMAND_ALIASES, COMMAND_DESCRIPTIONS, descKeys, DOM_CONTENT_COMMANDS (+20 more)

### Community 208 - "cdp-bridge.ts"
Cohesion: 0.20
Nodes (11): CdpAllowEntry, CdpDispatchResult, dispatchCdpCall(), getCdpSession(), getOrCreateCdpSession(), sessionCache, withCdpSession(), handleCdpCommand() (+3 more)

### Community 209 - "pty-session-lease.ts"
Cohesion: 0.13
Nodes (11): CharacterBody3D, Node3D, bool, Camera3D, Direction, string, Vector3, Vector3I (+3 more)

### Community 210 - "main"
Cohesion: 0.18
Nodes (5): __resetShuttingDown(), AGENT_RECORD_FILE, makeMinimalConfig(), PORT_FILE, TOKEN_FILE

### Community 213 - "media-extract.ts"
Cohesion: 0.25
Nodes (6): AudioInfo, BackgroundImageInfo, ImageInfo, MediaResult, VideoInfo, VideoSource

### Community 214 - "security-review-flow.test.ts"
Cohesion: 0.18
Nodes (4): handleChromiumDisconnect(), isCustomChromium(), resolveDisconnectCause(), shouldEnableChromiumSandbox()

### Community 221 - "sanitize.ts"
Cohesion: 0.14
Nodes (17): formatInspectorResult(), getModificationHistory(), assertJsOriginAllowed(), getCleanText(), handleReadCommand(), hasAwait(), needsBlockWrapper(), OutArgs (+9 more)

### Community 226 - "requireApiKey"
Cohesion: 0.25
Nodes (11): checkCommand(), checkMockup(), CheckResult, callImageGeneration(), generate(), GenerateOptions, GenerateResult, createSession() (+3 more)

### Community 227 - "memory.ts"
Cohesion: 0.22
Nodes (12): requireApiKey(), DesignToCodeResult, generateDesignToCodePrompt(), analyzeScreenshot(), evolve(), EvolveOptions, defaultDesign(), extractDesignLanguage() (+4 more)

## Knowledge Gaps
- **645 isolated node(s):** `build-node-server.sh script`, `ActivityEntry`, `activityBuffer`, `ActivitySubscriber`, `subscribers` (+640 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **50 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `BrowserManager` connect `BrowserManager` to `cookie-import-browser.ts`, `server.ts`, `domain-skill-commands.ts`, `cli.ts`, `TabSession`, `socks-bridge.ts`, `browser-manager.ts`, `activity.ts`, `compare-board.test.ts`, `url-validation.ts`, `auth.ts`, `meta-commands.ts`, `cdp-bridge.ts`, `main`, `security-review-flow.test.ts`, `.getActiveSession`, `cdp-mutex.test.ts`, `.getPage`, `sanitize.ts`, `file-drop.test.ts`, `serve.ts`?**
  _High betweenness centrality (0.053) - this node is a cross-community bridge._
- **Why does `req()` connect `Functional Requirements` to `security-classifier.ts`, `socks-bridge.ts`, `sse-session-cookie.ts`?**
  _High betweenness centrality (0.023) - this node is a cross-community bridge._
- **Why does `buildFetchHandler()` connect `sse-session-cookie.ts` to `Functional Requirements`, `cookie-import-browser.ts`, `token-registry.ts`, `server.ts`, `buildFetchHandler`, `mkdirSecure`, `cdp-inspector.ts`, `browser-manager.ts`, `activity.ts`, `gstack-gbrain-supabase-provision`, `security-sidecar-client.ts`, `config.ts`, `pty-session-cookie.ts`, `xvfb.ts`, `auth.ts`, `.handoff`, `terminal-agent-control.ts`, `gstack-gbrain-repo-policy`, `Ponytail`, `main`, `sanitize.ts`, `serve.ts`, `gstack-gbrain-mcp-verify`?**
  _High betweenness centrality (0.023) - this node is a cross-community bridge._
- **Are the 5 inferred relationships involving `buildFetchHandler()` (e.g. with `subscribe()` and `.totalAdded()`) actually correct?**
  _`buildFetchHandler()` has 5 INFERRED edges - model-reasoned connections that need verification._
- **What connects `build-node-server.sh script`, `ActivityEntry`, `activityBuffer` to the rest of the system?**
  _645 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SPEC: Dungeon Master — AI-Powered TTRPG Assistant` be split into smaller, more focused modules?**
  _Cohesion score 0.05704365079365079 - nodes in this community are weakly interconnected._
- **Should `main.py` be split into smaller, more focused modules?**
  _Cohesion score 0.04081632653061224 - nodes in this community are weakly interconnected._