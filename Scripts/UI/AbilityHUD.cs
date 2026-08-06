using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DungeonLord.Scripts.UI
{
    /// <summary>
    /// Ability HUD for Crawl Mode: hotbar action slots with cooldown timers, essence costs, and keybind display.
    /// Integrates with AbilityDatabase for ability data and cooldown management.
    /// </summary>
    [GlobalClass]
    public partial class AbilityHUD : Control
    {
        // UI References - Hotbar slots (8 slots: Q, W, E, R, 1-4)
        [Export] public GridContainer HotbarGrid { get; private set; }
        [Export] public VBoxContainer CooldownOverlayContainer { get; private set; }
        [Export] public Label AbilityTooltipLabel { get; private set; }
        [Export] public PanelContainer TooltipPanel { get; private set; }
        [Export] public Label EssenceLabel { get; private set; }
        [Export] public ProgressBar GlobalCooldownBar { get; private set; }

        // Configuration
        private const int HotbarSlotCount = 8;
        private readonly string[] DefaultKeybinds = { "Q", "W", "E", "R", "1", "2", "3", "4" };
        private readonly string[] DefaultAbilityIds = { "basic_attack", "essence_strike", "shadow_step", "dungeon_roar", "demonic_shield", "monster_possession", "", "" };

        // State
        private readonly AbilitySlot[] _hotbarSlots = new AbilitySlot[HotbarSlotCount];
        private int _lordLevel = 1;
        private int _dungeonRank = 1;
        private long _currentEssence = 0;
        private string _hoveredAbilityId = "";
        private bool _isInitialized = false;

        public override void _Ready()
        {
            // Auto-find UI elements
            HotbarGrid ??= GetNodeOrNull<GridContainer>("%HotbarGrid");
            CooldownOverlayContainer ??= GetNodeOrNull<VBoxContainer>("%CooldownOverlayContainer");
            AbilityTooltipLabel ??= GetNodeOrNull<Label>("%AbilityTooltipLabel");
            TooltipPanel ??= GetNodeOrNull<PanelContainer>("%TooltipPanel");
            EssenceLabel ??= GetNodeOrNull<Label>("%EssenceLabel");
            GlobalCooldownBar ??= GetNodeOrNull<ProgressBar>("%GlobalCooldownBar");

            // Build hotbar if not already built in editor
            if (HotbarGrid != null && HotbarGrid.GetChildCount() == 0)
            {
                BuildHotbar();
            }

            // Subscribe to AbilityDatabase events
            AbilityDatabase.OnCooldownStarted += OnAbilityCooldownStarted;
            AbilityDatabase.OnCooldownEnded += OnAbilityCooldownEnded;
            AbilityDatabase.OnAbilityUsed += OnAbilityUsed;

            // Hide tooltip initially
            if (TooltipPanel != null) TooltipPanel.Visible = false;

            GD.Print("AbilityHUD initialized");
        }

        private void BuildHotbar()
        {
            HotbarGrid.Columns = HotbarSlotCount;

            for (int i = 0; i < HotbarSlotCount; i++)
            {
                var slotContainer = new VBoxContainer
                {
                    CustomMinimumSize = new Vector2(64, 80)
                };

                // Keybind label (top)
                var keybindLabel = new Label
                {
                    Text = DefaultKeybinds[i],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    CustomMinimumSize = new Vector2(0, 18),
                    ThemeFontSizeOverride = 12
                };
                keybindLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));

                // Ability button (main slot)
                var abilityButton = new Button
                {
                    CustomMinimumSize = new Vector2(56, 56),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FocusMode = Control.FocusModeEnum.None
                };
                abilityButton.Pressed += () => OnHotbarSlotPressed(i);
                abilityButton.MouseEntered += () => OnHotbarSlotHovered(i, true);
                abilityButton.MouseExited += () => OnHotbarSlotHovered(i, false);

                // Cooldown overlay (progress bar on top of button)
                var cooldownOverlay = new ProgressBar
                {
                    MinValue = 0,
                    MaxValue = 1,
                    Value = 0,
                    CustomMinimumSize = new Vector2(56, 56),
                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                    SizeFlagsVertical = Control.SizeFlags.ExpandFill
                };
                cooldownOverlay.AddThemeStyleboxOverride("progress", new StyleBoxFlat
                {
                    BgColor = new Color(0, 0, 0, 0.7f),
                    BorderWidthTop = 2,
                    BorderWidthBottom = 2,
                    BorderWidthLeft = 2,
                    BorderWidthRight = 2,
                    BorderColor = new Color(1, 1, 1, 0.3f)
                });
                cooldownOverlay.Visible = false;

                // Essence cost label (bottom)
                var essenceCostLabel = new Label
                {
                    Text = "",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    CustomMinimumSize = new Vector2(0, 16),
                    ThemeFontSizeOverride = 10
                };
                essenceCostLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.8f, 1f));

                // Stack: keybind -> button (with cooldown overlay) -> essence cost
                slotContainer.AddChild(keybindLabel);

                var buttonContainer = new Control
                {
                    CustomMinimumSize = new Vector2(56, 56),
                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                    SizeFlagsVertical = Control.SizeFlags.ExpandFill
                };
                buttonContainer.AddChild(abilityButton);
                buttonContainer.AddChild(cooldownOverlay);
                slotContainer.AddChild(buttonContainer);

                slotContainer.AddChild(essenceCostLabel);

                _hotbarSlots[i] = new AbilitySlot
                {
                    Index = i,
                    Container = slotContainer,
                    KeybindLabel = keybindLabel,
                    AbilityButton = abilityButton,
                    CooldownOverlay = cooldownOverlay,
                    EssenceCostLabel = essenceCostLabel,
                    AssignedAbilityId = i < DefaultAbilityIds.Length ? DefaultAbilityIds[i] : ""
                };

                HotbarGrid.AddChild(slotContainer);
            }

            RefreshHotbar();
        }

        public void Initialize(int lordLevel, int dungeonRank, long currentEssence)
        {
            _lordLevel = lordLevel;
            _dungeonRank = dungeonRank;
            _currentEssence = currentEssence;

            // Ensure AbilityDatabase is loaded
            AbilityDatabase.LoadFromJson();

            RefreshHotbar();
            UpdateEssenceDisplay();
            _isInitialized = true;
        }

        public void SetLordStats(int lordLevel, int dungeonRank, long currentEssence)
        {
            _lordLevel = lordLevel;
            _dungeonRank = dungeonRank;
            _currentEssence = currentEssence;

            RefreshHotbar();
            UpdateEssenceDisplay();
        }

        public void UpdateEssence(long essence)
        {
            _currentEssence = essence;
            UpdateEssenceDisplay();
            RefreshHotbarAffordability();
        }

        public override void _Process(double delta)
        {
            if (!_isInitialized) return;

            // Update cooldown overlays
            float deltaTime = (float)delta;
            AbilityDatabase.UpdateCooldowns(deltaTime);

            foreach (var slot in _hotbarSlots)
            {
                if (slot == null || string.IsNullOrEmpty(slot.AssignedAbilityId)) continue;

                float cooldownRemaining = AbilityDatabase.GetCooldownRemaining(slot.AssignedAbilityId);
                var ability = AbilityDatabase.GetAbility(slot.AssignedAbilityId);

                if (ability != null && ability.Cooldown > 0)
                {
                    float progress = 1f - (cooldownRemaining / ability.GetEffectiveCooldown(_dungeonRank));
                    slot.CooldownOverlay.Value = Math.Clamp(progress, 0f, 1f);
                    slot.CooldownOverlay.Visible = cooldownRemaining > 0f;
                }
                else
                {
                    slot.CooldownOverlay.Visible = false;
                }
            }

            // Update global cooldown bar if any ability on cooldown
            UpdateGlobalCooldown();
        }

        private void RefreshHotbar()
        {
            foreach (var slot in _hotbarSlots)
            {
                if (slot == null) continue;

                if (!string.IsNullOrEmpty(slot.AssignedAbilityId))
                {
                    var ability = AbilityDatabase.GetAbility(slot.AssignedAbilityId);
                    if (ability != null)
                    {
                        // Set button icon (using text for now - would use TextureRect with icon in production)
                        slot.AbilityButton.Text = ability.Name.Length > 10 ? ability.Name.Substring(0, 10) : ability.Name;
                        slot.AbilityButton.TooltipText = ability.Description;

                        // Update essence cost label
                        slot.EssenceCostLabel.Text = ability.EssenceCost > 0 ? $"{ability.EssenceCost}⚡" : "";
                        slot.EssenceCostLabel.Visible = ability.EssenceCost > 0;

                        // Check affordability
                        bool canAfford = _currentEssence >= ability.EssenceCost;
                        bool unlocked = _lordLevel >= ability.UnlockLevel && _dungeonRank >= ability.RequiredDungeonRank;

                        slot.AbilityButton.Disabled = !unlocked || !canAfford;
                        slot.AbilityButton.AddThemeColorOverride("font_color",
                            !unlocked ? Colors.DarkGray : (!canAfford ? new Color(1f, 0.4f, 0.4f) : Colors.White));
                    }
                }
                else
                {
                    slot.AbilityButton.Text = "[Empty]";
                    slot.AbilityButton.Disabled = true;
                    slot.EssenceCostLabel.Text = "";
                    slot.CooldownOverlay.Visible = false;
                }
            }
        }

        private void RefreshHotbarAffordability()
        {
            foreach (var slot in _hotbarSlots)
            {
                if (slot == null || string.IsNullOrEmpty(slot.AssignedAbilityId)) continue;

                var ability = AbilityDatabase.GetAbility(slot.AssignedAbilityId);
                if (ability != null)
                {
                    bool canAfford = _currentEssence >= ability.EssenceCost;
                    bool unlocked = _lordLevel >= ability.UnlockLevel && _dungeonRank >= ability.RequiredDungeonRank;
                    bool onCooldown = ability.IsOnCooldown;

                    slot.AbilityButton.Disabled = !unlocked || !canAfford || onCooldown;

                    if (!unlocked)
                        slot.AbilityButton.AddThemeColorOverride("font_color", Colors.DarkGray);
                    else if (!canAfford)
                        slot.AbilityButton.AddThemeColorOverride("font_color", new Color(1f, 0.4f, 0.4f));
                    else if (onCooldown)
                        slot.AbilityButton.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 1f));
                    else
                        slot.AbilityButton.AddThemeColorOverride("font_color", Colors.White);
                }
            }
        }

        private void UpdateEssenceDisplay()
        {
            if (EssenceLabel != null)
            {
                EssenceLabel.Text = $"Essence: {_currentEssence:N0}";
            }
        }

        private void UpdateGlobalCooldown()
        {
            if (GlobalCooldownBar == null) return;

            // Find the ability with the longest remaining cooldown
            float maxCooldownRemaining = 0f;
            float maxCooldownTotal = 0f;

            foreach (var slot in _hotbarSlots)
            {
                if (slot == null || string.IsNullOrEmpty(slot.AssignedAbilityId)) continue;

                var ability = AbilityDatabase.GetAbility(slot.AssignedAbilityId);
                if (ability != null && ability.IsOnCooldown)
                {
                    float remaining = ability.CurrentCooldown;
                    float total = ability.GetEffectiveCooldown(_dungeonRank);
                    if (remaining > maxCooldownRemaining)
                    {
                        maxCooldownRemaining = remaining;
                        maxCooldownTotal = total;
                    }
                }
            }

            if (maxCooldownRemaining > 0 && maxCooldownTotal > 0)
            {
                GlobalCooldownBar.MaxValue = maxCooldownTotal;
                GlobalCooldownBar.Value = maxCooldownTotal - maxCooldownRemaining;
                GlobalCooldownBar.Visible = true;
            }
            else
            {
                GlobalCooldownBar.Visible = false;
            }
        }

        private void OnHotbarSlotPressed(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _hotbarSlots.Length) return;

            var slot = _hotbarSlots[slotIndex];
            if (slot == null || string.IsNullOrEmpty(slot.AssignedAbilityId)) return;

            var ability = AbilityDatabase.GetAbility(slot.AssignedAbilityId);
            if (ability == null) return;

            // Check requirements
            if (_lordLevel < ability.UnlockLevel)
            {
                GD.Print($"Ability {ability.Name} requires Lord Level {ability.UnlockLevel}");
                return;
            }
            if (_dungeonRank < ability.RequiredDungeonRank)
            {
                GD.Print($"Ability {ability.Name} requires Dungeon Rank {ability.RequiredDungeonRank}");
                return;
            }
            if (_currentEssence < ability.EssenceCost)
            {
                GD.Print($"Not enough Essence for {ability.Name} (need {ability.EssenceCost}, have {_currentEssence})");
                return;
            }
            if (ability.IsOnCooldown)
            {
                GD.Print($"Ability {ability.Name} on cooldown ({ability.CurrentCooldown:F1}s)");
                return;
            }

            // Try to use ability - returns true if successful
            bool success = AbilityDatabase.TryUseAbility(slot.AssignedAbilityId, _lordLevel, _dungeonRank, ref _currentEssence);

            if (success)
            {
                UpdateEssenceDisplay();
                RefreshHotbarAffordability();
                GD.Print($"Activated {ability.Name} from slot {slotIndex} ({slot.KeybindLabel.Text})");
            }
        }

        private void OnHotbarSlotHovered(int slotIndex, bool entered)
        {
            if (slotIndex < 0 || slotIndex >= _hotbarSlots.Length) return;

            var slot = _hotbarSlots[slotIndex];
            if (slot == null || string.IsNullOrEmpty(slot.AssignedAbilityId))
            {
                if (TooltipPanel != null) TooltipPanel.Visible = false;
                return;
            }

            if (entered)
            {
                var ability = AbilityDatabase.GetAbility(slot.AssignedAbilityId);
                if (ability != null && AbilityTooltipLabel != null && TooltipPanel != null)
                {
                    string tooltip = $"{ability.Name}\n{ability.Description}\n\n";
                    tooltip += $"Cooldown: {ability.GetEffectiveCooldown(_dungeonRank):F1}s";
                    if (ability.EssenceCost > 0) tooltip += $" | Essence: {ability.EssenceCost}";
                    tooltip += $"\nRange: {ability.Range} tiles";
                    tooltip += $"\nUnlock: Level {ability.UnlockLevel}, Rank {ability.RequiredDungeonRank}";

                    if (ability.SpecialEffects.Count > 0)
                    {
                        tooltip += $"\nEffects: {string.Join(", ", ability.SpecialEffects)}";
                    }

                    AbilityTooltipLabel.Text = tooltip;
                    TooltipPanel.Visible = true;

                    // Position tooltip near mouse (simplified - would use actual mouse position)
                    TooltipPanel.GlobalPosition = slot.AbilityButton.GetGlobalRect().End + new Vector2(10, -TooltipPanel.GetRect().Size.Y - 10);
                }
            }
            else
            {
                if (TooltipPanel != null) TooltipPanel.Visible = false;
            }
        }

        private void OnAbilityCooldownStarted(string abilityId, float cooldown)
        {
            // Cooldown overlay updated in _Process
            RefreshHotbarAffordability();
        }

        private void OnAbilityCooldownEnded(string abilityId)
        {
            RefreshHotbarAffordability();
        }

        private void OnAbilityUsed(string abilityId)
        {
            // Could add visual feedback here (screen shake, particle effect, etc.)
            var ability = AbilityDatabase.GetAbility(abilityId);
            if (ability != null)
            {
                GD.Print($"[AbilityHUD] {ability.Name} activated!");
            }
        }

        // Public API for assigning abilities to slots (for future customization)
        public bool AssignAbilityToSlot(int slotIndex, string abilityId)
        {
            if (slotIndex < 0 || slotIndex >= HotbarSlotCount) return false;

            var ability = AbilityDatabase.GetAbility(abilityId);
            if (ability == null) return false;

            _hotbarSlots[slotIndex].AssignedAbilityId = abilityId;
            RefreshHotbar();
            return true;
        }

        public string GetAbilityAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= HotbarSlotCount) return "";
            return _hotbarSlots[slotIndex]?.AssignedAbilityId ?? "";
        }

        public void SetKeybind(int slotIndex, string keybind)
        {
            if (slotIndex < 0 || slotIndex >= HotbarSlotCount) return;
            if (_hotbarSlots[slotIndex]?.KeybindLabel != null)
            {
                _hotbarSlots[slotIndex].KeybindLabel.Text = keybind;
            }
        }

        public override void _ExitTree()
        {
            AbilityDatabase.OnCooldownStarted -= OnAbilityCooldownStarted;
            AbilityDatabase.OnCooldownEnded -= OnAbilityCooldownEnded;
            AbilityDatabase.OnAbilityUsed -= OnAbilityUsed;
        }

        // Internal slot data class
        private class AbilitySlot
        {
            public int Index { get; set; }
            public VBoxContainer Container { get; set; }
            public Label KeybindLabel { get; set; }
            public Button AbilityButton { get; set; }
            public ProgressBar CooldownOverlay { get; set; }
            public Label EssenceCostLabel { get; set; }
            public string AssignedAbilityId { get; set; }
        }
    }
}