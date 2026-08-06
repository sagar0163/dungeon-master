using System;
using Godot;

namespace DungeonLord.Scripts.UI
{
    /// <summary>
    /// Builder Mode HUD: Essence counter, rank display, room/trap palette buttons, floor controls.
    /// </summary>
    [GlobalClass]
    public partial class BuilderHUD : Control
    {
        // UI References
        [Export] public Label EssenceLabel { get; private set; }
        [Export] public Label CapacityLabel { get; private set; }
        [Export] public Label RankLabel { get; private set; }
        [Export] public Label FloorLabel { get; private set; }
        [Export] public HBoxContainer ToolPalette { get; private set; }
        [Export] public HBoxContainer RoomPalette { get; private set; }
        [Export] public HBoxContainer TrapPalette { get; private set; }
        [Export] public HBoxContainer SpawnPalette { get; private set; }
        [Export] public VBoxContainer FloorControls { get; private set; }
        [Export] public Button FloorUpButton { get; private set; }
        [Export] public Button FloorDownButton { get; private set; }
        [Export] public Button SwitchModeButton { get; private set; }
        [Export] public Button ResetDungeonButton { get; private set; }

        // State
        private BuilderController _builderController;
        private EssenceManager _essenceManager;
        private DungeonResetCycle _resetCycle;

        // Tool buttons
        private Button _toolSelectBtn;
        private Button _toolRoomBtn;
        private Button _toolTrapBtn;
        private Button _toolSpawnBtn;
        private Button _toolDeleteBtn;

        public override void _Ready()
        {
            // Auto-find UI elements if not assigned in editor
            EssenceLabel ??= GetNodeOrNull<Label>("%EssenceLabel");
            CapacityLabel ??= GetNodeOrNull<Label>("%CapacityLabel");
            RankLabel ??= GetNodeOrNull<Label>("%RankLabel");
            FloorLabel ??= GetNodeOrNull<Label>("%FloorLabel");
            ToolPalette ??= GetNodeOrNull<HBoxContainer>("%ToolPalette");
            RoomPalette ??= GetNodeOrNull<HBoxContainer>("%RoomPalette");
            TrapPalette ??= GetNodeOrNull<HBoxContainer>("%TrapPalette");
            SpawnPalette ??= GetNodeOrNull<HBoxContainer>("%SpawnPalette");
            FloorControls ??= GetNodeOrNull<VBoxContainer>("%FloorControls");
            FloorUpButton ??= GetNodeOrNull<Button>("%FloorUpButton");
            FloorDownButton ??= GetNodeOrNull<Button>("%FloorDownButton");
            SwitchModeButton ??= GetNodeOrNull<Button>("%SwitchModeButton");
            ResetDungeonButton ??= GetNodeOrNull<Button>("%ResetDungeonButton");

            // Build tool palette if empty
            if (ToolPalette != null && ToolPalette.GetChildCount() == 0)
            {
                BuildToolPalette();
            }

            // Build room/trap/spawn palettes
            if (RoomPalette != null && RoomPalette.GetChildCount() == 0)
            {
                BuildRoomPalette();
            }
            if (TrapPalette != null && TrapPalette.GetChildCount() == 0)
            {
                BuildTrapPalette();
            }
            if (SpawnPalette != null && SpawnPalette.GetChildCount() == 0)
            {
                BuildSpawnPalette();
            }

            // Connect floor buttons
            FloorUpButton?.Pressed += () => _builderController?.ChangeFloor(_builderController.CurrentFloor + 1);
            FloorDownButton?.Pressed += () => _builderController?.ChangeFloor(_builderController.CurrentFloor - 1);

            // Connect mode switch
            SwitchModeButton?.Pressed += () => _builderController?.OnModeSwitchRequested?.Invoke();

            // Connect reset button
            ResetDungeonButton?.Pressed += () => _resetCycle?.TriggerManualReset();

            GD.Print("BuilderHUD initialized");
        }

        public void Initialize(BuilderController builderController, EssenceManager essenceManager, DungeonResetCycle resetCycle)
        {
            _builderController = builderController;
            _essenceManager = essenceManager;
            _resetCycle = resetCycle;

            // Subscribe to essence changes
            _essenceManager.OnEssenceChanged += OnEssenceChanged;
            _essenceManager.OnRankUp += OnRankUp;

            // Subscribe to floor changes
            _builderController.OnFloorChanged += OnFloorChanged;

            // Subscribe to reset cycle
            _resetCycle.OnResetTimerChanged += OnResetTimerChanged;
            _resetCycle.OnResetStarted += OnResetStarted;
            _resetCycle.OnResetCompleted += OnResetCompleted;

            // Initial update
            UpdateEssenceDisplay();
            UpdateRankDisplay();
            UpdateFloorDisplay();
            UpdateResetTimerDisplay();
        }

