using System;
using System.Collections.Generic;
using Godot;

namespace DungeonLord.Scripts
{
    /// <summary>
    /// Main GameManager: Links grid, controllers, EssenceManager, InvaderAI, PossessionManager,
    /// DungeonResetCycle, and UI into a complete playable dual-perspective game loop.
    /// </summary>
    [GlobalClass]
    public partial class GameManager : Node
    {
        // Core systems
        [Export] public DungeonGrid DungeonGrid { get; private set; }
        [Export] public EssenceManager EssenceManager { get; private set; }
        [Export] public LevelingEngine LevelingEngine { get; private set; }
        [Export] public BuilderController BuilderController { get; private set; }
        [Export] public CrawlController CrawlController { get; private set; }
        [Export] public InvaderAI InvaderAI { get; private set; }
        [Export] public PossessionManager PossessionManager { get; private set; }
        [Export] public DungeonResetCycle DungeonResetCycle { get; private set; }
        [Export] public MonsterProductionManager MonsterProductionManager { get; private set; }
        [Export] public MarketManager MarketManager { get; private set; }

        // UI
        [Export] public BuilderHUD BuilderHUD { get; private set; }
        [Export] public CrawlHUD CrawlHUD { get; private set; }

        // Game state
        private GameMode _currentMode = GameMode.Builder;
        private bool _isInitialized = false;
        private int _currentWave = 1;
        private float _waveTimer = 0f;
        private bool _waveInProgress = false;

        // Lord stats (persistent across modes)
        private LordState _lordState;

        public enum GameMode
        {
            Builder,
            Crawl
        }

        public GameMode CurrentMode => _currentMode;
        public LordState LordState => _lordState;
        public int CurrentWave => _currentWave;

        public event Action<GameMode> OnModeChanged;
        public event Action<int> OnWaveChanged;
        public event Action OnGameOver;

        public override void _Ready()
        {
            GD.Print("GameManager initializing...");

            // Find components if not assigned in editor
            FindComponents();

            // Create missing components
            CreateMissingComponents();

            // Initialize all systems
            InitializeSystems();

            // Start in Builder mode
            SwitchMode(GameMode.Builder);

            _isInitialized = true;
            GD.Print("GameManager initialized successfully");
        }

        private void FindComponents()
        {
            // These would normally be assigned in the editor, but we'll find/create them
            DungeonGrid ??= new DungeonGrid(32, 32, 3);
            EssenceManager ??= new EssenceManager(200);
            LevelingEngine ??= new LevelingEngine();
        }

        private void CreateMissingComponents()
        {
            // Create BuilderController if missing
            if (BuilderController == null)
            {
                BuilderController = new BuilderController();
                AddChild(BuilderController);
                BuilderController.Name = "BuilderController";
            }

            // Create CrawlController if missing
            if (CrawlController == null)
            {
                CrawlController = new CrawlController();
                AddChild(CrawlController);
                CrawlController.Name = "CrawlController";
            }

            // Create InvaderAI if missing
            if (InvaderAI == null)
            {
                InvaderAI = new InvaderAI();
                AddChild(InvaderAI);
                InvaderAI.Name = "InvaderAI";
            }

            // Create PossessionManager if missing
            if (PossessionManager == null)
            {
                PossessionManager = new PossessionManager();
                AddChild(PossessionManager);
                PossessionManager.Name = "PossessionManager";
            }

            // Create DungeonResetCycle if missing
            if (DungeonResetCycle == null)
            {
                DungeonResetCycle = new DungeonResetCycle();
                AddChild(DungeonResetCycle);
                DungeonResetCycle.Name = "DungeonResetCycle";
            }

            // Create MonsterProductionManager if missing
            if (MonsterProductionManager == null)
            {
                MonsterProductionManager = new MonsterProductionManager(EssenceManager, DungeonGrid);
                AddChild(MonsterProductionManager);
                MonsterProductionManager.Name = "MonsterProductionManager";
            }

            // Create MarketManager if missing
            if (MarketManager == null)
            {
                MarketManager = new MarketManager(EssenceManager, InvaderAI);
                AddChild(MarketManager);
                MarketManager.Name = "MarketManager";
            }

            // Create UI if missing
            if (BuilderHUD == null)
            {
                BuilderHUD = new BuilderHUD();
                AddChild(BuilderHUD);
                BuilderHUD.Name = "BuilderHUD";
            }

            if (CrawlHUD == null)
            {
                CrawlHUD = new CrawlHUD();
                AddChild(CrawlHUD);
                CrawlHUD.Name = "CrawlHUD";
            }
        }

