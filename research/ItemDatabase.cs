using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace DungeonLord.Scripts
{
    public class ItemData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        
        [JsonPropertyName("equip_slot")]
        public string EquipSlot { get; set; }
        
        public int Tier { get; set; }
        public string Rarity { get; set; }
        
        [JsonPropertyName("attack_bonus")]
        public int AttackBonus { get; set; }
        
        [JsonPropertyName("defense_bonus")]
        public int DefenseBonus { get; set; }
        
        [JsonPropertyName("hp_bonus")]
        public int HpBonus { get; set; }
        
        [JsonPropertyName("gold_value")]
        public long GoldValue { get; set; }
        
        [JsonPropertyName("essence_cost")]
        public long EssenceCost { get; set; }
        
        [JsonPropertyName("required_dungeon_rank")]
        public int RequiredDungeonRank { get; set; }
    }

    public static class ItemDatabase
    {
        private static readonly Dictionary<string, ItemData> _items = new();
        private static bool _loaded = false;

        public static void LoadFromJson(string path = "res://data/items.json")
        {
            if (_loaded) return;

            try
            {
                string jsonPath = ProjectSettings.GlobalizePath(path);
                if (!File.Exists(jsonPath))
                {
                    GD.PrintErr($"Item database not found at {jsonPath}");
                    LoadDefaultItems();
                    return;
                }

                string json = File.ReadAllText(jsonPath);
                var items = JsonSerializer.Deserialize<List<ItemData>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (items != null)
                {
                    _items.Clear();
                    foreach (var item in items)
                    {
                        _items[item.Id] = item;
                    }
                    GD.Print($"Loaded {_items.Count} items from {path}");
                }
                else
                {
                    GD.PrintErr("Failed to deserialize items.json");
                    LoadDefaultItems();
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error loading items.json: {ex.Message}");
                LoadDefaultItems();
            }

            _loaded = true;
        }

        private static void LoadDefaultItems()
        {
            _items.Clear();

            AddItem(new ItemData { Id = "weapon_t1_shortsword", Name = "Iron Shortsword", Category = "Weapon", EquipSlot = "Weapon", Tier = 1, Rarity = "Common", AttackBonus = 8, GoldValue = 50, EssenceCost = 30, RequiredDungeonRank = 1 });
            AddItem(new ItemData { Id = "weapon_t1_wooden_staff", Name = "Apprentice Staff", Category = "Weapon", EquipSlot = "Weapon", Tier = 1, Rarity = "Common", AttackBonus = 6, DefenseBonus = 2, HpBonus = 5, GoldValue = 45, EssenceCost = 25, RequiredDungeonRank = 1 });
            AddItem(new ItemData { Id = "armor_t1_leather_vest", Name = "Leather Tunic", Category = "Light Armor", EquipSlot = "Chest", Tier = 1, Rarity = "Common", DefenseBonus = 6, HpBonus = 10, GoldValue = 40, EssenceCost = 25, RequiredDungeonRank = 1 });
            AddItem(new ItemData { Id = "helm_t1_iron_cap", Name = "Iron Skullcap", Category = "Heavy Armor", EquipSlot = "Head", Tier = 1, Rarity = "Common", DefenseBonus = 4, HpBonus = 5, GoldValue = 30, EssenceCost = 20, RequiredDungeonRank = 1 });
            AddItem(new ItemData { Id = "boots_t1_leather_boots", Name = "Scout Trail Boots", Category = "Light Armor", EquipSlot = "Feet", Tier = 1, Rarity = "Common", DefenseBonus = 3, HpBonus = 5, GoldValue = 25, EssenceCost = 15, RequiredDungeonRank = 1 });
            AddItem(new ItemData { Id = "accessory_t1_copper_ring", Name = "Copper Ring of Vigor", Category = "Accessory", EquipSlot = "Accessory", Tier = 1, Rarity = "Common", AttackBonus = 2, DefenseBonus = 2, HpBonus = 15, GoldValue = 60, EssenceCost = 35, RequiredDungeonRank = 1 });
            AddItem(new ItemData { Id = "potion_t1_health", Name = "Lesser Health Potion", Category = "Consumable", EquipSlot = "Consumable", Tier = 1, Rarity = "Common", HpBonus = 50, GoldValue = 20, EssenceCost = 10, RequiredDungeonRank = 1 });
        }

        public static void AddItem(ItemData item)
        {
            if (item != null && !string.IsNullOrEmpty(item.Id))
            {
                _items[item.Id] = item;
            }
        }

        public static ItemData GetItem(string id)
        {
            return _items.TryGetValue(id, out var item) ? item : null;
        }

        public static List<ItemData> GetAllItems()
        {
            return _items.Values.ToList();
        }

        public static List<ItemData> GetItemsByTier(int maxTier)
        {
            return _items.Values.Where(i => i.Tier <= maxTier).ToList();
        }

        public static List<ItemData> GetItemsByCategory(string category)
        {
            return _items.Values.Where(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public static List<ItemData> GetItemsByEquipSlot(string equipSlot)
        {
            return _items.Values.Where(i => i.EquipSlot.Equals(equipSlot, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public static List<ItemData> GetItemsByRarity(string rarity)
        {
            return _items.Values.Where(i => i.Rarity.Equals(rarity, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public static List<ItemData> GetItemsByRankRequirement(int maxRank)
        {
            return _items.Values.Where(i => i.RequiredDungeonRank <= maxRank).ToList();
        }

        public static List<ItemData> GetWeapons(int maxTier = 10)
        {
            return _items.Values.Where(i => i.Category == "Weapon" && i.Tier <= maxTier).ToList();
        }

        public static List<ItemData> GetArmor(int maxTier = 10)
        {
            return _items.Values.Where(i => i.Category == "Heavy Armor" || i.Category == "Light Armor" || i.Category == "Clothes" && i.Tier <= maxTier).ToList();
        }

        public static List<ItemData> GetAccessories(int maxTier = 10)
        {
            return _items.Values.Where(i => i.Category == "Accessory" && i.Tier <= maxTier).ToList();
        }

        public static List<ItemData> GetConsumables(int maxTier = 10)
        {
            return _items.Values.Where(i => i.Category == "Consumable" && i.Tier <= maxTier).ToList();
        }

        public static List<ItemData> GetItemsForSlot(string equipSlot, int maxTier = 10)
        {
            return _items.Values.Where(i => i.EquipSlot.Equals(equipSlot, StringComparison.OrdinalIgnoreCase) && i.Tier <= maxTier).ToList();
        }

        public static int Count => _items.Count;

        public static void Reload(string path = "res://data/items.json")
        {
            _loaded = false;
            LoadFromJson(path);
        }
    }
}