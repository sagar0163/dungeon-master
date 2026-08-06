using System;
using System.Collections.Generic;
using Godot;

namespace DungeonLord.Scripts
{
    /// <summary>
    /// Manages Dungeon Lord possession of garrisoned monsters.
    /// Limited duration, cooldown, line-of-sight required for initiation.
    /// </summary>
    [GlobalClass]
    public partial class PossessionManager : Node
    {
        // Configuration
        [Export] public float MaxPossessionDuration { get; set; } = 30.0f; // seconds
        [Export] public float PossessionCooldown { get; set; } = 60.0f; // seconds
        [Export] public float PossessionRange { get; set; } = 10.0f; // tiles
        [Export] public bool RequireLineOfSight { get; set; } = true;
        
        // References
        [Export] public DungeonGrid DungeonGrid { get; private set; }
        [Export] public CrawlController CrawlController { get; private set; }
        
        // State
        private bool _isPossessing = false;
        private string _possessedMonsterId = "";
        private Vector3I _possessedPosition = new Vector3I(-1, -1, -1);
        private float _possessionTimer = 0f;
        private float _cooldownTimer = 0f;
        private MonsterInstance _originalLordState;
        
        public event Action<string, Vector3I> OnPossessionStarted;
        public event Action<string> OnPossessionEnded;
        public event Action<float> OnPossessionTimerChanged;
        
        public bool IsPossessing => _isPossessing;
        public string PossessedMonsterId => _possessedMonsterId;
        public float RemainingTime => Math.Max(0f, MaxPossessionDuration - _possessionTimer);
        public float CooldownRemaining => Math.Max(0f, PossessionCooldown - _cooldownTimer);
        public bool CanPossess => !_isPossessing && _cooldownTimer <= 0f;
        
        public override void _Ready()
        {
            GD.Print("PossessionManager initialized");
        }
        
        public override void _Process(double delta)
        {
            float dt = (float)delta;
            
            if (_isPossessing)
            {
                _possessionTimer += dt;
                OnPossessionTimerChanged?.Invoke(RemainingTime);
                
                if (_possessionTimer >= MaxPossessionDuration)
                {
                    EndPossession();
                }
            }
            else if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= dt;
            }
        }
        
        /// <summary>
        /// Attempt to possess a monster at the given grid position.
        /// Requires line of sight from Lord's current position.
        /// </summary>
        public bool TryPossess(Vector3I targetPos)
        {
            if (!CanPossess) return false;
            if (DungeonGrid == null) return false;
            
            var tile = DungeonGrid.GetTile(targetPos.X, targetPos.Y, targetPos.Z);
            if (tile == null || tile.GarrisonedMonsters.Count == 0) return false;
            
            // Check range from Lord's position
            if (CrawlController != null)
            {
                var lordPos = CrawlController.GridPosition;
                float distance = Math.Abs(lordPos.X - targetPos.X) + Math.Abs(lordPos.Y - targetPos.Y);
                if (distance > PossessionRange) return false;
                
                // Check line of sight
                if (RequireLineOfSight && !HasLineOfSight(lordPos, targetPos))
                    return false;
            }
            
            // Possess the first available monster
            string monsterId = tile.GarrisonedMonsters[0];
            StartPossession(monsterId, targetPos);
            return true;
        }
        
        /// <summary>
        /// Start possessing a specific monster by ID at position.
        /// </summary>
        public void StartPossession(string monsterId, Vector3I position)
        {
            if (_isPossessing) return;
            
            // Store Lord's original state for restoration
            if (CrawlController != null)
            {
                _originalLordState = new MonsterInstance
                {
                    Id = "lord",
                    Position = CrawlController.GridPosition,
                    Facing = CrawlController.Facing,
                    CurrentHP = 100, // Would come from Lord stats
                    MaxHP = 100
                };
            }
            
            _isPossessing = true;
            _possessedMonsterId = monsterId;
            _possessedPosition = position;
            _possessionTimer = 0f;
            
            // Move camera/controller to monster position
            if (CrawlController != null)
            {
                CrawlController.EnterCrawlMode(position, CrawlController.Direction.North);
            }
            
            OnPossessionStarted?.Invoke(monsterId, position);
            GD.Print($"Possession started: {monsterId} at {position}");
        }
        
        /// <summary>
        /// End possession early, return to Lord body.
        /// </summary>
        public void EndPossession()
        {
            if (!_isPossessing) return;
            
            string endedMonster = _possessedMonsterId;
            
            _isPossessing = false;
            _possessedMonsterId = "";
            _possessedPosition = new Vector3I(-1, -1, -1);
            _possessionTimer = 0f;
            _cooldownTimer = PossessionCooldown;
            
            // Return to Lord's original position
            if (CrawlController != null && _originalLordState != null)
            {
                CrawlController.EnterCrawlMode(_originalLordState.Position, _originalLordState.Facing);
            }
            
            OnPossessionEnded?.Invoke(endedMonster);
            GD.Print($"Possession ended: {endedMonster}, cooldown: {PossessionCooldown}s");
        }
        
        /// <summary>
        /// Check line of sight between two grid positions using Bresenham.
        /// </summary>
        private bool HasLineOfSight(Vector3I from, Vector3I to)
        {
            if (DungeonGrid == null) return false;
            
            int x0 = from.X, y0 = from.Y;
            int x1 = to.X, y1 = to.Y;
            
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            
            while (true)
            {
                var tile = DungeonGrid.GetTile(x0, y0, from.Z);
                if (tile != null && tile.Type == DungeonGrid.TileType.Empty)
                    return false; // Blocked by empty space (wall)
                
                if (x0 == x1 && y0 == y1) break;
                
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get list of possessable monsters within range.
        /// </summary>
        public List<(string monsterId, Vector3I position)> GetPossessableTargets()
        {
            var targets = new List<(string, Vector3I)>();
            
            if (DungeonGrid == null || CrawlController == null) return targets;
            
            var lordPos = CrawlController.GridPosition;
            
            for (int z = 0; z < DungeonGrid.Floors; z++)
            {
                for (int x = 0; x < DungeonGrid.Width; x++)
                {
                    for (int y = 0; y < DungeonGrid.Height; y++)
                    {
                        var tile = DungeonGrid.GetTile(x, y, z);
                        if (tile?.GarrisonedMonsters.Count > 0)
                        {
                            var pos = new Vector3I(x, y, z);
                            float dist = Math.Abs(lordPos.X - x) + Math.Abs(lordPos.Y - y);
                            if (dist <= PossessionRange)
                            {
                                bool los = !RequireLineOfSight || HasLineOfSight(lordPos, pos);
                                if (los)
                                {
                                    foreach (var monsterId in tile.GarrisonedMonsters)
                                        targets.Add((monsterId, pos));
                                }
                            }
                        }
                    }
                }
            }
            
            return targets;
        }
    }
    
    /// <summary>
    /// Simple monster instance for possession state tracking.
    /// </summary>
    public class MonsterInstance
    {
        public string Id { get; set; }
        public Vector3I Position { get; set; }
        public CrawlController.Direction Facing { get; set; }
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public Dictionary<string, object> CustomData { get; set; } = new();
    }
}