        private void InitializeSystems()
        {
            // Initialize Lord state
            _lordState = new LordState
            {
                CurrentHP = 100,
                MaxHP = 100,
                Level = 1,
                XP = 0,
                EssenceCarried = 0,
                Position = new Vector3I(16, 16, 0),
                Facing = CrawlController.Direction.North
            };

            // Initialize BuilderController
            BuilderController.Initialize(DungeonGrid, EssenceManager);
            BuilderController.OnModeSwitchRequested += () => RequestModeSwitch();
            BuilderController.OnRoomPlaced += OnRoomPlaced;
            BuilderController.OnTrapPlaced += OnTrapPlaced;
            BuilderController.OnSpawnPointPlaced += OnSpawnPointPlaced;

            // Initialize CrawlController
            CrawlController.Initialize(DungeonGrid, _lordState.Position, _lordState.Facing);
            CrawlController.OnModeSwitchRequested += () => RequestModeSwitch();
            CrawlController.OnPositionChanged += OnCrawlPositionChanged;

            // Initialize InvaderAI
            InvaderAI.DungeonGrid = DungeonGrid;
            InvaderAI.EssenceManager = EssenceManager;
            InvaderAI.OnPartySpawned += OnInvaderPartySpawned;
            InvaderAI.OnPartyDestroyed += OnInvaderPartyDestroyed;
            InvaderAI.OnPartyReachedTarget += OnInvaderReachedTarget;

            // Initialize PossessionManager
            PossessionManager.DungeonGrid = DungeonGrid;
            PossessionManager.CrawlController = CrawlController;
            PossessionManager.OnPossessionStarted += OnPossessionStarted;
            PossessionManager.OnPossessionEnded += OnPossessionEnded;

            // Initialize DungeonResetCycle
            DungeonResetCycle.DungeonGrid = DungeonGrid;
            DungeonResetCycle.EssenceManager = EssenceManager;
            DungeonResetCycle.InvaderAI = InvaderAI;
            DungeonResetCycle.OnResetCompleted += OnDungeonResetCompleted;
            DungeonResetCycle.OnWaveCompleted += OnWaveCompleted;
            DungeonResetCycle.CaptureInitialState();

            // Initialize MonsterProductionManager
            MonsterProductionManager.OnProductionStarted += OnProductionStarted;
            MonsterProductionManager.OnProductionCompleted += OnProductionCompleted;

            // Initialize MarketManager
            MarketManager.OnGoldChanged += OnGoldChanged;
            MarketManager.OnItemPurchased += OnItemPurchased;
            MarketManager.OnItemSold += OnItemSold;

            // Initialize UI
            BuilderHUD.Initialize(BuilderController, EssenceManager, DungeonResetCycle);
            CrawlHUD.Initialize(CrawlController, PossessionManager, InvaderAI, DungeonGrid);
            CrawlHUD.SetLordStats(_lordState.CurrentHP, _lordState.MaxHP, _lordState.Level, _lordState.XP, _lordState.EssenceCarried);
            CrawlHUD.SetCurrentWave(_currentWave);

            // Connect EssenceManager events
            EssenceManager.OnEssenceChanged += OnEssenceChanged;
            EssenceManager.OnRankUp += OnRankUp;
        }

        public override void _Process(double delta)
        {
            if (!_isInitialized) return;

            float dt = (float)delta;

            // Update wave timer
            if (_waveInProgress)
            {
                _waveTimer += dt;
            }

            // Process dungeon reset cycle
            DungeonResetCycle?.ProcessGarrisonRespawns(dt);

            // Process monster production
            MonsterProductionManager?.Update(dt);

            // Process market restocks (convert delta to hours)
            MarketManager?.UpdateRestocks(dt / 3600f);

            // Auto-spawn waves if enabled (for testing)
            // In real game, this would be triggered by reputation/time
        }