        private void BuildToolPalette()
        {
            var toolData = new[]
            {
                (BuilderController.BuildTool.Select, "Select", "Tool for selecting tiles", Key.Key1),
                (BuilderController.BuildTool.Room, "Room", "Place rooms (corridor, spawn, treasure, etc.)", Key.Key2),
                (BuilderController.BuildTool.Trap, "Trap", "Place traps on corridors/rooms", Key.Key3),
                (BuilderController.BuildTool.SpawnPoint, "Spawn", "Place monster spawn points", Key.Key4),
                (BuilderController.BuildTool.Delete, "Delete", "Remove placed elements", Key.Key5)
            };

            foreach (var (tool, name, tooltip, key) in toolData)
            {
                var btn = new Button
                {
                    Text = name,
                    TooltipText = $"{tooltip} ({key})",
                    ToggleMode = true,
                    ButtonGroup = new ButtonGroup()
                };

                btn.Toggled += (pressed) =>
                {
                    if (pressed)
                    {
                        _builderController?.SetTool(tool);
                        UpdateToolSelection(tool);
                    }
                };

                ToolPalette.AddChild(btn);

                // Store reference for selection update
                switch (tool)
                {
                    case BuilderController.BuildTool.Select: _toolSelectBtn = btn; break;
                    case BuilderController.BuildTool.Room: _toolRoomBtn = btn; break;
                    case BuilderController.BuildTool.Trap: _toolTrapBtn = btn; break;
                    case BuilderController.BuildTool.SpawnPoint: _toolSpawnBtn = btn; break;
                    case BuilderController.BuildTool.Delete: _toolDeleteBtn = btn; break;
                }
            }

            // Default select tool
            _toolSelectBtn?.EmitSignal(Button.SignalName.Toggled, true);
        }

        private void BuildRoomPalette()
        {
            var roomTypes = new[]
            {
                ("corridor", "Corridor", "Basic passage"),
                ("spawn", "Spawn Room", "Monster spawn point"),
                ("treasure", "Treasure Room", "Loot and essence"),
                ("trap_room", "Trap Room", "Trap-heavy room"),
                ("boss", "Boss Room", "High-tier monster lair"),
                ("shrine", "Shrine", "Buffs and upgrades"),
                ("barracks", "Barracks", "Monster housing")
            };

            foreach (var (id, name, tooltip) in roomTypes)
            {
                var btn = new Button
                {
                    Text = name,
                    TooltipText = tooltip,
                    ToggleMode = true,
                    ButtonGroup = new ButtonGroup()
                };

                btn.Toggled += (pressed) =>
                {
                    if (pressed)
                    {
                        _builderController?.SetSelectedRoomType(id);
                    }
                };

                RoomPalette.AddChild(btn);
            }
        }

        private void BuildTrapPalette()
        {
            var trapTypes = new[]
            {
                ("spike", "Spike Trap", "Physical damage"),
                ("fire", "Fire Trap", "Burn damage over time"),
                ("poison", "Poison Trap", "Poison damage over time"),
                ("alarm", "Alarm Trap", "Alerts nearby monsters"),
                ("slow", "Slow Trap", "Reduces movement speed"),
                ("fear", "Fear Trap", "Causes invaders to flee")
            };

            foreach (var (id, name, tooltip) in trapTypes)
            {
                var btn = new Button
                {
                    Text = name,
                    TooltipText = tooltip,
                    ToggleMode = true,
                    ButtonGroup = new ButtonGroup()
                };

                btn.Toggled += (pressed) =>
                {
                    if (pressed)
                    {
                        _builderController?.SetSelectedTrapType(id);
                    }
                };

                TrapPalette.AddChild(btn);
            }
        }

