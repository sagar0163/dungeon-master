using System;
using System.Collections.Generic;
using Godot;

namespace DungeonLord.Scripts
{
    /// <summary>
    /// Invader party AI with A* pathfinding on the dungeon grid.
    /// Party strength scales with Settlement Reputation + Dungeon Rank.
    /// </summary>
    [GlobalClass]
    public partial class InvaderAI : Node
    {
        // Configuration
        [Export] public float PathRecalcInterval { get; set; } = 2.0f;
        [Export] public int MaxPartySize { get; set; } = 6;
        [Export] public float BaseSpawnInterval { get; set; } = 60.0f; // seconds
        [Export] public int WaveSeed { get; set; } = 1337;
        
        // References
        [Export] public DungeonGrid DungeonGrid { get; private set; }
        [Export] public EssenceManager EssenceManager { get; private set; }
        
        // State
        private readonly List<InvaderParty> _activeParties = new();
        private float _spawnTimer = 0f;
        private int _nextPartyId = 1;
        private RandomNumberGenerator _rng = new();
        
        // Settlement reputation (0.0 to 10.0+)
        public float SettlementReputation { get; private set; } = 1.0f;
        
        public event Action<InvaderParty> OnPartySpawned;
        public event Action<InvaderParty> OnPartyDestroyed;
        public event Action<InvaderParty, Vector3I> OnPartyReachedTarget;
        
        public override void _Ready()
        {
            _rng.Seed = (ulong)WaveSeed;
            GD.Print("InvaderAI initialized");
        }
        
        public override void _Process(double delta)
        {
            float dt = (float)delta;
            
            UpdateSpawnTimer(dt);
            UpdateParties(dt);
        }
        
        private void UpdateSpawnTimer(float dt)
        {
            // Spawn interval scales inversely with reputation
            float spawnInterval = BaseSpawnInterval / Math.Max(1.0f, SettlementReputation);
            _spawnTimer += dt;
            
            if (_spawnTimer >= spawnInterval && _activeParties.Count < MaxPartySize)
            {
                SpawnParty();
                _spawnTimer = 0f;
            }
        }
        
        private void UpdateParties(float dt)
        {
            for (int i = _activeParties.Count - 1; i >= 0; i--)
            {
                var party = _activeParties[i];
                party.Update(dt, DungeonGrid);
                
                if (party.State == PartyState.Destroyed)
                {
                    OnPartyDestroyed?.Invoke(party);
                    _activeParties.RemoveAt(i);
                }
                else if (party.State == PartyState.ReachedTarget)
                {
                    OnPartyReachedTarget?.Invoke(party, party.TargetPosition);
                    _activeParties.RemoveAt(i);
                }
            }
        }
        
        private void SpawnParty()
        {
            if (DungeonGrid == null) return;
            
            // Find entrance on floor 0
            Vector3I entrance = FindEntrance();
            if (entrance.X < 0) return;
            
            // Find dungeon core (Lord Chamber)
            Vector3I core = FindCore();
            if (core.X < 0) core = entrance; // fallback
            
            // Generate party scaled to reputation + dungeon rank
            int dungeonRank = EssenceManager?.DungeonRank ?? 1;
            float difficulty = SettlementReputation * 0.5f + dungeonRank * 0.3f;
            
            var party = new InvaderParty
            {
                Id = _nextPartyId++,
                Members = GeneratePartyMembers(difficulty),
                StartPosition = entrance,
                TargetPosition = core,
                State = PartyState.Pathfinding,
                ReputationModifier = SettlementReputation,
                DungeonRank = dungeonRank
            };
            
            // Initial pathfind
            party.Path = FindPath(entrance, core);
            party.State = party.Path.Count > 0 ? PartyState.Advancing : PartyState.Destroyed;
            
            _activeParties.Add(party);
            OnPartySpawned?.Invoke(party);
            
            GD.Print($"Spawned invader party {party.Id} with {party.Members.Count} members (difficulty: {difficulty:F1})");
        }
        
        private Vector3I FindEntrance()
        {
            for (int x = 0; x < DungeonGrid.Width; x++)
            {
                for (int y = 0; y < DungeonGrid.Height; y++)
                {
                    var tile = DungeonGrid.GetTile(x, y, 0);
                    if (tile?.Type == TileType.SpawnPoint)
                        return new Vector3I(x, y, 0);
                }
            }
            return new Vector3I(-1, -1, -1);
        }
        
        private Vector3I FindCore()
        {
            for (int z = DungeonGrid.Floors - 1; z >= 0; z--)
            {
                for (int x = 0; x < DungeonGrid.Width; x++)
                {
                    for (int y = 0; y < DungeonGrid.Height; y++)
                    {
                        var tile = DungeonGrid.GetTile(x, y, z);
                        if (tile?.Type == TileType.LordChamber)
                            return new Vector3I(x, y, z);
                    }
                }
            }
            return new Vector3I(-1, -1, -1);
        }
        