        /// <summary>
        /// Request a mode switch (Builder ↔ Crawl)
        /// </summary>
        public void RequestModeSwitch()
        {
            GameMode newMode = _currentMode == GameMode.Builder ? GameMode.Crawl : GameMode.Builder;
            SwitchMode(newMode);
        }

        /// <summary>
        /// Switch to a specific mode
        /// </summary>
        public void SwitchMode(GameMode mode)
        {
            if (_currentMode == mode) return;

            GD.Print($"Switching to {mode} mode...");

            // Exit current mode
            switch (_currentMode)
            {
                case GameMode.Builder:
                    BuilderController.ExitBuilderMode();
                    BuilderHUD.Visible = false;
                    break;
                case GameMode.Crawl:
                    CrawlController.ExitCrawlMode();
                    CrawlHUD.Visible = false;
                    break;
            }

            // Enter new mode
            _currentMode = mode;

            switch (_currentMode)
            {
                case GameMode.Builder:
                    BuilderController.EnterBuilderMode();
                    BuilderHUD.Visible = true;
                    // Sync Lord position to grid
                    _lordState.Position = CrawlController.GridPosition;
                    _lordState.Facing = CrawlController.Facing;
                    break;
                case GameMode.Crawl:
                    CrawlController.EnterCrawlMode(_lordState.Position, _lordState.Facing);
                    CrawlHUD.Visible = true;
                    break;
            }

            OnModeChanged?.Invoke(_currentMode);
            GD.Print($"Switched to {_currentMode} mode");
        }

        /// <summary>
        /// Handle essence changes from any source
        /// </summary>
        private void OnEssenceChanged(long essence)
        {
            GD.Print($"Essence changed: {essence}");
        }

        /// <summary>
        /// Handle dungeon rank up
        /// </summary>
        private void OnRankUp(int rank)
        {
            GD.Print($"Dungeon Rank Up! New rank: {rank}");
            // Unlock new room types, traps, monsters based on rank
            UnlockContentForRank(rank);
        }

        /// <summary>
        /// Unlock content based on dungeon rank
        /// </summary>
        private void UnlockContentForRank(int rank)
        {
            // This would enable new buttons in the UI palettes
            GD.Print($"Content unlocked for rank {rank}");
        }

        /// <summary>
        /// Handle room placement
        /// </summary>
        private void OnRoomPlaced(string roomType, Vector3I position)
        {
            GD.Print($"Room placed: {roomType} at {position}");
        }

        /// <summary>
        /// Handle trap placement
        /// </summary>
        private void OnTrapPlaced(string trapType, Vector3I position)
        {
            GD.Print($"Trap placed: {trapType} at {position}");
            // Notify reset cycle
            DungeonResetCycle?.OnTrapTriggered(position); // This captures initial state
        }

        /// <summary>
        /// Handle spawn point placement
        /// </summary>
        private void OnSpawnPointPlaced(Vector3I position)
        {
            GD.Print($"Spawn point placed at {position}");
        }

        /// <summary>
        /// Handle crawl position change
        /// </summary>
        private void OnCrawlPositionChanged(Vector3I position, CrawlController.Direction facing)
        {
            _lordState.Position = position;
            _lordState.Facing = facing;
        }

        /// <summary>
        /// Handle invader party spawned
        /// </summary>
        private void OnInvaderPartySpawned(InvaderAI.InvaderParty party)
        {
            GD.Print($"Invader party {party.Id} spawned with {party.Members.Count} members");
            _waveInProgress = true;
            _waveTimer = 0f;
        }

        /// <summary>
        /// Handle invader party destroyed
        /// </summary>
        private void OnInvaderPartyDestroyed(InvaderAI.InvaderParty party)
        {
            GD.Print($"Invader party {party.Id} destroyed");

            // Award essence based on party difficulty
            long essenceReward = CalculateEssenceReward(party);
            EssenceManager.AddEssence(essenceReward);
            _lordState.EssenceCarried += essenceReward;
            CrawlHUD.SetLordStats(_lordState.CurrentHP, _lordState.MaxHP, _lordState.Level, _lordState.XP, _lordState.EssenceCarried);

            // Award XP to Lord
            long xpReward = CalculateXpReward(party);
            _lordState.XP += xpReward;
            CheckLordLevelUp();

            // Check if wave is complete
            CheckWaveComplete();
        }

