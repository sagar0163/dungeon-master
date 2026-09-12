# WAR_ROOM_PLAN_7.md — Issue #7: Session save/load to SQLite

## Remaining work (prior commit `9cc461b` has basics; gaps below)

- [x] Validate schema version on `load_game()` — reject incompatible saves, raise clear error
- [x] Add startup auto-load: `main.py` loads saved state from DB if it exists (resume)
- [x] Add `GET /game/save_status` — report whether a save exists (for UI "Resume" button)
- [x] Deterministic assertion test: compare full `model_dump()` dict equality after save/load
- [x] Test: schema version mismatch raises error
- [x] Test: all Lord/Rank fields round-trip correctly (hp_current, attack_base, defense_base, z, etc.)
- [x] Test: multiple grid tiles + multiple monsters round-trip
- [x] Test: wave_count and settlement_reputation survive round-trip
- [x] Test: startup auto-load restores previous state
- [x] Test: save_status endpoint reports correctly
- [x] Test: autosave on mode_switch and quit endpoints persist state
- [x] Run full test suite, fix any failures
- [x] Commit incrementally as each subtask completes