        private void BuildSpawnPalette()
        {
            var spawnTypes = new[]
            {
                ("goblin_t1", "Goblin", "Tier 1 - Weak melee"),
                ("rat_t1", "Giant Rat", "Tier 1 - Fast, low HP"),
                ("skeleton_t1", "Skeleton", "Tier 1 - Resistant to physical"),
                ("hobgoblin_t2", "Hobgoblin", "Tier 2 - Strong melee"),
                ("wraith_t2", "Wraith", "Tier 2 - Magic damage"),
                ("ogre_t2", "Ogre", "Tier 2 - High HP, slow"),
                ("troll_t3", "Troll", "Tier 3 - Regenerates"),
                ("vampire_t3", "Vampire", "Tier 3 - Life steal"),
                ("lich_t3", "Lich", "Tier 3 - Spellcaster")
            };

            foreach (var (id, name, tooltip) in spawnTypes)
            {
                var btn = new Button
                {
                    Text = name,
                    TooltipText = tooltip,
                    ToggleMode = true,
                    ButtonGroup = new ButtonGroup()
                };

                btn.Toggled += (pressed) =>
                {
                    if (pressed)
                    {
                        // Could extend builder controller to handle monster spawn type
                        GD.Print($"Selected spawn type: {id}");
                    }
                };

                SpawnPalette.AddChild(btn);
            }
        }

        private void UpdateToolSelection(BuilderController.BuildTool tool)
        {
            _toolSelectBtn?.SetPressedNoSignal(tool == BuilderController.BuildTool.Select);
            _toolRoomBtn?.SetPressedNoSignal(tool == BuilderController.BuildTool.Room);
            _toolTrapBtn?.SetPressedNoSignal(tool == BuilderController.BuildTool.Trap);
            _toolSpawnBtn?.SetPressedNoSignal(tool == BuilderController.BuildTool.SpawnPoint);
            _toolDeleteBtn?.SetPressedNoSignal(tool == BuilderController.BuildTool.Delete);
        }

        private void OnEssenceChanged(long essence)
        {
            UpdateEssenceDisplay();
        }

        private void OnRankUp(int rank)
        {
            UpdateRankDisplay();
            UpdateEssenceDisplay(); // Capacity may have changed
        }

        private void OnFloorChanged(int floor)
        {
            UpdateFloorDisplay();
        }

        private void OnResetTimerChanged(float timeRemaining)
        {
            UpdateResetTimerDisplay();
        }

        private void OnResetStarted()
        {
            if (ResetDungeonButton != null)
            {
                ResetDungeonButton.Text = "RESETTING...";
                ResetDungeonButton.Disabled = true;
            }
        }

        private void OnResetCompleted()
        {
            if (ResetDungeonButton != null)
            {
                ResetDungeonButton.Text = "Manual Reset";
                ResetDungeonButton.Disabled = false;
            }
        }

        private void UpdateEssenceDisplay()
        {
            if (EssenceLabel != null && _essenceManager != null)
            {
                EssenceLabel.Text = $"Essence: {_essenceManager.CurrentEssence:N0}";
            }
            if (CapacityLabel != null && _essenceManager != null)
            {
                CapacityLabel.Text = $"Capacity: {_essenceManager.EssenceCapacity:N0}";
            }
        }

        private void UpdateRankDisplay()
        {
            if (RankLabel != null && _essenceManager != null)
            {
                RankLabel.Text = $"Dungeon Rank: {_essenceManager.DungeonRank}";
            }
        }

        private void UpdateFloorDisplay()
        {
            if (FloorLabel != null && _builderController != null)
            {
                FloorLabel.Text = $"Floor: {_builderController.CurrentFloor + 1} / {_builderController.MaxFloors}";
            }
            if (FloorUpButton != null && _builderController != null)
            {
                FloorUpButton.Disabled = _builderController.CurrentFloor >= _builderController.MaxFloors - 1;
            }
            if (FloorDownButton != null && _builderController != null)
            {
                FloorDownButton.Disabled = _builderController.CurrentFloor <= 0;
            }
        }

        private void UpdateResetTimerDisplay()
        {
            if (ResetDungeonButton != null && _resetCycle != null)
            {
                float timeRemaining = _resetCycle.TimeUntilReset;
                if (timeRemaining > 0)
                {
                    int minutes = (int)(timeRemaining / 60);
                    int seconds = (int)(timeRemaining % 60);
                    ResetDungeonButton.TooltipText = $"Auto-reset in: {minutes:D2}:{seconds:D2}";
                }
                else
                {
                    ResetDungeonButton.TooltipText = "Ready to reset";
                }
            }
        }

        public override void _ExitTree()
        {
            if (_essenceManager != null)
            {
                _essenceManager.OnEssenceChanged -= OnEssenceChanged;
                _essenceManager.OnRankUp -= OnRankUp;
            }
            if (_builderController != null)
            {
                _builderController.OnFloorChanged -= OnFloorChanged;
            }
            if (_resetCycle != null)
            {
                _resetCycle.OnResetTimerChanged -= OnResetTimerChanged;
                _resetCycle.OnResetStarted -= OnResetStarted;
                _resetCycle.OnResetCompleted -= OnResetCompleted;
            }
        }
    }
}