        /// <summary>
        /// Handle invader reaching target (dungeon core)
        /// </summary>
        private void OnInvaderReachedTarget(InvaderAI.InvaderParty party, Vector3I target)
        {
            GD.Print($"Invader party {party.Id} reached target at {target}!");

            // Damage Lord if at core
            if (_currentMode == GameMode.Crawl)
            {
                // Direct combat would happen here
                TakeLordDamage(20);
            }
            else
            {
                // Auto-resolve in builder mode
                TakeLordDamage(10);
            }

            CheckWaveComplete();
        }

        /// <summary>
        /// Calculate essence reward from defeating a party
        /// </summary>
        private long CalculateEssenceReward(InvaderAI.InvaderParty party)
        {
            long baseReward = 50;
            foreach (var member in party.Members)
            {
                baseReward += member.Level * 10;
            }
            return baseReward * (long)Math.Max(1, InvaderAI.SettlementReputation);
        }

        /// <summary>
        /// Calculate XP reward from defeating a party
        /// </summary>
        private long CalculateXpReward(InvaderAI.InvaderParty party)
        {
            long baseReward = 100;
            foreach (var member in party.Members)
            {
                baseReward += member.Level * 20;
            }
            return baseReward;
        }

        /// <summary>
        /// Check if current wave is complete
        /// </summary>
        private void CheckWaveComplete()
        {
            if (InvaderAI != null && InvaderAI.ActiveParties.Count == 0 && _waveInProgress)
            {
                _waveInProgress = false;
                GD.Print($"Wave {_currentWave} complete!");

                // Trigger dungeon reset cycle wave counter
                DungeonResetCycle?.OnWaveComplete();

                // Start next wave after delay
                StartNextWave();
            }
        }

        /// <summary>
        /// Start the next wave
        /// </summary>
        private void StartNextWave()
        {
            _currentWave++;
            OnWaveChanged?.Invoke(_currentWave);
            CrawlHUD.SetCurrentWave(_currentWave);
            GD.Print($"Starting wave {_currentWave}");

            // Increase reputation slightly each wave
            InvaderAI?.ModifyReputation(0.1f);
            CrawlHUD.UpdateReputationDisplay();

            // In a real game, you'd have a wave timer before next spawn
            // For now, the InvaderAI will spawn based on its timer
        }

        /// <summary>
        /// Handle wave completed event from reset cycle
        /// </summary>
        private void OnWaveCompleted(int wavesSinceReset)
        {
            GD.Print($"Waves since reset: {wavesSinceReset}");
        }

        /// <summary>
        /// Handle possession started
        /// </summary>
        private void OnPossessionStarted(string monsterId, Vector3I position)
        {
            GD.Print($"Possession started: {monsterId}");
        }

        /// <summary>
        /// Handle possession ended
        /// </summary>
        private void OnPossessionEnded(string monsterId)
        {
            GD.Print($"Possession ended: {monsterId}");
            // Restore Lord state
            CrawlController.EnterCrawlMode(_lordState.Position, _lordState.Facing);
        }

        /// <summary>
        /// Handle dungeon reset completed
        /// </summary>
        private void OnDungeonResetCompleted()
        {
            GD.Print("Dungeon reset completed - ready for next waves");
        }

        /// <summary>
        /// Apply damage to Lord
        /// </summary>
        public void TakeLordDamage(int damage)
        {
            _lordState.CurrentHP = Math.Max(0, _lordState.CurrentHP - damage);
            CrawlHUD.SetLordStats(_lordState.CurrentHP, _lordState.MaxHP, _lordState.Level, _lordState.XP, _lordState.EssenceCarried);

            GD.Print($"Lord took {damage} damage. HP: {_lordState.CurrentHP}/{_lordState.MaxHP}");

            if (_lordState.CurrentHP <= 0)
            {
                OnLordDeath();
            }
        }