        private List<InvaderMember> GeneratePartyMembers(float difficulty)
        {
            var members = new List<InvaderMember>();
            int count = Math.Min(MaxPartySize, Math.Max(1, (int)(difficulty * 1.5f)));
            
            string[] classes = { "fighter", "wizard", "cleric", "rogue", "ranger", "paladin" };
            string[] roles = { "tank", "dps", "healer", "support" };
            
            for (int i = 0; i < count; i++)
            {
                int level = Math.Max(1, (int)(difficulty + _rng.RandiRange(-1, 2)));
                string clazz = classes[_rng.RandiRange(0, classes.Length - 1)];
                string role = roles[_rng.RandiRange(0, roles.Length - 1)];
                
                members.Add(new InvaderMember
                {
                    Class = clazz,
                    Level = level,
                    Role = role,
                    CurrentHP = 100 + level * 10,
                    MaxHP = 100 + level * 10
                });
            }
            
            return members;
        }
        
        // A* pathfinding on dungeon grid (shared, Godot-free engine in Pathfinding.cs)
        public List<Vector3I> FindPath(Vector3I start, Vector3I goal)
        {
            if (DungeonGrid == null) return new List<Vector3I>();

            var found = Pathfinding.FindPath(DungeonGrid,
                new Pathfinding.Cell(start.X, start.Y, start.Z),
                new Pathfinding.Cell(goal.X, goal.Y, goal.Z));

            var result = new List<Vector3I>(found.Count);
            foreach (var cell in found)
                result.Add(new Vector3I(cell.X, cell.Y, cell.Z));
            return result;
        }
        
        public void ModifyReputation(float delta)
        {
            SettlementReputation = Math.Max(0.1f, SettlementReputation + delta);
            GD.Print($"Settlement Reputation changed to: {SettlementReputation:F1}");
        }

        /// <summary>
        /// Spawns one invader wave deterministically from the given seed (visible in Crawl Mode).
        /// </summary>
        public void SpawnWave(int seed)
        {
            _rng.Seed = (ulong)seed;
            SpawnParty();
        }

        public IReadOnlyList<InvaderParty> ActiveParties => _activeParties.AsReadOnly();
    }
    
    // Data classes
    public class InvaderParty
    {
        public int Id { get; set; }
        public List<InvaderMember> Members { get; set; } = new();
        public Vector3I StartPosition { get; set; }
        public Vector3I TargetPosition { get; set; }
        public List<Vector3I> Path { get; set; } = new();
        public int CurrentPathIndex { get; set; } = 0;
        public PartyState State { get; set; } = PartyState.Idle;
        public float ReputationModifier { get; set; } = 1.0f;
        public int DungeonRank { get; set; } = 1;
        public float StuckTimer { get; set; } = 0f;
        
        public void Update(float dt, DungeonGrid grid)
        {
            if (State != PartyState.Advancing || Path.Count == 0) return;
            
            if (CurrentPathIndex >= Path.Count)
            {
                State = PartyState.ReachedTarget;
                return;
            }
            
            var targetTile = Path[CurrentPathIndex];
            var tile = grid.GetTile(targetTile.X, targetTile.Y, targetTile.Z);
            
            // Check for trap
            if (tile?.Type == TileType.Trap)
            {
                TriggerTrap(tile);
            }
            
            // Check for monsters
            if (tile?.GarrisonedMonsters.Count > 0)
            {
                EngageMonsters(tile);
                return; // Combat resolves over time
            }
            
            // Move to next tile (simplified - instant for now)
            CurrentPathIndex++;
            StuckTimer = 0f;
        }
        
        private void TriggerTrap(DungeonTile tile)
        {
            // Apply trap damage to party
            foreach (var member in Members)
            {
                if (member.CurrentHP > 0)
                {
                    member.CurrentHP -= 20; // Base trap damage
                    if (member.CurrentHP <= 0)
                        member.CurrentHP = 0;
                }
            }
            
            // Check if party wiped
            if (Members.TrueForAll(m => m.CurrentHP <= 0))
                State = PartyState.Destroyed;
        }
        
        private void EngageMonsters(DungeonTile tile)
        {
            State = PartyState.Combat;
            // Combat logic would go here - simplified
            // For now, just damage both sides
            foreach (var member in Members)
            {
                if (member.CurrentHP > 0)
                    member.CurrentHP -= 10;
            }
            
            if (Members.TrueForAll(m => m.CurrentHP <= 0))
                State = PartyState.Destroyed;
            else
                State = PartyState.Advancing; // Continue after combat
        }
    }
    
    public class InvaderMember
    {
        public string Class { get; set; }
        public int Level { get; set; }
        public string Role { get; set; }
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
    }
    
    public enum PartyState
    {
        Idle,
        Pathfinding,
        Advancing,
        Combat,
        ReachedTarget,
        Destroyed
    }
}