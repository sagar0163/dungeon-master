using System;
using System.Collections.Generic;
using Godot;

namespace DungeonLord.Scripts
{
    /// <summary>
    /// Manages dungeon reset cycles: trap re-arming, monster garrison respawning, loot repopulation.
    /// Configurable: timer-based, wave-based, or manual trigger from Builder Mode.
    /// </summary>
    [GlobalClass]
    public partial class DungeonResetCycle : Node
    {
        // Configuration
        [Export] public float ResetCooldown { get; set; } = 300.0f; // 5 minutes default
        [Export] public int WavesPerReset { get; set; } = 5;
        [Export] public bool AutoResetEnabled { get; set; } = true;
        [Export] public bool ResetTraps { get; set; } = true;
        [Export] public bool ResetGarrisons { get; set; } = true;
        [Export] public bool ResetLoot { get; set; } = true;
        [Export] public float GarrisonRespawnDelay { get; set; } = 30.0f; // seconds after reset
        
        // References
        [Export] public DungeonGrid DungeonGrid { get; private set; }
        [Export] public EssenceManager EssenceManager { get; private set; }
        [Export] public InvaderAI InvaderAI { get; private set; }
        
        // State
        private float _resetTimer = 0f;
        private int _wavesSinceReset = 0;
        private bool _isResetting = false;
        private readonly Dictionary<string, TrapState> _trapStates = new();
        private readonly Dictionary<Vector3I, GarrisonState> _garrisonStates = new();
        private readonly Dictionary<Vector3I, LootState> _lootStates = new();
        
        public event Action OnResetStarted;
        public event Action OnResetCompleted;
        public event Action<float> OnResetTimerChanged;
        public event Action<int> OnWaveCompleted;
        
        public float TimeUntilReset => Math.Max(0f, ResetCooldown - _resetTimer);
        public int WavesUntilReset => Math.Max(0, WavesPerReset - _wavesSinceReset);
        public bool IsResetting => _isResetting;
        
        public override void _Ready()
        {
            GD.Print("DungeonResetCycle initialized");
            CaptureInitialState();
        }
        
        public override void _Process(double delta)
        {
            float dt = (float)delta;
            
            if (AutoResetEnabled && !_isResetting)
            {
                _resetTimer += dt;
                OnResetTimerChanged?.Invoke(TimeUntilReset);
                
                if (_resetTimer >= ResetCooldown)
                {
                    TriggerReset();
                }
            }
        }
        
        /// <summary>
        /// Capture initial state of all traps, garrisons, and loot for reset reference.
        /// </summary>
        public void CaptureInitialState()
        {
            if (DungeonGrid == null) return;
            
            _trapStates.Clear();
            _garrisonStates.Clear();
            _lootStates.Clear();
            
            for (int z = 0; z < DungeonGrid.Floors; z++)
            {
                for (int x = 0; x < DungeonGrid.Width; x++)
                {
                    for (int y = 0; y < DungeonGrid.Height; y++)
                    {
                        var tile = DungeonGrid.GetTile(x, y, z);
                        if (tile == null) continue;
                        
                        var pos = new Vector3I(x, y, z);
                        
                        // Capture trap state
                        if (tile.Type == DungeonGrid.TileType.Trap && !string.IsNullOrEmpty(tile.TrapId))
                        {
                            _trapStates[$"{x},{y},{z}"] = new TrapState
                            {
                                Position = pos,
                                TrapId = tile.TrapId,
                                IsArmed = true,
                                CooldownRemaining = 0f
                            };
                        }
                        
                        // Capture garrison state
                        if (tile.GarrisonedMonsters.Count > 0)
                        {
                            _garrisonStates[pos] = new GarrisonState
                            {
                                Position = pos,
                                MonsterIds = new List<string>(tile.GarrisonedMonsters),
                                MaxCapacity = tile.GarrisonedMonsters.Count,
                                RespawnTimer = 0f
                            };
                        }
                        
                        // Capture loot state (simplified - could be expanded)
                        if (tile.Type == DungeonGrid.TileType.Room && 
                            (tile.RoomId == "treasure" || tile.RoomId == "boss"))
                        {
                            _lootStates[pos] = new LootState
                            {
                                Position = pos,
                                RoomType = tile.RoomId,
                                IsLooted = false,
                                LootTable = GetLootTableForRoom(tile.RoomId)
                            };
                        }
                    }
                }
            }
            
            GD.Print($"Captured initial state: {_trapStates.Count} traps, {_garrisonStates.Count} garrisons, {_lootStates.Count} loot containers");
        }
        
        /// <summary>
        /// Called when an invader wave is completed.
        /// </summary>
        public void OnWaveComplete()
        {
            _wavesSinceReset++;
            OnWaveCompleted?.Invoke(_wavesSinceReset);
            
            if (AutoResetEnabled && _wavesSinceReset >= WavesPerReset)
            {
                TriggerReset();
            }
        }
        
        /// <summary>
        /// Manually trigger a dungeon reset (e.g., from Builder Mode button).
        /// </summary>
        public void TriggerManualReset()
        {
            TriggerReset();
        }
        
        private void TriggerReset()
        {
            if (_isResetting) return;
            
            _isResetting = true;
            OnResetStarted?.Invoke();
            
            GD.Print("Dungeon reset initiated...");
            
            // Reset traps immediately
            if (ResetTraps)
                ResetAllTraps();
            
            // Reset loot immediately
            if (ResetLoot)
                ResetAllLoot();
            
            // Schedule garrison respawn
            if (ResetGarrisons)
                ScheduleGarrisonRespawn();
            
            // Reset wave counter and timer
            _wavesSinceReset = 0;
            _resetTimer = 0f;
            
            _isResetting = false;
            OnResetCompleted?.Invoke();
            
            GD.Print("Dungeon reset completed");
        }
        
