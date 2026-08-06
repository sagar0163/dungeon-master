using System;
using Godot;

namespace DungeonLord.Scripts.UI
{
    /// <summary>
    /// Crawl Mode HUD: Lord HP bar, compass/minimap, possession trigger, wave info.
    /// </summary>
    [GlobalClass]
    public partial class CrawlHUD : Control
    {
        // UI References
        [Export] public ProgressBar HPBar { get; private set; }
        [Export] public Label HPLabel { get; private set; }
        [Export] public Label LevelLabel { get; private set; }
        [Export] public Label XpLabel { get; private set; }
        [Export] public Label EssenceCarriedLabel { get; private set; }
        [Export] public TextureRect Compass { get; private set; }
        [Export] public Label CompassLabel { get; private set; }
        [Export] public PanelContainer MinimapContainer { get; private set; }
        [Export] public Control MinimapGrid { get; private set; }
        [Export] public Button PossessionButton { get; private set; }
        [Export] public ProgressBar PossessionCooldownBar { get; private set; }
        [Export] public Label PossessionTimerLabel { get; private set; }
        [Export] public Label WaveLabel { get; private set; }
        [Export] public Label ReputationLabel { get; private set; }
        [Export] public Label PositionLabel { get; private set; }
        [Export] public Button SwitchModeButton { get; private set; }
        [Export] public VBoxContainer PossessionTargetsPanel { get; private set; }
        [Export] public ScrollContainer PossessionTargetsScroll { get; private set; }
        [Export] public Label FacingLabel { get; private set; }

        // State
        private CrawlController _crawlController;
        private PossessionManager _possessionManager;
        private InvaderAI _invaderAI;
        private DungeonGrid _dungeonGrid;

        // Lord stats (would come from LordState in full implementation)
        private int _lordCurrentHP = 100;
        private int _lordMaxHP = 100;
        private int _lordLevel = 1;
        private long _lordXp = 0;
        private long _lordEssenceCarried = 0;
        private int _currentWave = 1;

        public override void _Ready()
        {
            // Auto-find UI elements if not assigned in editor
            HPBar ??= GetNodeOrNull<ProgressBar>("%HPBar");
            HPLabel ??= GetNodeOrNull<Label>("%HPLabel");
            LevelLabel ??= GetNodeOrNull<Label>("%LevelLabel");
            XpLabel ??= GetNodeOrNull<Label>("%XpLabel");
            EssenceCarriedLabel ??= GetNodeOrNull<Label>("%EssenceCarriedLabel");
            Compass ??= GetNodeOrNull<TextureRect>("%Compass");
            CompassLabel ??= GetNodeOrNull<Label>("%CompassLabel");
            MinimapContainer ??= GetNodeOrNull<PanelContainer>("%MinimapContainer");
            MinimapGrid ??= GetNodeOrNull<Control>("%MinimapGrid");
            PossessionButton ??= GetNodeOrNull<Button>("%PossessionButton");
            PossessionCooldownBar ??= GetNodeOrNull<ProgressBar>("%PossessionCooldownBar");
            PossessionTimerLabel ??= GetNodeOrNull<Label>("%PossessionTimerLabel");
            WaveLabel ??= GetNodeOrNull<Label>("%WaveLabel");
            ReputationLabel ??= GetNodeOrNull<Label>("%ReputationLabel");
            PositionLabel ??= GetNodeOrNull<Label>("%PositionLabel");
            SwitchModeButton ??= GetNodeOrNull<Button>("%SwitchModeButton");
            PossessionTargetsPanel ??= GetNodeOrNull<VBoxContainer>("%PossessionTargetsPanel");
            PossessionTargetsScroll ??= GetNodeOrNull<ScrollContainer>("%PossessionTargetsScroll");
            FacingLabel ??= GetNodeOrNull<Label>("%FacingLabel");

            // Connect buttons
            PossessionButton?.Pressed += OnPossessionButtonPressed;
            SwitchModeButton?.Pressed += () => _crawlController?.OnModeSwitchRequested?.Invoke();

            // Hide possession targets panel initially
            if (PossessionTargetsScroll != null)
                PossessionTargetsScroll.Visible = false;

            GD.Print("CrawlHUD initialized");
        }

