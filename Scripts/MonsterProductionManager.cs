using System;
using System.Collections.Generic;
using Godot;

namespace DungeonLord.Scripts
{
    public enum ProductionRoomType
    {
        None,
        Forge,
        Tannery,
        AlchemyLab,
        Loom,
        ArcaneShrine
    }

    public class ProductionRecipe
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string ResultItemId { get; set; }
        public ProductionRoomType RequiredRoom { get; set; }
        public int RequiredDungeonRank { get; set; }
        public float BaseProductionTime { get; set; } // seconds
        public long BaseEssenceCost { get; set; }
        public int OutputQuantity { get; set; } = 1;
        public Dictionary<string, int> InputResources { get; set; } = new();
    }

    public static class ProductionRecipeDatabase
    {
        private static readonly Dictionary<string, ProductionRecipe> _recipes = new();

        static ProductionRecipeDatabase()
        {
            LoadDefaultRecipes();
        }

        public static void LoadDefaultRecipes()
        {
            _recipes.Clear();

            // FORGE recipes (Weapons, Heavy Armor)
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_weapon_t1_shortsword",
                DisplayName = "Iron Shortsword",
                ResultItemId = "weapon_t1_shortsword",
                RequiredRoom = ProductionRoomType.Forge,
                RequiredDungeonRank = 1,
                BaseProductionTime = 60f,
                BaseEssenceCost = 30,
                OutputQuantity = 1,
                InputResources = new() { { "iron_ore", 3 }, { "coal", 1 } }
            });
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_weapon_t2_longsword",
                DisplayName = "Steel Longsword",
                ResultItemId = "weapon_t2_longsword",
                RequiredRoom = ProductionRoomType.Forge,
                RequiredDungeonRank = 2,
                BaseProductionTime = 120f,
                BaseEssenceCost = 100,
                OutputQuantity = 1,
                InputResources = new() { { "steel_ingot", 2 }, { "coal", 2 }, { "leather_strips", 1 } }
            });
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_armor_t2_chainmail",
                DisplayName = "Iron Chainmail",
                ResultItemId = "armor_t2_chainmail",
                RequiredRoom = ProductionRoomType.Forge,
                RequiredDungeonRank = 2,
                BaseProductionTime = 180f,
                BaseEssenceCost = 120,
                OutputQuantity = 1,
                InputResources = new() { { "iron_ore", 5 }, { "coal", 3 } }
            });
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_weapon_t3_battleaxe",
                DisplayName = "Berserker Battleaxe",
                ResultItemId = "weapon_t3_battleaxe",
                RequiredRoom = ProductionRoomType.Forge,
                RequiredDungeonRank = 3,
                BaseProductionTime = 240f,
                BaseEssenceCost = 300,
                OutputQuantity = 1,
                InputResources = new() { { "mithral_ingot", 3 }, { "dragon_bone", 1 }, { "coal", 3 } }
            });

            // TANNERY recipes (Light Armor, Boots, Helm)
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_armor_t1_leather_vest",
                DisplayName = "Leather Tunic",
                ResultItemId = "armor_t1_leather_vest",
                RequiredRoom = ProductionRoomType.Tannery,
                RequiredDungeonRank = 1,
                BaseProductionTime = 40f,
                BaseEssenceCost = 25,
                OutputQuantity = 1,
                InputResources = new() { { "leather", 3 }, { "thread", 2 } }
            });
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_boots_t1_leather_boots",
                DisplayName = "Scout Trail Boots",
                ResultItemId = "boots_t1_leather_boots",
                RequiredRoom = ProductionRoomType.Tannery,
                RequiredDungeonRank = 1,
                BaseProductionTime = 50f,
                BaseEssenceCost = 15,
                OutputQuantity = 1,
                InputResources = new() { { "leather", 2 }, { "thread", 1 } }
            });
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_helm_t1_iron_cap",
                DisplayName = "Iron Skullcap",
                ResultItemId = "helm_t1_iron_cap",
                RequiredRoom = ProductionRoomType.Tannery,
                RequiredDungeonRank = 1,
                BaseProductionTime = 60f,
                BaseEssenceCost = 20,
                OutputQuantity = 1,
                InputResources = new() { { "leather", 1 }, { "iron_ore", 2 } }
            });

            // ALCHEMY LAB recipes (Potions, Accessories)
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_potion_t1_health",
                DisplayName = "Lesser Health Potion",
                ResultItemId = "potion_t1_health",
                RequiredRoom = ProductionRoomType.AlchemyLab,
                RequiredDungeonRank = 1,
                BaseProductionTime = 20f,
                BaseEssenceCost = 10,
                OutputQuantity = 3,
                InputResources = new() { { "herb_red", 2 }, { "water_flask", 1 } }
            });
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_potion_t2_mana",
                DisplayName = "Mana Potion",
                ResultItemId = "potion_t2_mana",
                RequiredRoom = ProductionRoomType.AlchemyLab,
                RequiredDungeonRank = 2,
                BaseProductionTime = 40f,
                BaseEssenceCost = 40,
                OutputQuantity = 2,
                InputResources = new() { { "herb_blue", 3 }, { "crystal_shard", 1 }, { "water_flask", 1 } }
            });
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_accessory_t3_ruby_pendant",
                DisplayName = "Ruby Essence Pendant",
                ResultItemId = "accessory_t3_ruby_pendant",
                RequiredRoom = ProductionRoomType.AlchemyLab,
                RequiredDungeonRank = 3,
                BaseProductionTime = 120f,
                BaseEssenceCost = 250,
                OutputQuantity = 1,
                InputResources = new() { { "ruby", 1 }, { "gold_ingot", 1 }, { "essence_dust", 5 } }
            });

            // LOOM recipes (Clothes, Light Armor, Accessories)
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_accessory_t1_copper_ring",
                DisplayName = "Copper Ring of Vigor",
                ResultItemId = "accessory_t1_copper_ring",
                RequiredRoom = ProductionRoomType.Loom,
                RequiredDungeonRank = 1,
                BaseProductionTime = 30f,
                BaseEssenceCost = 35,
                OutputQuantity = 1,
                InputResources = new() { { "copper_ore", 2 }, { "gem_dust", 1 } }
            });
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_armor_t1_cloth_robe",
                DisplayName = "Apprentice Robe",
                ResultItemId = "armor_t1_cloth_robe",
                RequiredRoom = ProductionRoomType.Loom,
                RequiredDungeonRank = 1,
                BaseProductionTime = 45f,
                BaseEssenceCost = 30,
                OutputQuantity = 1,
                InputResources = new() { { "cloth", 4 }, { "thread", 3 } }
            });

            // ARCANE SHRINE recipes (Enchantments, High-tier Accessories)
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_enchant_weapon_fire",
                DisplayName = "Flame Enchantment",
                ResultItemId = "enchant_weapon_fire",
                RequiredRoom = ProductionRoomType.ArcaneShrine,
                RequiredDungeonRank = 3,
                BaseProductionTime = 90f,
                BaseEssenceCost = 150,
                OutputQuantity = 1,
                InputResources = new() { { "fire_essence", 3 }, { "enchanting_dust", 2 } }
            });
            AddRecipe(new ProductionRecipe
            {
                Id = "prod_enchant_armor_protection",
                DisplayName = "Protection Enchantment",
                ResultItemId = "enchant_armor_protection",
                RequiredRoom = ProductionRoomType.ArcaneShrine,
                RequiredDungeonRank = 3,
                BaseProductionTime = 100f,
                BaseEssenceCost = 180,
                OutputQuantity = 1,
                InputResources = new() { { "earth_essence", 3 }, { "enchanting_dust", 2 } }
            });
        }

        public static void AddRecipe(ProductionRecipe recipe)
        {
            if (recipe != null && !string.IsNullOrEmpty(recipe.Id))
                _recipes[recipe.Id] = recipe;
        }

        public static ProductionRecipe GetRecipe(string id)
        {
            return _recipes.TryGetValue(id, out var recipe) ? recipe : null;
        }

        public static List<ProductionRecipe> GetRecipesForRoom(ProductionRoomType roomType, int dungeonRank)
        {
            var result = new List<ProductionRecipe>();
            foreach (var recipe in _recipes.Values)
            {
                if (recipe.RequiredRoom == roomType && recipe.RequiredDungeonRank <= dungeonRank)
                    result.Add(recipe);
            }
            return result;
        }

        public static List<ProductionRecipe> GetAllRecipes() => new(_recipes.Values);
    }

    public class ProductionJob
    {
        public string RecipeId { get; set; }
        public string ResultItemId { get; set; }
        public int Quantity { get; set; }
        public float TotalTime { get; set; }
        public float ElapsedTime { get; set; }
        public long EssenceCost { get; set; }
        public ProductionRoomType RoomType { get; set; }
        public Vector3I RoomPosition { get; set; }
        public Action<ProductionJob, string> OnComplete { get; set; }
    }

    public class MonsterProductionManager
    {
        private readonly EssenceManager _essenceManager;
        private readonly DungeonGrid _dungeonGrid;
        private readonly Dictionary<Vector3I, ProductionJob> _activeProductions = new();
        private readonly Dictionary<ProductionRoomType, int> _roomProductionSlots = new();
        private const int BaseSlotsPerRoom = 1;

        public event Action<ProductionJob, string> OnProductionStarted;
        public event Action<ProductionJob, string> OnProductionCompleted;

        public MonsterProductionManager(EssenceManager essenceManager, DungeonGrid dungeonGrid)
        {
            _essenceManager = essenceManager;
            _dungeonGrid = dungeonGrid;

            foreach (ProductionRoomType roomType in Enum.GetValues(typeof(ProductionRoomType)))
            {
                if (roomType != ProductionRoomType.None)
                    _roomProductionSlots[roomType] = BaseSlotsPerRoom;
            }
        }

        public bool StartProduction(string recipeId, Vector3I roomPosition)
        {
            var tile = _dungeonGrid.GetTile(roomPosition.X, roomPosition.Y, roomPosition.Z);
            if (tile == null || tile.Type != DungeonGrid.TileType.Room)
                return false;

            var roomType = GetProductionRoomType(tile.RoomId);
            if (roomType == ProductionRoomType.None)
                return false;

            var recipe = ProductionRecipeDatabase.GetRecipe(recipeId);
            if (recipe == null || recipe.RequiredRoom != roomType)
                return false;

            if (_essenceManager.DungeonRank < recipe.RequiredDungeonRank)
                return false;

            if (_essenceManager.CurrentEssence < recipe.BaseEssenceCost)
                return false;

            int activeInRoom = CountActiveInRoom(roomPosition);
            int maxSlots = _roomProductionSlots.GetValueOrDefault(roomType, BaseSlotsPerRoom);
            if (activeInRoom >= maxSlots)
                return false;

            if (!_essenceManager.SpendEssence(recipe.BaseEssenceCost))
                return false;

            var job = new ProductionJob
            {
                RecipeId = recipeId,
                ResultItemId = recipe.ResultItemId,
                Quantity = recipe.OutputQuantity,
                TotalTime = recipe.BaseProductionTime,
                ElapsedTime = 0f,
                EssenceCost = recipe.BaseEssenceCost,
                RoomType = roomType,
                RoomPosition = roomPosition,
                OnComplete = OnJobComplete
            };

            _activeProductions[roomPosition] = job;
            OnProductionStarted?.Invoke(job, recipe.ResultItemId);
            return true;
        }

        public void Update(float deltaTime)
        {
            var completed = new List<Vector3I>();

            foreach (var kvp in _activeProductions)
            {
                var job = kvp.Value;
                job.ElapsedTime += deltaTime;

                if (job.ElapsedTime >= job.TotalTime)
                {
                    completed.Add(kvp.Key);
                }
            }

            foreach (var pos in completed)
            {
                var job = _activeProductions[pos];
                job.OnComplete?.Invoke(job, job.ResultItemId);
                _activeProductions.Remove(pos);
            }
        }

        private void OnJobComplete(ProductionJob job, string itemId)
        {
            // Item is produced - in a full impl this would add to inventory/storage
            OnProductionCompleted?.Invoke(job, itemId);
        }

        public ProductionJob GetActiveProduction(Vector3I roomPosition)
        {
            return _activeProductions.TryGetValue(roomPosition, out var job) ? job : null;
        }

        public List<ProductionJob> GetAllActiveProductions() => new(_activeProductions.Values);

        public bool CanProduceInRoom(Vector3I roomPosition)
        {
            var tile = _dungeonGrid.GetTile(roomPosition.X, roomPosition.Y, roomPosition.Z);
            if (tile == null || tile.Type != DungeonGrid.TileType.Room)
                return false;

            var roomType = GetProductionRoomType(tile.RoomId);
            if (roomType == ProductionRoomType.None)
                return false;

            int activeInRoom = CountActiveInRoom(roomPosition);
            int maxSlots = _roomProductionSlots.GetValueOrDefault(roomType, BaseSlotsPerRoom);
            return activeInRoom < maxSlots;
        }

        public List<ProductionRecipe> GetAvailableRecipes(Vector3I roomPosition)
        {
            var tile = _dungeonGrid.GetTile(roomPosition.X, roomPosition.Y, roomPosition.Z);
            if (tile == null || tile.Type != DungeonGrid.TileType.Room)
                return new();

            var roomType = GetProductionRoomType(tile.RoomId);
            if (roomType == ProductionRoomType.None)
                return new();

            return ProductionRecipeDatabase.GetRecipesForRoom(roomType, _essenceManager.DungeonRank);
        }

        public void UpgradeRoomSlots(ProductionRoomType roomType, int additionalSlots)
        {
            if (_roomProductionSlots.ContainsKey(roomType))
                _roomProductionSlots[roomType] += additionalSlots;
        }

        private ProductionRoomType GetProductionRoomType(string roomId)
        {
            return roomId?.ToLower() switch
            {
                "forge" => ProductionRoomType.Forge,
                "tannery" => ProductionRoomType.Tannery,
                "alchemy" or "alchemylab" => ProductionRoomType.AlchemyLab,
                "loom" => ProductionRoomType.Loom,
                "arcane" or "arcaneshrine" => ProductionRoomType.ArcaneShrine,
                _ => ProductionRoomType.None
            };
        }

        private int CountActiveInRoom(Vector3I roomPosition)
        {
            int count = 0;
            foreach (var kvp in _activeProductions)
            {
                if (kvp.Key == roomPosition)
                    count++;
            }
            return count;
        }
    }
}