        private void ResetAllTraps()
        {
            if (DungeonGrid == null) return;
            
            foreach (var kvp in _trapStates)
            {
                var state = kvp.Value;
                var tile = DungeonGrid.GetTile(state.Position.X, state.Position.Y, state.Position.Z);
                
                if (tile != null && tile.Type == DungeonGrid.TileType.Trap)
                {
                    state.IsArmed = true;
                    state.CooldownRemaining = 0f;
                    
                    // Ensure trap ID is set
                    if (string.IsNullOrEmpty(tile.TrapId))
                        tile.TrapId = state.TrapId;
                }
            }
            
            GD.Print($"Reset {_trapStates.Count} traps");
        }
        
        private void ResetAllLoot()
        {
            foreach (var kvp in _lootStates)
            {
                kvp.Value.IsLooted = false;
            }
            
            GD.Print($"Reset {_lootStates.Count} loot containers");
        }
        
        private void ScheduleGarrisonRespawn()
        {
            // In a full implementation, this would use a timer
            // For now, respawn immediately with delay tracking
            foreach (var kvp in _garrisonStates)
            {
                kvp.Value.RespawnTimer = GarrisonRespawnDelay;
            }
            
            // Process respawns (in real implementation, this would be in _Process)
            ProcessGarrisonRespawns(0f); // Immediate for now
        }
        
        public void ProcessGarrisonRespawns(float dt)
        {
            if (DungeonGrid == null) return;
            
            foreach (var kvp in _garrisonStates)
            {
                var state = kvp.Value;
                if (state.RespawnTimer > 0f)
                {
                    state.RespawnTimer -= dt;
                    if (state.RespawnTimer <= 0f)
                    {
                        RespawnGarrison(state);
                    }
                }
            }
        }
        
        private void RespawnGarrison(GarrisonState state)
        {
            var tile = DungeonGrid.GetTile(state.Position.X, state.Position.Y, state.Position.Z);
            if (tile == null) return;
            
            // Clear dead monsters, refill to max capacity
            tile.GarrisonedMonsters.Clear();
            
            foreach (var monsterId in state.MonsterIds)
            {
                tile.GarrisonedMonsters.Add(monsterId);
            }
            
            state.RespawnTimer = 0f;
            
            GD.Print($"Respawned garrison at {state.Position}: {state.MonsterIds.Count} monsters");
        }
        
        /// <summary>
        /// Called when a trap is triggered - starts cooldown.
        /// </summary>
        public void OnTrapTriggered(Vector3I position)
        {
            string key = $"{position.X},{position.Y},{position.Z}";
            if (_trapStates.TryGetValue(key, out var state))
            {
                state.IsArmed = false;
                state.CooldownRemaining = 10f; // Base cooldown, could be config per trap type
            }
        }
        
        /// <summary>
        /// Called when a monster dies in garrison.
        /// </summary>
        public void OnGarrisonMonsterDied(Vector3I position, string monsterId)
        {
            if (_garrisonStates.TryGetValue(position, out var state))
            {
                state.MonsterIds.Remove(monsterId);
            }
        }
        
        /// <summary>
        /// Called when loot is taken.
        /// </summary>
        public void OnLootTaken(Vector3I position)
        {
            if (_lootStates.TryGetValue(position, out var state))
            {
                state.IsLooted = true;
            }
        }
        
        /// <summary>
        /// Get trap state for UI/inspection.
        /// </summary>
        public TrapState GetTrapState(Vector3I position)
        {
            string key = $"{position.X},{position.Y},{position.Z}";
            _trapStates.TryGetValue(key, out var state);
            return state;
        }
        
        /// <summary>
        /// Get garrison state for UI/inspection.
        /// </summary>
        public GarrisonState GetGarrisonState(Vector3I position)
        {
            _garrisonStates.TryGetValue(position, out var state);
            return state;
        }
        
        /// <summary>
        /// Get loot state for UI/inspection.
        /// </summary>
        public LootState GetLootState(Vector3I position)
        {
            _lootStates.TryGetValue(position, out var state);
            return state;
        }
        
        private List<string> GetLootTableForRoom(string roomType)
        {
            return roomType switch
            {
                "treasure" => new List<string> { "essence_shard", "gold", "potion_minor", "scroll_identify" },
                "boss" => new List<string> { "essence_crystal", "artifact_fragment", "legendary_weapon", "gold_large" },
                "spawn" => new List<string> { "monster_essence", "spawn_token" },
                _ => new List<string> { "essence_shard" }
            };
        }
        
        // State data classes
        public class TrapState
        {
            public Vector3I Position { get; set; }
            public string TrapId { get; set; }
            public bool IsArmed { get; set; }
            public float CooldownRemaining { get; set; }
        }
        
        public class GarrisonState
        {
            public Vector3I Position { get; set; }
            public List<string> MonsterIds { get; set; } = new();
            public int MaxCapacity { get; set; }
            public float RespawnTimer { get; set; }
        }
        
        public class LootState
        {
            public Vector3I Position { get; set; }
            public string RoomType { get; set; }
            public bool IsLooted { get; set; }
            public List<string> LootTable { get; set; } = new();
        }
    }
}