        public void Initialize(CrawlController crawlController, PossessionManager possessionManager, InvaderAI invaderAI, DungeonGrid dungeonGrid)
        {
            _crawlController = crawlController;
            _possessionManager = possessionManager;
            _invaderAI = invaderAI;
            _dungeonGrid = dungeonGrid;

            // Subscribe to events
            _crawlController.OnPositionChanged += OnPositionChanged;
            _possessionManager.OnPossessionStarted += OnPossessionStarted;
            _possessionManager.OnPossessionEnded += OnPossessionEnded;
            _possessionManager.OnPossessionTimerChanged += OnPossessionTimerChanged;
            _invaderAI.OnPartySpawned += OnPartySpawned;
            _invaderAI.OnPartyDestroyed += OnPartyDestroyed;

            // Initial update
            UpdateHPDisplay();
            UpdateLevelDisplay();
            UpdateCompass();
            UpdatePositionDisplay();
            UpdateWaveDisplay();
            UpdateReputationDisplay();
            UpdatePossessionButton();
        }

        private void OnPositionChanged(Vector3I position, CrawlController.Direction facing)
        {
            UpdatePositionDisplay();
            UpdateCompass();
            UpdateMinimap();
            UpdatePossessionButton();
        }

        private void OnPossessionStarted(string monsterId, Vector3I position)
        {
            UpdatePossessionButton();
            UpdatePossessionTimerDisplay();
            HidePossessionTargets();
        }

        private void OnPossessionEnded(string monsterId)
        {
            UpdatePossessionButton();
        }

        private void OnPossessionTimerChanged(float remainingTime)
        {
            UpdatePossessionTimerDisplay();
        }

        private void OnPartySpawned(InvaderAI.InvaderParty party)
        {
            UpdateWaveDisplay();
        }

        private void OnPartyDestroyed(InvaderAI.InvaderParty party)
        {
            // Could add essence gain notification here
        }

        private void OnPossessionButtonPressed()
        {
            if (_possessionManager.IsPossessing)
            {
                _possessionManager.EndPossession();
            }
            else if (_possessionManager.CanPossess)
            {
                ShowPossessionTargets();
            }
            else
            {
                // Show cooldown feedback
                GD.Print($"Possession on cooldown: {_possessionManager.CooldownRemaining:F1}s");
            }
        }