        /// <summary>
        /// Heal Lord
        /// </summary>
        public void HealLord(int amount)
        {
            _lordState.CurrentHP = Math.Min(_lordState.MaxHP, _lordState.CurrentHP + amount);
            CrawlHUD.SetLordStats(_lordState.CurrentHP, _lordState.MaxHP, _lordState.Level, _lordState.XP, _lordState.EssenceCarried);
        }

        /// <summary>
        /// Handle Lord death
        /// </summary>
        private void OnLordDeath()
        {
            GD.Print("LORD HAS FALLEN! Game Over.");
            OnGameOver?.Invoke();

            // Could trigger respawn with penalty, or end game
            // For now, just reset to builder mode with some penalty
            _lordState.CurrentHP = _lordState.MaxHP / 2; // Respawn at half HP
            SwitchMode(GameMode.Builder);

            // Penalty: lose some essence
            long penalty = EssenceManager.CurrentEssence / 4;
            EssenceManager.SpendEssence(penalty);
            _lordState.EssenceCarried = Math.Max(0, _lordState.EssenceCarried - penalty);

            CrawlHUD.SetLordStats(_lordState.CurrentHP, _lordState.MaxHP, _lordState.Level, _lordState.XP, _lordState.EssenceCarried);
        }

        /// <summary>
        /// Check for Lord level up
        /// </summary>
        private void CheckLordLevelUp()
        {
            long xpForNext = CalculateXpForLevel(_lordState.Level + 1);
            if (_lordState.XP >= xpForNext)
            {
                _lordState.Level++;
                _lordState.XP -= xpForNext;

                // Increase stats using shared growth formula
                float oldMaxHP = _lordState.MaxHP;
                _lordState.MaxHP = (int)LevelingEngine.CalculateAttribute(100, _lordState.Level);
                _lordState.CurrentHP = _lordState.MaxHP; // Full heal on level up

                GD.Print($"Lord Level Up! Now level {_lordState.Level}. Max HP: {oldMaxHP} -> {_lordState.MaxHP}");

                CrawlHUD.SetLordStats(_lordState.CurrentHP, _lordState.MaxHP, _lordState.Level, _lordState.XP, _lordState.EssenceCarried);
            }
        }

        private long CalculateXpForLevel(int level)
        {
            return 100L * level * level;
        }

        /// <summary>
        /// Add essence to Lord's carried amount (from crawling)
        /// </summary>
        public void AddEssenceToLord(long amount)
        {
            _lordState.EssenceCarried += amount;
            CrawlHUD.SetLordStats(_lordState.CurrentHP, _lordState.MaxHP, _lordState.Level, _lordState.XP, _lordState.EssenceCarried);
        }

        /// <summary>
        /// Deposit Lord's carried essence into dungeon storage
        /// </summary>
        public void DepositEssence()
        {
            if (_lordState.EssenceCarried > 0)
            {
                EssenceManager.AddEssence(_lordState.EssenceCarried);
                _lordState.EssenceCarried = 0;
                CrawlHUD.SetLordStats(_lordState.CurrentHP, _lordState.MaxHP, _lordState.Level, _lordState.XP, _lordState.EssenceCarried);
                GD.Print("Deposited carried essence into dungeon storage");
            }
        }

        /// <summary>
        /// Handle production started
        /// </summary>
        private void OnProductionStarted(ProductionJob job, string itemId)
        {
            GD.Print($"Production started: {itemId} in {job.RoomType} at {job.RoomPosition}");
        }

        /// <summary>
        /// Handle production completed
        /// </summary>
        private void OnProductionCompleted(ProductionJob job, string itemId)
        {
            GD.Print($"Production completed: {itemId} x{job.Quantity} from {job.RoomType} at {job.RoomPosition}");
            // In full impl: add to dungeon storage/inventory
        }

        /// <summary>
        /// Handle gold changed
        /// </summary>
        private void OnGoldChanged(long gold)
        {
            GD.Print($"Gold changed: {gold}");
        }

        /// <summary>
        /// Handle item purchased
        /// </summary>
        private void OnItemPurchased(string itemId, int quantity)
        {
            GD.Print($"Purchased: {itemId} x{quantity}");
        }

