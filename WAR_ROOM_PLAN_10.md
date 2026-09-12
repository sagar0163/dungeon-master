# WAR ROOM PLAN — Issue #10: Deterministic invader-wave integration

Goal: prove the `pathfind -> trap -> combat -> Essence` loop headlessly (deterministic test) and make it visible in-game in Crawl Mode.

Approach mirrors issue #9: extract Godot-free core modules into `Scripts/`, link them into `DungeonLord.Tests/`, and assert the full deterministic run in NUnit. The existing Python `dice.py`/`combat.py` reference is the semantic source of truth (same rules: d20 attack vs AC, damage dice, essence reward).

## Checklist

- [x] Add Godot-free `Scripts/DiceEngine.cs` (seeded, deterministic; mirrors `dungeon_master/dice.py` standard/adv/dis/kh notation)
- [x] Add Godot-free `Scripts/CombatRules.cs` (Combatant, ResolveAttack, CalculateInvaderEssenceReward; mirrors Python dice/combat/rules behavior)
- [x] Add Godot-free `Scripts/InvaderWaveSimulation.cs` (spawn at entrance -> A* -> step path -> trap -> combat -> kill credits Essence -> reach core; deterministic transcript log)
- [x] Add test project links for the three new core files `DungeonLord.Tests.csproj`
- [x] Add `DungeonLord.Tests/InvaderWaveTests.cs` covering all acceptance criteria (path to core, trap, combat, Essence credit, determinism)
- [x] In-game wiring: `InvaderAI.SpawnWave(seed)` + deterministic RNG seed; `GameManager` builds a starter dungeon + spawns first visible wave
- [x] Unify GameManager Essence reward to shared `CalculateInvaderEssenceReward`
- [ ] Run dotnet test + python pytest; fix failures
- [ ] graphify update . ; cleanup plan file; final commit + push