        private void ShowPossessionTargets()
        {
            if (PossessionTargetsPanel == null || _possessionManager == null) return;

            // Clear existing targets
            foreach (Node child in PossessionTargetsPanel.GetChildren())
            {
                child.QueueFree();
            }

            var targets = _possessionManager.GetPossessableTargets();
            if (targets.Count == 0)
            {
                var noTargetsLabel = new Label { Text = "No possessable monsters in range" };
                PossessionTargetsPanel.AddChild(noTargetsLabel);
            }
            else
            {
                foreach (var (monsterId, position) in targets)
                {
                    var btn = new Button
                    {
                        Text = $"{monsterId} at ({position.X}, {position.Y}, F{position.Z})",
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    btn.Pressed += () => _possessionManager.TryPossess(position);
                    PossessionTargetsPanel.AddChild(btn);
                }
            }

            if (PossessionTargetsScroll != null)
                PossessionTargetsScroll.Visible = true;
        }

        private void HidePossessionTargets()
        {
            if (PossessionTargetsScroll != null)
                PossessionTargetsScroll.Visible = false;
        }

        private void UpdateHPDisplay()
        {
            if (HPBar != null)
            {
                HPBar.MaxValue = _lordMaxHP;
                HPBar.Value = _lordCurrentHP;
            }
            if (HPLabel != null)
            {
                HPLabel.Text = $"{_lordCurrentHP} / {_lordMaxHP}";
            }
        }

        private void UpdateLevelDisplay()
        {
            if (LevelLabel != null)
            {
                LevelLabel.Text = $"Level: {_lordLevel}";
            }
            if (XpLabel != null)
            {
                long xpForNext = CalculateXpForLevel(_lordLevel + 1);
                XpLabel.Text = $"XP: {_lordXp:N0} / {xpForNext:N0}";
            }
            if (EssenceCarriedLabel != null)
            {
                EssenceCarriedLabel.Text = $"Essence: {_lordEssenceCarried:N0}";
            }
        }

        private void UpdateCompass()
        {
            if (CompassLabel != null && _crawlController != null)
            {
                CompassLabel.Text = _crawlController.Facing.ToString();
            }
            if (FacingLabel != null && _crawlController != null)
            {
                FacingLabel.Text = $"Facing: {_crawlController.Facing}";
            }
            // Could rotate compass texture here based on facing
        }

        private void UpdatePositionDisplay()
        {
            if (PositionLabel != null && _crawlController != null)
            {
                var pos = _crawlController.GridPosition;
                PositionLabel.Text = $"Pos: ({pos.X}, {pos.Y}, F{pos.Z})";
            }
        }

        private void UpdateWaveDisplay()
        {
            if (WaveLabel != null && _invaderAI != null)
            {
                int activeParties = _invaderAI.ActiveParties.Count;
                WaveLabel.Text = $"Wave: {_currentWave} | Active Parties: {activeParties}";
            }
        }

        private void UpdateReputationDisplay()
        {
            if (ReputationLabel != null && _invaderAI != null)
            {
                ReputationLabel.Text = $"Reputation: {_invaderAI.SettlementReputation:F1}";
            }
        }

        private void UpdateMinimap()
        {
            if (MinimapGrid == null || _dungeonGrid == null || _crawlController == null) return;

            // Clear existing minimap
            foreach (Node child in MinimapGrid.GetChildren())
            {
                child.QueueFree();
            }

            // Create a small grid representation around the player
            int range = 5; // 5 tiles in each direction
            var gridContainer = new GridContainer
            {
                Columns = range * 2 + 1
            };

            var playerPos = _crawlController.GridPosition;
            int currentFloor = playerPos.Z;

            for (int y = -range; y <= range; y++)
            {
                for (int x = -range; x <= range; x++)
                {
                    int worldX = playerPos.X + x;
                    int worldY = playerPos.Y + y;

                    var tileData = _dungeonGrid.GetTile(worldX, worldY, currentFloor);
                    var panel = new Panel
                    {
                        CustomMinimumSize = new Vector2(12, 12)
                    };

                    if (x == 0 && y == 0)
                    {
                        // Player position
                        panel.AddThemeColorOverride("panel_color", Colors.Yellow);
                    }
                    else if (tileData != null)
                    {
                        // Color based on tile type
                        panel.AddThemeColorOverride("panel_color", tileData.Type switch
                        {
                            DungeonGrid.TileType.Empty => Colors.Transparent,
                            DungeonGrid.TileType.Corridor => new Color(0.6f, 0.5f, 0.4f),
                            DungeonGrid.TileType.Room => new Color(0.4f, 0.6f, 0.4f),
                            DungeonGrid.TileType.Trap => new Color(0.8f, 0.2f, 0.2f),
                            DungeonGrid.TileType.SpawnPoint => new Color(0.2f, 0.8f, 0.2f),
                            DungeonGrid.TileType.LordChamber => new Color(0.8f, 0.8f, 0.2f),
                            _ => Colors.Gray
                        });
                    }
                    else
                    {
                        panel.AddThemeColorOverride("panel_color", Colors.Transparent);
                    }

                    // Check for invaders on this tile
                    if (tileData != null && _invaderAI != null)
                    {
                        foreach (var party in _invaderAI.ActiveParties)
                        {
                            if (party.CurrentPathIndex < party.Path.Count)
                            {
                                var invaderPos = party.Path[party.CurrentPathIndex];
                                if (invaderPos.X == worldX && invaderPos.Y == worldY && invaderPos.Z == currentFloor)
                                {
                                    panel.AddThemeColorOverride("panel_color", Colors.Red);
                                    break;
                                }
                            }
                        }
                    }

                    gridContainer.AddChild(panel);
                }
            }

            MinimapGrid.AddChild(gridContainer);
        }

        private void UpdatePossessionButton()
        {
            if (PossessionButton == null || _possessionManager == null) return;

            if (_possessionManager.IsPossessing)
            {
                PossessionButton.Text = "End Possession";
                PossessionButton.Disabled = false;
                PossessionButton.TooltipText = $"Possessing: {_possessionManager.PossessedMonsterId}\nTime left: {_possessionManager.RemainingTime:F1}s";
            }
            else if (_possessionManager.CanPossess)
            {
                PossessionButton.Text = "Possess Monster";
                PossessionButton.Disabled = false;
                PossessionButton.TooltipText = "Click to select a monster to possess (requires line of sight)";
            }
            else
            {
                PossessionButton.Text = $"Possession ({_possessionManager.CooldownRemaining:F0}s)";
                PossessionButton.Disabled = true;
                PossessionButton.TooltipText = $"On cooldown: {_possessionManager.CooldownRemaining:F1}s remaining";
            }
        }

        private void UpdatePossessionTimerDisplay()
        {
            if (PossessionCooldownBar != null && _possessionManager != null)
            {
                if (_possessionManager.IsPossessing)
                {
                    PossessionCooldownBar.MaxValue = _possessionManager.MaxPossessionDuration;
                    PossessionCooldownBar.Value = _possessionManager.MaxPossessionDuration - _possessionManager.RemainingTime;
                    PossessionCooldownBar.Visible = true;
                }
                else
                {
                    PossessionCooldownBar.Visible = false;
                }
            }

            if (PossessionTimerLabel != null && _possessionManager != null)
            {
                if (_possessionManager.IsPossessing)
                {
                    PossessionTimerLabel.Text = $"{_possessionManager.RemainingTime:F1}s";
                    PossessionTimerLabel.Visible = true;
                }
                else if (_possessionManager.CooldownRemaining > 0)
                {
                    PossessionTimerLabel.Text = $"CD: {_possessionManager.CooldownRemaining:F1}s";
                    PossessionTimerLabel.Visible = true;
                }
                else
                {
                    PossessionTimerLabel.Visible = false;
                }
            }
        }

        public void SetLordStats(int currentHP, int maxHP, int level, long xp, long essenceCarried)
        {
            _lordCurrentHP = currentHP;
            _lordMaxHP = maxHP;
            _lordLevel = level;
            _lordXp = xp;
            _lordEssenceCarried = essenceCarried;

            UpdateHPDisplay();
            UpdateLevelDisplay();
        }

        public void SetCurrentWave(int wave)
        {
            _currentWave = wave;
            UpdateWaveDisplay();
        }

        private long CalculateXpForLevel(int level)
        {
            // Simple XP curve: 100 * level^2
            return 100L * level * level;
        }

        public override void _ExitTree()
        {
            if (_crawlController != null)
            {
                _crawlController.OnPositionChanged -= OnPositionChanged;
            }
            if (_possessionManager != null)
            {
                _possessionManager.OnPossessionStarted -= OnPossessionStarted;
                _possessionManager.OnPossessionEnded -= OnPossessionEnded;
                _possessionManager.OnPossessionTimerChanged -= OnPossessionTimerChanged;
            }
            if (_invaderAI != null)
            {
                _invaderAI.OnPartySpawned -= OnPartySpawned;
                _invaderAI.OnPartyDestroyed -= OnPartyDestroyed;
            }
        }
    }
}