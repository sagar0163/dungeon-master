using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DungeonLord.Scripts.UI
{
    /// <summary>
    /// Inventory HUD for managing Dungeon Lord and monster equipment.
    /// Supports equipment slots, item stats comparison, drag-and-drop, and essence/gold costs.
    /// </summary>
    [GlobalClass]
    public partial class InventoryHUD : Control
    {
        // UI References
        [Export] public TabContainer TabContainer { get; private set; }
        [Export] public VBoxContainer LordTab { get; private set; }
        [Export] public VBoxContainer MonsterTab { get; private set; }
        [Export] public GridContainer LordEquipmentGrid { get; private set; }
        [Export] public GridContainer MonsterEquipmentGrid { get; private set; }
        [Export] public VBoxContainer ItemDetailsPanel { get; private set; }
        [Export] public Label ItemNameLabel { get; private set; }
        [Export] public Label ItemCategoryLabel { get; private set; }
        [Export] public Label ItemRarityLabel { get; private set; }
        [Export] public Label ItemTierLabel { get; private set; }
        [Export] public Label ItemStatsLabel { get; private set; }
        [Export] public Label ItemValueLabel { get; private set; }
        [Export] public Label ItemEssenceLabel { get; private set; }
        [Export] public Label ItemRankLabel { get; private set; }
        [Export] public VBoxContainer InventoryList { get; private set; }
        [Export] public ScrollContainer InventoryScroll { get; private set; }
        [Export] public HBoxContainer FilterBar { get; private set; }
        [Export] public Button FilterAllBtn { get; private set; }
        [Export] public Button FilterWeaponsBtn { get; private set; }
        [Export] public Button FilterArmorBtn { get; private set; }
        [Export] public Button FilterAccessoriesBtn { get; private set; }
        [Export] public Button FilterConsumablesBtn { get; private set; }
        [Export] public Label GoldLabel { get; private set; }
        [Export] public Label EssenceLabel { get; private set; }
        [Export] public Button CloseButton { get; private set; }
        [Export] public Button EquipButton { get; private set; }
        [Export] public Button UnequipButton { get; private set; }
        [Export] public Button UseButton { get; private set; }
        [Export] public Button DropButton { get; private set; }

        // Equipment slot definitions for Lord
        private static readonly string[] LordEquipSlots = new[]
        {
            "Weapon", "Chest", "Head", "Feet", "Accessory"
        };

        // Equipment slot definitions for monsters
        private static readonly string[] MonsterEquipSlots = new[]
        {
            "Weapon", "Chest", "Head", "Feet", "Accessory"
        };

        // State
        private ItemDatabase.ItemData _selectedItem;
        private string _selectedSlot;
        private bool _isLordTab = true;
        private string _currentFilter = "All";
        private Dictionary<string, string> _lordEquipment = new();
        private Dictionary<string, string> _monsterEquipment = new();
        private List<string> _inventory = new(); // Item IDs
        private long _gold = 0;
        private long _essence = 0;
        private string _currentMonsterId = "";

        public override void _Ready()
        {
            // Auto-find UI elements
            TabContainer ??= GetNodeOrNull<TabContainer>("%TabContainer");
            LordTab ??= GetNodeOrNull<VBoxContainer>("%LordTab");
            MonsterTab ??= GetNodeOrNull<VBoxContainer>("%MonsterTab");
            LordEquipmentGrid ??= GetNodeOrNull<GridContainer>("%LordEquipmentGrid");
            MonsterEquipmentGrid ??= GetNodeOrNull<GridContainer>("%MonsterEquipmentGrid");
            ItemDetailsPanel ??= GetNodeOrNull<VBoxContainer>("%ItemDetailsPanel");
            ItemNameLabel ??= GetNodeOrNull<Label>("%ItemNameLabel");
            ItemCategoryLabel ??= GetNodeOrNull<Label>("%ItemCategoryLabel");
            ItemRarityLabel ??= GetNodeOrNull<Label>("%ItemRarityLabel");
            ItemTierLabel ??= GetNodeOrNull<Label>("%ItemTierLabel");
            ItemStatsLabel ??= GetNodeOrNull<Label>("%ItemStatsLabel");
            ItemValueLabel ??= GetNodeOrNull<Label>("%ItemValueLabel");
            ItemEssenceLabel ??= GetNodeOrNull<Label>("%ItemEssenceLabel");
            ItemRankLabel ??= GetNodeOrNull<Label>("%ItemRankLabel");
            InventoryList ??= GetNodeOrNull<VBoxContainer>("%InventoryList");
            InventoryScroll ??= GetNodeOrNull<ScrollContainer>("%InventoryScroll");
            FilterBar ??= GetNodeOrNull<HBoxContainer>("%FilterBar");
            FilterAllBtn ??= GetNodeOrNull<Button>("%FilterAllBtn");
            FilterWeaponsBtn ??= GetNodeOrNull<Button>("%FilterWeaponsBtn");
            FilterArmorBtn ??= GetNodeOrNull<Button>("%FilterArmorBtn");
            FilterAccessoriesBtn ??= GetNodeOrNull<Button>("%FilterAccessoriesBtn");
            FilterConsumablesBtn ??= GetNodeOrNull<Button>("%FilterConsumablesBtn");
            GoldLabel ??= GetNodeOrNull<Label>("%GoldLabel");
            EssenceLabel ??= GetNodeOrNull<Label>("%EssenceLabel");
            CloseButton ??= GetNodeOrNull<Button>("%CloseButton");
            EquipButton ??= GetNodeOrNull<Button>("%EquipButton");
            UnequipButton ??= GetNodeOrNull<Button>("%UnequipButton");
            UseButton ??= GetNodeOrNull<Button>("%UseButton");
            DropButton ??= GetNodeOrNull<Button>("%DropButton");

            // Build equipment grids if empty
            if (LordEquipmentGrid != null && LordEquipmentGrid.GetChildCount() == 0)
            {
                BuildEquipmentGrid(LordEquipmentGrid, LordEquipSlots, true);
            }
            if (MonsterEquipmentGrid != null && MonsterEquipmentGrid.GetChildCount() == 0)
            {
                BuildEquipmentGrid(MonsterEquipmentGrid, MonsterEquipSlots, false);
            }

            // Connect filter buttons
            FilterAllBtn?.Pressed += () => SetFilter("All");
            FilterWeaponsBtn?.Pressed += () => SetFilter("Weapon");
            FilterArmorBtn?.Pressed += () => SetFilter("Armor");
            FilterAccessoriesBtn?.Pressed += () => SetFilter("Accessory");
            FilterConsumablesBtn?.Pressed += () => SetFilter("Consumable");

            // Connect action buttons
            EquipButton?.Pressed += OnEquipPressed;
            UnequipButton?.Pressed += OnUnequipPressed;
            UseButton?.Pressed += OnUsePressed;
            DropButton?.Pressed += OnDropPressed;
            CloseButton?.Pressed += () => Visible = false;

            // Connect tab change
            TabContainer?.TabChanged += OnTabChanged;

            // Load items database
            ItemDatabase.LoadFromJson();

            // Initial state
            UpdateFilterButtons();
            RefreshInventoryList();
            UpdateCurrencyDisplay();
            ClearItemDetails();
            SetActionButtonsEnabled(false);
        }

        private void BuildEquipmentGrid(GridContainer grid, string[] slots, bool isLord)
        {
            grid.Columns = 1;
            foreach (var slot in slots)
            {
                var slotContainer = new VBoxContainer();
                
                var slotLabel = new Label
                {
                    Text = slot,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    CustomMinimumSize = new Vector2(0, 24)
                };
                slotLabel.AddThemeFontSizeOverride("font_size", 14);

                var slotButton = new Button
                {
                    Text = "[Empty]",
                    TooltipText = $"Equip item in {slot} slot",
                    CustomMinimumSize = new Vector2(120, 60),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                slotButton.Pressed += () => OnEquipmentSlotPressed(slot, isLord);

                slotContainer.AddChild(slotLabel);
                slotContainer.AddChild(slotButton);
                grid.AddChild(slotContainer);
            }
        }

        private void OnTabChanged(long tabIndex)
        {
            _isLordTab = tabIndex == 0;
            RefreshEquipmentGrid();
            RefreshInventoryList();
            ClearSelection();
        }

        private void OnEquipmentSlotPressed(string slot, bool isLord)
        {
            _selectedSlot = slot;
            var equipment = isLord ? _lordEquipment : _monsterEquipment;
            
            if (equipment.TryGetValue(slot, out var itemId))
            {
                var item = ItemDatabase.GetItem(itemId);
                if (item != null)
                {
                    SelectItem(item);
                    SetActionButtonsEnabled(true, canUnequip: true);
                    return;
                }
            }
            
            // Empty slot - show compatible items
            ClearItemDetails();
            ItemNameLabel.Text = $"{slot} Slot (Empty)";
            ItemCategoryLabel.Text = "Click to equip compatible item";
            RefreshInventoryList();
            SetActionButtonsEnabled(false);
        }

        private void SelectItem(ItemDatabase.ItemData item)
        {
            _selectedItem = item;
            _selectedSlot = null;

            ItemNameLabel.Text = item.Name;
            ItemCategoryLabel.Text = $"{item.Category} - {item.EquipSlot}";
            ItemRarityLabel.Text = $"Rarity: {item.Rarity}";
            ItemRarityLabel.AddThemeColorOverride("font_color", GetRarityColor(item.Rarity));
            ItemTierLabel.Text = $"Tier: {item.Tier}";

            var stats = new List<string>();
            if (item.AttackBonus > 0) stats.Add($"Attack: +{item.AttackBonus}");
            if (item.DefenseBonus > 0) stats.Add($"Defense: +{item.DefenseBonus}");
            if (item.HpBonus > 0) stats.Add($"HP: +{item.HpBonus}");
            ItemStatsLabel.Text = stats.Count > 0 ? string.Join("\n", stats) : "No stat bonuses";

            ItemValueLabel.Text = $"Gold Value: {item.GoldValue:N0}";
            ItemEssenceLabel.Text = $"Essence Cost: {item.EssenceCost:N0}";
            ItemRankLabel.Text = $"Requires Dungeon Rank: {item.RequiredDungeonRank}";
        }

        private Color GetRarityColor(string rarity)
        {
            return rarity switch
            {
                "Common" => Colors.White,
                "Uncommon" => new Color(0.2f, 1f, 0.2f),
                "Rare" => new Color(0.2f, 0.6f, 1f),
                "Epic" => new Color(0.8f, 0.2f, 1f),
                "Legendary" => new Color(1f, 0.6f, 0f),
                "Mythic" => new Color(1f, 0.2f, 0.2f),
                "Divine" => new Color(1f, 1f, 0.2f),
                _ => Colors.White
            };
        }

        private void ClearItemDetails()
        {
            _selectedItem = null;
            ItemNameLabel.Text = "Select an item";
            ItemCategoryLabel.Text = "";
            ItemRarityLabel.Text = "";
            ItemTierLabel.Text = "";
            ItemStatsLabel.Text = "";
            ItemValueLabel.Text = "";
            ItemEssenceLabel.Text = "";
            ItemRankLabel.Text = "";
        }

        private void ClearSelection()
        {
            _selectedItem = null;
            _selectedSlot = null;
            ClearItemDetails();
            SetActionButtonsEnabled(false);
        }

        private void SetFilter(string filter)
        {
            _currentFilter = filter;
            UpdateFilterButtons();
            RefreshInventoryList();
        }

        private void UpdateFilterButtons()
        {
            var buttons = new Dictionary<string, Button>
            {
                ["All"] = FilterAllBtn,
                ["Weapon"] = FilterWeaponsBtn,
                ["Armor"] = FilterArmorBtn,
                ["Accessory"] = FilterAccessoriesBtn,
                ["Consumable"] = FilterConsumablesBtn
            };

            foreach (var kvp in buttons)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.ButtonPressed = kvp.Key == _currentFilter;
                }
            }
        }

        private void RefreshInventoryList()
        {
            if (InventoryList == null) return;

            // Clear existing
            foreach (Node child in InventoryList.GetChildren())
            {
                child.QueueFree();
            }

            // Get filtered items
            var items = GetFilteredItems();
            
            foreach (var item in items)
            {
                var btn = new Button
                {
                    Text = $"{item.Name} (T{item.Tier})",
                    TooltipText = $"{item.Category} | {item.Rarity} | ATK:{item.AttackBonus} DEF:{item.DefenseBonus} HP:{item.HpBonus}",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    CustomMinimumSize = new Vector2(0, 36)
                };
                
                var itemId = item.Id; // Capture for closure
                btn.Pressed += () => SelectItem(item);
                
                // Color by rarity
                btn.AddThemeColorOverride("font_color", GetRarityColor(item.Rarity));
                
                // Highlight if equipped
                bool isEquipped = _lordEquipment.ContainsValue(itemId) || _monsterEquipment.ContainsValue(itemId);
                if (isEquipped)
                {
                    btn.AddThemeColorOverride("font_color", Colors.Yellow);
                    btn.Text += " [EQUIPPED]";
                }
                
                // Highlight if selected
                if (_selectedItem?.Id == itemId)
                {
                    btn.AddThemeColorOverride("font_color", Colors.Cyan);
                }
                
                InventoryList.AddChild(btn);
            }
        }

        private List<ItemDatabase.ItemData> GetFilteredItems()
        {
            var allItems = ItemDatabase.GetAllItems();
            
            return _currentFilter switch
            {
                "All" => allItems,
                "Weapon" => allItems.Where(i => i.Category == "Weapon").ToList(),
                "Armor" => allItems.Where(i => i.Category == "Heavy Armor" || i.Category == "Light Armor" || i.Category == "Clothes").ToList(),
                "Accessory" => allItems.Where(i => i.Category == "Accessory").ToList(),
                "Consumable" => allItems.Where(i => i.Category == "Consumable").ToList(),
                _ => allItems
            };
        }

        private void RefreshEquipmentGrid()
        {
            var grid = _isLordTab ? LordEquipmentGrid : MonsterEquipmentGrid;
            var equipment = _isLordTab ? _lordEquipment : _monsterEquipment;
            
            if (grid == null) return;

            int index = 0;
            foreach (Node child in grid.GetChildren())
            {
                if (child is VBoxContainer slotContainer)
                {
                    var slotName = LordEquipSlots[index]; // Same order for both
                    var slotButton = slotContainer.GetChild(1) as Button;
                    
                    if (equipment.TryGetValue(slotName, out var itemId))
                    {
                        var item = ItemDatabase.GetItem(itemId);
                        if (item != null)
                        {
                            slotButton.Text = item.Name;
                            slotButton.AddThemeColorOverride("font_color", GetRarityColor(item.Rarity));
                            slotButton.TooltipText = $"{item.Name}\nATK:+{item.AttackBonus} DEF:+{item.DefenseBonus} HP:+{item.HpBonus}";
                        }
                    }
                    else
                    {
                        slotButton.Text = "[Empty]";
                        slotButton.AddThemeColorOverride("font_color", Colors.Gray);
                        slotButton.TooltipText = $"Equip item in {slotName} slot";
                    }
                    index++;
                }
            }
        }

        private void OnEquipPressed()
        {
            if (_selectedItem == null || string.IsNullOrEmpty(_selectedSlot)) return;
            
            var equipment = _isLordTab ? _lordEquipment : _monsterEquipment;
            
            // Check if item fits slot
            if (!_selectedItem.EquipSlot.Equals(_selectedSlot, StringComparison.OrdinalIgnoreCase) &&
                !(_selectedItem.Category == "Heavy Armor" && _selectedSlot == "Chest") &&
                !(_selectedItem.Category == "Light Armor" && _selectedSlot == "Chest") &&
                !(_selectedItem.Category == "Clothes" && _selectedSlot == "Chest"))
            {
                GD.Print($"Cannot equip {_selectedItem.Name} in {_selectedSlot} slot");
                return;
            }

            // Check rank requirement (simplified - would check actual dungeon rank)
            // if (DungeonRank < _selectedItem.RequiredDungeonRank) return;

            // Unequip current item if any
            if (equipment.TryGetValue(_selectedSlot, out var currentItemId))
            {
                _inventory.Add(currentItemId);
            }

            // Equip new item
            equipment[_selectedSlot] = _selectedItem.Id;
            _inventory.Remove(_selectedItem.Id);

            // Update monster equipment if monster tab
            if (!_isLordTab && !string.IsNullOrEmpty(_currentMonsterId))
            {
                // Notify monster of equipment change
                GD.Print($"Monster {_currentMonsterId} equipped {_selectedItem.Name} in {_selectedSlot}");
            }

            RefreshEquipmentGrid();
            RefreshInventoryList();
            SelectItem(_selectedItem); // Refresh details
            SetActionButtonsEnabled(true, canUnequip: true);
        }

        private void OnUnequipPressed()
        {
            if (string.IsNullOrEmpty(_selectedSlot)) return;
            
            var equipment = _isLordTab ? _lordEquipment : _monsterEquipment;
            
            if (equipment.TryGetValue(_selectedSlot, out var itemId))
            {
                _inventory.Add(itemId);
                equipment.Remove(_selectedSlot);
                
                RefreshEquipmentGrid();
                RefreshInventoryList();
                ClearSelection();
            }
        }

        private void OnUsePressed()
        {
            if (_selectedItem == null || _selectedItem.Category != "Consumable") return;
            
            GD.Print($"Using {_selectedItem.Name}: Restores {_selectedItem.HpBonus} HP");
            
            // Apply consumable effect (would integrate with Lord/Monster stats)
            _inventory.Remove(_selectedItem.Id);
            _selectedItem = null;
            
            RefreshInventoryList();
            ClearItemDetails();
            SetActionButtonsEnabled(false);
        }

        private void OnDropPressed()
        {
            if (_selectedItem == null) return;
            
            _inventory.Remove(_selectedItem.Id);
            _gold += _selectedItem.GoldValue; // Sell for gold
            
            GD.Print($"Dropped/sold {_selectedItem.Name} for {_selectedItem.GoldValue} gold");
            
            RefreshInventoryList();
            UpdateCurrencyDisplay();
            ClearItemDetails();
            SetActionButtonsEnabled(false);
        }

        private void SetActionButtonsEnabled(bool enabled, bool canUnequip = false, bool canUse = false)
        {
            EquipButton?.SetDisabled(!enabled);
            UnequipButton?.SetDisabled(!canUnequip);
            UseButton?.SetDisabled(!canUse);
            DropButton?.SetDisabled(!enabled);
        }

        private void UpdateCurrencyDisplay()
        {
            if (GoldLabel != null) GoldLabel.Text = $"Gold: {_gold:N0}";
            if (EssenceLabel != null) EssenceLabel.Text = $"Essence: {_essence:N0}";
        }

        // Public API for external systems
        public void SetLordEquipment(Dictionary<string, string> equipment)
        {
            _lordEquipment = equipment ?? new Dictionary<string, string>();
            RefreshEquipmentGrid();
            RefreshInventoryList();
        }

        public void SetMonsterEquipment(string monsterId, Dictionary<string, string> equipment)
        {
            _currentMonsterId = monsterId;
            _monsterEquipment = equipment ?? new Dictionary<string, string>();
            RefreshEquipmentGrid();
            RefreshInventoryList();
        }

        public void AddItemToInventory(string itemId, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                _inventory.Add(itemId);
            }
            RefreshInventoryList();
        }

        public void RemoveItemFromInventory(string itemId, int count = 1)
        {
            for (int i = 0; i < count && _inventory.Remove(itemId); i++) { }
            RefreshInventoryList();
        }

        public void SetCurrency(long gold, long essence)
        {
            _gold = gold;
            _essence = essence;
            UpdateCurrencyDisplay();
        }

        public Dictionary<string, string> GetLordEquipment() => new(_lordEquipment);
        public Dictionary<string, string> GetMonsterEquipment() => new(_monsterEquipment);
        public List<string> GetInventory() => new(_inventory);

        public ItemDatabase.ItemData GetSelectedItem() => _selectedItem;

        public void OpenInventory(bool forLord = true)
        {
            Visible = true;
            if (TabContainer != null)
            {
                TabContainer.CurrentTab = forLord ? 0 : 1;
            }
        }

        public void CloseInventory()
        {
            Visible = false;
            ClearSelection();
        }

        public override void _ExitTree()
        {
            if (TabContainer != null)
                TabContainer.TabChanged -= OnTabChanged;
        }
    }
}