using System;
using System.Collections.Generic;
using Godot;

namespace DungeonLord.Scripts
{
    public class MarketItem
    {
        public string ItemId { get; set; }
        public string DisplayName { get; set; }
        public int Tier { get; set; }
        public int RequiredDungeonRank { get; set; }
        public float ReputationRequirement { get; set; }
        public long BaseGoldCost { get; set; }
        public long BaseEssenceCost { get; set; }
        public int Stock { get; set; } = -1; // -1 = unlimited
        public float RestockIntervalHours { get; set; } = 24f;
        public DateTime LastRestockTime { get; set; } = DateTime.UtcNow;
    }

    public static class MarketCatalog
    {
        private static readonly Dictionary<string, MarketItem> _items = new();

        static MarketCatalog()
        {
            LoadDefaultCatalog();
        }

        public static void LoadDefaultCatalog()
        {
            _items.Clear();

            // WEAPONS
            AddItem(new MarketItem
            {
                ItemId = "weapon_t1_shortsword",
                DisplayName = "Iron Shortsword",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 1.0f,
                BaseGoldCost = 50,
                BaseEssenceCost = 30,
                Stock = 10,
                RestockIntervalHours = 12f
            });
            AddItem(new MarketItem
            {
                ItemId = "weapon_t1_wooden_staff",
                DisplayName = "Apprentice Staff",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 1.0f,
                BaseGoldCost = 45,
                BaseEssenceCost = 25,
                Stock = 8,
                RestockIntervalHours = 12f
            });
            AddItem(new MarketItem
            {
                ItemId = "weapon_t2_longsword",
                DisplayName = "Steel Longsword",
                Tier = 2,
                RequiredDungeonRank = 2,
                ReputationRequirement = 2.5f,
                BaseGoldCost = 180,
                BaseEssenceCost = 100,
                Stock = 5,
                RestockIntervalHours = 24f
            });
            AddItem(new MarketItem
            {
                ItemId = "weapon_t3_battleaxe",
                DisplayName = "Berserker Battleaxe",
                Tier = 3,
                RequiredDungeonRank = 3,
                ReputationRequirement = 4.0f,
                BaseGoldCost = 500,
                BaseEssenceCost = 300,
                Stock = 2,
                RestockIntervalHours = 48f
            });
            AddItem(new MarketItem
            {
                ItemId = "weapon_t4_greatsword",
                DisplayName = "Knight's Greatsword",
                Tier = 4,
                RequiredDungeonRank = 4,
                ReputationRequirement = 6.0f,
                BaseGoldCost = 1200,
                BaseEssenceCost = 750,
                Stock = 1,
                RestockIntervalHours = 72f
            });
            AddItem(new MarketItem
            {
                ItemId = "weapon_t5_dragon_blade",
                DisplayName = "Dragonfire Blade",
                Tier = 5,
                RequiredDungeonRank = 5,
                ReputationRequirement = 8.5f,
                BaseGoldCost = 3500,
                BaseEssenceCost = 2000,
                Stock = 1,
                RestockIntervalHours = 168f // weekly
            });

            // ARMOR - Heavy
            AddItem(new MarketItem
            {
                ItemId = "armor_t2_chainmail",
                DisplayName = "Iron Chainmail",
                Tier = 2,
                RequiredDungeonRank = 2,
                ReputationRequirement = 2.0f,
                BaseGoldCost = 200,
                BaseEssenceCost = 120,
                Stock = 4,
                RestockIntervalHours = 24f
            });
            AddItem(new MarketItem
            {
                ItemId = "armor_t3_plate",
                DisplayName = "Full Steel Plate",
                Tier = 3,
                RequiredDungeonRank = 3,
                ReputationRequirement = 4.5f,
                BaseGoldCost = 650,
                BaseEssenceCost = 400,
                Stock = 2,
                RestockIntervalHours = 48f
            });

            // ARMOR - Light
            AddItem(new MarketItem
            {
                ItemId = "armor_t1_leather_vest",
                DisplayName = "Leather Tunic",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 1.0f,
                BaseGoldCost = 40,
                BaseEssenceCost = 25,
                Stock = 10,
                RestockIntervalHours = 12f
            });
            AddItem(new MarketItem
            {
                ItemId = "armor_t1_cloth_robe",
                DisplayName = "Apprentice Robe",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 1.5f,
                BaseGoldCost = 35,
                BaseEssenceCost = 20,
                Stock = 8,
                RestockIntervalHours = 12f
            });

            // HELMETS
            AddItem(new MarketItem
            {
                ItemId = "helm_t1_iron_cap",
                DisplayName = "Iron Skullcap",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 1.0f,
                BaseGoldCost = 30,
                BaseEssenceCost = 20,
                Stock = 6,
                RestockIntervalHours = 12f
            });

            // BOOTS
            AddItem(new MarketItem
            {
                ItemId = "boots_t1_leather_boots",
                DisplayName = "Scout Trail Boots",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 1.0f,
                BaseGoldCost = 25,
                BaseEssenceCost = 15,
                Stock = 10,
                RestockIntervalHours = 12f
            });

            // ACCESSORIES
            AddItem(new MarketItem
            {
                ItemId = "accessory_t1_copper_ring",
                DisplayName = "Copper Ring of Vigor",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 1.5f,
                BaseGoldCost = 60,
                BaseEssenceCost = 35,
                Stock = 5,
                RestockIntervalHours = 24f
            });
            AddItem(new MarketItem
            {
                ItemId = "accessory_t3_ruby_pendant",
                DisplayName = "Ruby Essence Pendant",
                Tier = 3,
                RequiredDungeonRank = 3,
                ReputationRequirement = 5.0f,
                BaseGoldCost = 800,
                BaseEssenceCost = 500,
                Stock = 1,
                RestockIntervalHours = 72f
            });

            // CONSUMABLES
            AddItem(new MarketItem
            {
                ItemId = "potion_t1_health",
                DisplayName = "Lesser Health Potion",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 1.0f,
                BaseGoldCost = 20,
                BaseEssenceCost = 10,
                Stock = 20,
                RestockIntervalHours = 6f
            });
            AddItem(new MarketItem
            {
                ItemId = "potion_t2_mana",
                DisplayName = "Mana Potion",
                Tier = 2,
                RequiredDungeonRank = 2,
                ReputationRequirement = 2.5f,
                BaseGoldCost = 80,
                BaseEssenceCost = 40,
                Stock = 10,
                RestockIntervalHours = 12f
            });

            // ENCHANTMENTS
            AddItem(new MarketItem
            {
                ItemId = "enchant_weapon_fire",
                DisplayName = "Flame Enchantment Scroll",
                Tier = 2,
                RequiredDungeonRank = 3,
                ReputationRequirement = 4.0f,
                BaseGoldCost = 300,
                BaseEssenceCost = 150,
                Stock = 2,
                RestockIntervalHours = 48f
            });
            AddItem(new MarketItem
            {
                ItemId = "enchant_armor_protection",
                DisplayName = "Protection Enchantment Scroll",
                Tier = 2,
                RequiredDungeonRank = 3,
                ReputationRequirement = 4.0f,
                BaseGoldCost = 350,
                BaseEssenceCost = 180,
                Stock = 2,
                RestockIntervalHours = 48f
            });

            // RESOURCES (unlimited stock, no rep requirement for basics)
            AddItem(new MarketItem
            {
                ItemId = "iron_ore",
                DisplayName = "Iron Ore",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 0.5f,
                BaseGoldCost = 5,
                BaseEssenceCost = 2,
                Stock = -1,
                RestockIntervalHours = 1f
            });
            AddItem(new MarketItem
            {
                ItemId = "coal",
                DisplayName = "Coal",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 0.5f,
                BaseGoldCost = 3,
                BaseEssenceCost = 1,
                Stock = -1,
                RestockIntervalHours = 1f
            });
            AddItem(new MarketItem
            {
                ItemId = "leather",
                DisplayName = "Raw Leather",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 0.5f,
                BaseGoldCost = 8,
                BaseEssenceCost = 3,
                Stock = -1,
                RestockIntervalHours = 2f
            });
            AddItem(new MarketItem
            {
                ItemId = "cloth",
                DisplayName = "Cloth Bolt",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 0.5f,
                BaseGoldCost = 6,
                BaseEssenceCost = 2,
                Stock = -1,
                RestockIntervalHours = 1f
            });
            AddItem(new MarketItem
            {
                ItemId = "water_flask",
                DisplayName = "Water Flask",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 0.5f,
                BaseGoldCost = 2,
                BaseEssenceCost = 1,
                Stock = -1,
                RestockIntervalHours = 1f
            });
            AddItem(new MarketItem
            {
                ItemId = "herb_red",
                DisplayName = "Red Herb",
                Tier = 1,
                RequiredDungeonRank = 1,
                ReputationRequirement = 1.0f,
                BaseGoldCost = 10,
                BaseEssenceCost = 3,
                Stock = -1,
                RestockIntervalHours = 4f
            });
            AddItem(new MarketItem
            {
                ItemId = "herb_blue",
                DisplayName = "Blue Herb",
                Tier = 2,
                RequiredDungeonRank = 2,
                ReputationRequirement = 2.0f,
                BaseGoldCost = 25,
                BaseEssenceCost = 8,
                Stock = -1,
                RestockIntervalHours = 8f
            });
            AddItem(new MarketItem
            {
                ItemId = "steel_ingot",
                DisplayName = "Steel Ingot",
                Tier = 2,
                RequiredDungeonRank = 2,
                ReputationRequirement = 2.0f,
                BaseGoldCost = 40,
                BaseEssenceCost = 15,
                Stock = -1,
                RestockIntervalHours = 12f
            });
            AddItem(new MarketItem
            {
                ItemId = "mithral_ingot",
                DisplayName = "Mithral Ingot",
                Tier = 3,
                RequiredDungeonRank = 3,
                ReputationRequirement = 4.0f,
                BaseGoldCost = 150,
                BaseEssenceCost = 50,
                Stock = 3,
                RestockIntervalHours = 24f
            });
            AddItem(new MarketItem
            {
                ItemId = "dragon_bone",
                DisplayName = "Dragon Bone",
                Tier = 3,
                RequiredDungeonRank = 3,
                ReputationRequirement = 5.0f,
                BaseGoldCost = 300,
                BaseEssenceCost = 100,
                Stock = 1,
                RestockIntervalHours = 48f
            });
            AddItem(new MarketItem
            {
                ItemId = "ruby",
                DisplayName = "Ruby",
                Tier = 3,
                RequiredDungeonRank = 3,
                ReputationRequirement = 4.5f,
                BaseGoldCost = 200,
                BaseEssenceCost = 80,
                Stock = 2,
                RestockIntervalHours = 24f
            });
            AddItem(new MarketItem
            {
                ItemId = "gold_ingot",
                DisplayName = "Gold Ingot",
                Tier = 2,
                RequiredDungeonRank = 2,
                ReputationRequirement = 3.0f,
                BaseGoldCost = 100,
                BaseEssenceCost = 30,
                Stock = -1,
                RestockIntervalHours = 12f
            });
            AddItem(new MarketItem
            {
                ItemId = "enchanting_dust",
                DisplayName = "Enchanting Dust",
                Tier = 2,
                RequiredDungeonRank = 3,
                ReputationRequirement = 3.5f,
                BaseGoldCost = 80,
                BaseEssenceCost = 40,
                Stock = -1,
                RestockIntervalHours = 12f
            });
            AddItem(new MarketItem
            {
                ItemId = "fire_essence",
                DisplayName = "Fire Essence",
                Tier = 2,
                RequiredDungeonRank = 3,
                ReputationRequirement = 4.0f,
                BaseGoldCost = 100,
                BaseEssenceCost = 50,
                Stock = 2,
                RestockIntervalHours = 24f
            });
            AddItem(new MarketItem
            {
                ItemId = "earth_essence",
                DisplayName = "Earth Essence",
                Tier = 2,
                RequiredDungeonRank = 3,
                ReputationRequirement = 4.0f,
                BaseGoldCost = 100,
                BaseEssenceCost = 50,
                Stock = 2,
                RestockIntervalHours = 24f
            });
        }