        /// <summary>
        /// Handle item sold
        /// </summary>
        private void OnItemSold(string itemId)
        {
            GD.Print($"Sold: {itemId}");
        }

        /// <summary>
        /// Save game state (simplified - would use SQLite in production)
        /// </summary>
        public void SaveGame()
        {
            var saveData = new GameSaveData
            {
                DungeonGrid = SerializeGrid(),
                LordState = _lordState,
                Essence = EssenceManager.CurrentEssence,
                DungeonRank = EssenceManager.DungeonRank,
                CurrentWave = _currentWave,
                SettlementReputation = InvaderAI.SettlementReputation,
                CurrentMode = _currentMode,
                LordPosition = _lordState.Position,
                LordFacing = _lordState.Facing
            };

            // In production: save to file/JSON/SQLite
            GD.Print("Game saved (stub)");
        }

        /// <summary>
        /// Load game state
        /// </summary>
        public void LoadGame()
        {
            // In production: load from file/JSON/SQLite
            GD.Print("Game loaded (stub)");
        }

        private object SerializeGrid()
        {
            // Simplified serialization
            return new { Width = DungeonGrid.Width, Height = DungeonGrid.Height, Floors = DungeonGrid.Floors };
        }

        public override void _ExitTree()
        {
            // Cleanup events
            if (BuilderController != null)
            {
                BuilderController.OnModeSwitchRequested -= () => RequestModeSwitch();
                BuilderController.OnRoomPlaced -= OnRoomPlaced;
                BuilderController.OnTrapPlaced -= OnTrapPlaced;
                BuilderController.OnSpawnPointPlaced -= OnSpawnPointPlaced;
            }

            if (CrawlController != null)
            {
                CrawlController.OnModeSwitchRequested -= () => RequestModeSwitch();
                CrawlController.OnPositionChanged -= OnCrawlPositionChanged;
            }

            if (InvaderAI != null)
            {
                InvaderAI.OnPartySpawned -= OnInvaderPartySpawned;
                InvaderAI.OnPartyDestroyed -= OnInvaderPartyDestroyed;
                InvaderAI.OnPartyReachedTarget -= OnInvaderReachedTarget;
            }

            if (PossessionManager != null)
            {
                PossessionManager.OnPossessionStarted -= OnPossessionStarted;
                PossessionManager.OnPossessionEnded -= OnPossessionEnded;
            }

            if (DungeonResetCycle != null)
            {
                DungeonResetCycle.OnResetCompleted -= OnDungeonResetCompleted;
                DungeonResetCycle.OnWaveCompleted -= OnWaveCompleted;
            }

            if (MonsterProductionManager != null)
            {
                MonsterProductionManager.OnProductionStarted -= OnProductionStarted;
                MonsterProductionManager.OnProductionCompleted -= OnProductionCompleted;
            }

            if (MarketManager != null)
            {
                MarketManager.OnGoldChanged -= OnGoldChanged;
                MarketManager.OnItemPurchased -= OnItemPurchased;
                MarketManager.OnItemSold -= OnItemSold;
            }

            if (EssenceManager != null)
            {
                EssenceManager.OnEssenceChanged -= OnEssenceChanged;
                EssenceManager.OnRankUp -= OnRankUp;
            }
        }
    }

    /// <summary>
    /// Persistent Lord state across modes
    /// </summary>
    public class LordState
    {
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public int Level { get; set; }
        public long XP { get; set; }
        public long EssenceCarried { get; set; }
        public Vector3I Position { get; set; }
        public CrawlController.Direction Facing { get; set; }
        public Dictionary<string, object> Equipment { get; set; } = new();
        public List<string> Abilities { get; set; } = new();
    }

    /// <summary>
    /// Game save data structure
    /// </summary>
    public class GameSaveData
    {
        public object DungeonGrid { get; set; }
        public LordState LordState { get; set; }
        public long Essence { get; set; }
        public int DungeonRank { get; set; }
        public int CurrentWave { get; set; }
        public float SettlementReputation { get; set; }
        public GameManager.GameMode CurrentMode { get; set; }
        public Vector3I LordPosition { get; set; }
        public CrawlController.Direction LordFacing { get; set; }
    }
}