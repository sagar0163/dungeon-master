using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonLord.Scripts
{
    public enum WaveEventType
    {
        PartySpawned,
        MovedTo,
        TrapTriggered,
        CombatRound,
        PartyMemberKilled,
        PartyWiped,
        ReachedCore,
        NoPath
    }

    public enum WaveOutcome
    {
        NoPath,
        Wiped,
        ReachedCore
    }

    public sealed class WaveEvent
    {
        public WaveEventType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Round { get; set; }
        public int AliveMembers { get; set; }
        public long EssenceTotal { get; set; }
        public string Detail { get; set; } = "";

        public override string ToString()
            => $"{Type}|{X},{Y},{Z}|{Round}|{AliveMembers}|{EssenceTotal}|{Detail}";
    }

    /// <summary>
    /// Godot-free deterministic invader wave: spawns a party at the dungeon entrance,
    /// A*-pathfinds to the core, steps tile-by-tile triggering traps, resolving combat
    /// via CombatRules/DiceEngine, and crediting Essence for each kill. Same seed + same
    /// grid -> identical transcript (SPEC NFR-4, FR-6.3, FR-3.1).
    /// </summary>
    public sealed class InvaderWaveSimulator
    {
        private readonly DungeonGrid _grid;
        private readonly EssenceManager _essence;
        private readonly DiceEngine _dice;
        private readonly HashSet<string> _triggeredTraps = new();

        public List<Combatant> Party { get; } = new();
        public List<WaveEvent> Log { get; } = new();
        public List<Pathfinding.Cell> Path { get; private set; } = new();
        public WaveOutcome Outcome { get; private set; }
        public int SettlementReputation { get; set; } = 10;
        public int MaxCombatRounds { get; set; } = 20;
        public int TrapDamageDice { get; set; } = 2;

        public InvaderWaveSimulator(DungeonGrid grid, EssenceManager essence, int seed)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _essence = essence ?? throw new ArgumentNullException(nameof(essence));
            _dice = new DiceEngine(seed);
        }

        public void SetParty(IEnumerable<Combatant> members)
        {
            Party.Clear();
            if (members != null)
                Party.AddRange(members);
        }

        public WaveOutcome Run()
        {
            Log.Clear();
            Log.Add(new WaveEvent { Type = WaveEventType.PartySpawned, AliveMembers = Party.Count });

            Pathfinding.Cell? entrance = FindEntrance();
            Pathfinding.Cell? core = FindCore();
            if (entrance == null || core == null)
            {
                Outcome = WaveOutcome.NoPath;
                return Outcome;
            }

            if (Party.Count == 0)
                SpawnDefaultParty();

            Path = Pathfinding.FindPath(_grid, entrance.Value, core.Value);
            if (Path.Count == 0)
            {
                Log.Add(new WaveEvent { Type = WaveEventType.NoPath, X = entrance.Value.X, Y = entrance.Value.Y, Z = entrance.Value.Z, AliveMembers = AliveCount(), EssenceTotal = _essence.CurrentEssence });
                Outcome = WaveOutcome.NoPath;
                return Outcome;
            }

            Log.Add(new WaveEvent { Type = WaveEventType.MovedTo, X = entrance.Value.X, Y = entrance.Value.Y, Z = entrance.Value.Z, AliveMembers = AliveCount(), EssenceTotal = _essence.CurrentEssence, Detail = "entrance" });

            for (int i = 1; i < Path.Count; i++)
            {
                var cell = Path[i];
                var tile = _grid.GetTile(cell.X, cell.Y, cell.Z);
                Log.Add(new WaveEvent { Type = WaveEventType.MovedTo, X = cell.X, Y = cell.Y, Z = cell.Z, AliveMembers = AliveCount(), EssenceTotal = _essence.CurrentEssence });

                if (AliveCount() > 0 && tile?.Type == TileType.Trap && _triggeredTraps.Add(Key(cell)))
                    TriggerTrap(cell, tile);

                if (AliveCount() > 0 && tile != null && tile.GarrisonedMonsters.Count > 0)
                    FightGarrison(cell, tile);

                if (AliveCount() == 0)
                {
                    Log.Add(new WaveEvent { Type = WaveEventType.PartyWiped, X = cell.X, Y = cell.Y, Z = cell.Z, AliveMembers = 0, EssenceTotal = _essence.CurrentEssence });
                    Outcome = WaveOutcome.Wiped;
                    return Outcome;
                }
            }

            var last = Path[Path.Count - 1];
            Log.Add(new WaveEvent { Type = WaveEventType.ReachedCore, X = last.X, Y = last.Y, Z = last.Z, AliveMembers = AliveCount(), EssenceTotal = _essence.CurrentEssence });
            Outcome = WaveOutcome.ReachedCore;
            return Outcome;
        }

        private void TriggerTrap(Pathfinding.Cell at, DungeonTile tile)
        {
            int damage = _dice.Roll($"{TrapDamageDice}d6").Total;
            foreach (var member in Party)
                if (member.IsAlive)
                    member.ApplyDamage(damage);

            Log.Add(new WaveEvent { Type = WaveEventType.TrapTriggered, X = at.X, Y = at.Y, Z = at.Z, AliveMembers = AliveCount(), EssenceTotal = _essence.CurrentEssence, Detail = $"{tile.TrapId ?? "trap"}|{damage}" });
        }

        private void FightGarrison(Pathfinding.Cell at, DungeonTile tile)
        {
            var defenders = new List<Combatant>();
            foreach (var id in tile.GarrisonedMonsters)
                defenders.Add(MonsterFromId(id));

            int round = 1;
            while (AliveCount() > 0 && defenders.Any(d => d.IsAlive) && round <= MaxCombatRounds)
            {
                foreach (var defender in defenders)
                {
                    if (!defender.IsAlive) continue;
                    var target = Party.FirstOrDefault(m => m.IsAlive);
                    if (target == null) break;

                    var result = CombatRules.ResolveAttack(defender, target, _dice);
                    if (result.Hit)
                        target.ApplyDamage(result.Damage);
                    if (!target.IsAlive)
                        CreditKill(target, at, round);

                    Log.Add(new WaveEvent { Type = WaveEventType.CombatRound, X = at.X, Y = at.Y, Z = at.Z, Round = round, AliveMembers = AliveCount(), EssenceTotal = _essence.CurrentEssence, Detail = $"{defender.Name}|{target.Name}|{result.AttackTotal}|{result.Damage}|hit={result.Hit}|crit={result.IsCrit}" });
                    if (AliveCount() == 0) break;
                }

                foreach (var member in Party)
                {
                    if (!member.IsAlive) continue;
                    var target = defenders.FirstOrDefault(d => d.IsAlive);
                    if (target == null) break;

                    var result = CombatRules.ResolveAttack(member, target, _dice);
                    if (result.Hit)
                        target.ApplyDamage(result.Damage);

                    Log.Add(new WaveEvent { Type = WaveEventType.CombatRound, X = at.X, Y = at.Y, Z = at.Z, Round = round, AliveMembers = AliveCount(), EssenceTotal = _essence.CurrentEssence, Detail = $"{member.Name}|{target.Name}|{result.AttackTotal}|{result.Damage}|hit={result.Hit}|crit={result.IsCrit}" });
                }

                round++;
            }
        }

        private void CreditKill(Combatant member, Pathfinding.Cell at, int round)
        {
            long reward = CombatRules.CalculateInvaderEssenceReward(member.Level, 1, SettlementReputation);
            _essence.AddEssence(reward);
            Log.Add(new WaveEvent { Type = WaveEventType.PartyMemberKilled, X = at.X, Y = at.Y, Z = at.Z, Round = round, AliveMembers = AliveCount(), EssenceTotal = _essence.CurrentEssence, Detail = $"{member.Name}|{reward}" });
        }

        private static Combatant MonsterFromId(string id)
        {
            string family = id ?? "";
            int level = 1;
            int idx = family.LastIndexOf('_');
            if (idx >= 0 && int.TryParse(family.Substring(idx + 1), out int parsed))
            {
                family = family.Substring(0, idx);
                level = parsed;
            }

            int hp, ac, atk;
            string dmg;
            switch (family)
            {
                case "goblin": hp = 8 + 3 * level; ac = 13; atk = 1 + level; dmg = "1d4+2"; break;
                case "rat": hp = 6 + 2 * level; ac = 11; atk = level; dmg = "1d3"; break;
                case "skeleton": hp = 10 + 4 * level; ac = 14; atk = 1 + level; dmg = "1d6"; break;
                case "wolf": hp = 8 + 3 * level; ac = 13; atk = 1 + level; dmg = "1d6+1"; break;
                default: hp = 10 + 4 * level; ac = 12; atk = level; dmg = "1d6"; break;
            }

            return new Combatant
            {
                Id = id,
                Name = family,
                Level = level,
                CurrentHp = hp,
                MaxHp = hp,
                ArmorClass = ac,
                AttackBonus = atk,
                DamageDice = dmg
            };
        }

        private void SpawnDefaultParty()
        {
            AddCombatant("fighter", 3);
            AddCombatant("wizard", 2);
            AddCombatant("cleric", 2);
        }

        private void AddCombatant(string name, int level)
        {
            int hp, ac, atk;
            string dmg;
            switch (name)
            {
                case "fighter": hp = 12 + 6 * level; ac = 16; atk = 2 + level; dmg = "1d8+2"; break;
                case "wizard": hp = 8 + 4 * level; ac = 11; atk = 1 + level; dmg = "1d6"; break;
                case "cleric": hp = 10 + 5 * level; ac = 14; atk = 1 + level; dmg = "1d6+1"; break;
                default: hp = 10 + 4 * level; ac = 12; atk = level; dmg = "1d6"; break;
            }

            Party.Add(new Combatant
            {
                Id = $"{name}_{level}",
                Name = name,
                Level = level,
                CurrentHp = hp,
                MaxHp = hp,
                ArmorClass = ac,
                AttackBonus = atk,
                DamageDice = dmg
            });
        }

        private Pathfinding.Cell? FindEntrance()
        {
            for (int x = 0; x < _grid.Width; x++)
                for (int y = 0; y < _grid.Height; y++)
                    if (_grid.GetTile(x, y, 0)?.Type == TileType.SpawnPoint)
                        return new Pathfinding.Cell(x, y, 0);
            return null;
        }

        private Pathfinding.Cell? FindCore()
        {
            for (int z = _grid.Floors - 1; z >= 0; z--)
                for (int x = 0; x < _grid.Width; x++)
                    for (int y = 0; y < _grid.Height; y++)
                        if (_grid.GetTile(x, y, z)?.Type == TileType.LordChamber)
                            return new Pathfinding.Cell(x, y, z);
            return null;
        }

        private int AliveCount() => Party.Count(m => m.IsAlive);
        private static string Key(Pathfinding.Cell c) => $"{c.X},{c.Y},{c.Z}";
    }
}