        public static void AddItem(MarketItem item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ItemId))
                _items[item.ItemId] = item;
        }

        public static MarketItem GetItem(string itemId)
        {
            return _items.TryGetValue(itemId, out var item) ? item : null;
        }

        public static List<MarketItem> GetAvailableItems(float reputation, int dungeonRank)
        {
            var result = new List<MarketItem>();
            foreach (var item in _items.Values)
            {
                if (item.ReputationRequirement <= reputation && item.RequiredDungeonRank <= dungeonRank)
                    result.Add(item);
            }
            return result;
        }

        public static List<MarketItem> GetAllItems() => new(_items.Values);
    }

    public class MarketManager
    {
        private readonly EssenceManager _essenceManager;
        private readonly InvaderAI _invaderAI;
        private long _gold = 1000; // Starting gold

        public event Action<long> OnGoldChanged;
        public event Action<string, int> OnItemPurchased;
        public event Action<string> OnItemSold;

        public MarketManager(EssenceManager essenceManager, InvaderAI invaderAI)
        {
            _essenceManager = essenceManager;
            _invaderAI = invaderAI;
        }

        public long Gold => _gold;
        public float SettlementReputation => _invaderAI?.SettlementReputation ?? 1.0f;
        public int DungeonRank => _essenceManager?.DungeonRank ?? 1;

        public void AddGold(long amount)
        {
            if (amount <= 0) return;
            _gold += amount;
            OnGoldChanged?.Invoke(_gold);
        }

        public bool SpendGold(long amount)
        {
            if (amount <= 0 || _gold < amount) return false;
            _gold -= amount;
            OnGoldChanged?.Invoke(_gold);
            return true;
        }

        public bool CanAfford(MarketItem item, CurrencyType currency)
        {
            long cost = CalculateCost(item, currency);
            return currency == CurrencyType.Gold ? _gold >= cost : _essenceManager.CurrentEssence >= cost;
        }

        public long CalculateCost(MarketItem item, CurrencyType currency)
        {
            float reputationFactor = CalculateReputationDiscount();
            float rankFactor = 1f + (DungeonRank - 1) * 0.1f; // 10% increase per rank

            long baseCost = currency == CurrencyType.Gold ? item.BaseGoldCost : item.BaseEssenceCost;
            return (long)Math.Ceiling(baseCost * reputationFactor * rankFactor);
        }

        private float CalculateReputationDiscount()
        {
            // Higher reputation = discount (minimum 50% of base price)
            float rep = SettlementReputation;
            if (rep >= 10f) return 0.5f;
            if (rep >= 5f) return 0.6f;
            if (rep >= 3f) return 0.75f;
            if (rep >= 2f) return 0.85f;
            return 1.0f; // No discount at low reputation
        }

        public bool PurchaseItem(string itemId, CurrencyType currency, int quantity = 1)
        {
            var item = MarketCatalog.GetItem(itemId);
            if (item == null) return false;

            if (item.RequiredDungeonRank > DungeonRank) return false;
            if (item.ReputationRequirement > SettlementReputation) return false;

            if (item.Stock > 0 && item.Stock < quantity) return false;

            long totalCost = CalculateCost(item, currency) * quantity;

            bool success = currency == CurrencyType.Gold
                ? SpendGold(totalCost)
                : _essenceManager.SpendEssence(totalCost);

            if (!success) return false;

            if (item.Stock > 0)
                item.Stock -= quantity;

            // In a full impl, this would add item to player inventory
            OnItemPurchased?.Invoke(itemId, quantity);
            return true;
        }

        public bool SellItem(string itemId, int quantity = 1)
        {
            var itemData = ItemDatabase.GetItem(itemId);
            if (itemData == null) return false;

            // Sell at 40% of gold value, no essence return
            long sellValue = (long)(itemData.GoldValue * 0.4f * quantity);
            _gold += sellValue;
            OnGoldChanged?.Invoke(_gold);
            OnItemSold?.Invoke(itemId);
            return true;
        }

        public List<MarketItem> GetAvailableMarketItems()
        {
            return MarketCatalog.GetAvailableItems(SettlementReputation, DungeonRank);
        }

        public void UpdateRestocks(float deltaHours)
        {
            foreach (var item in MarketCatalog.GetAllItems())
            {
                if (item.Stock >= 0 && item.Stock < 20) // Only restock limited items
                {
                    float hoursSinceRestock = (float)(DateTime.UtcNow - item.LastRestockTime).TotalHours;
                    if (hoursSinceRestock >= item.RestockIntervalHours)
                    {
                        item.Stock = Math.Min(item.Stock + 1, 20);
                        item.LastRestockTime = DateTime.UtcNow;
                    }
                }
            }
        }

        public MarketItem GetMarketItem(string itemId) => MarketCatalog.GetItem(itemId);
    }

    public enum CurrencyType
    {
        Gold,
        Essence
    }
}