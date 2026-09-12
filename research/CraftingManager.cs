using System;
using System.Collections.Generic;
using Godot;

namespace DungeonLord.Scripts
{
    public class CraftingRecipe
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string ResultItemId { get; set; }
        public string Category { get; set; }
        public int Tier { get; set; }
        public int RequiredCrafterLevel { get; set; }
        public int RequiredDungeonRank { get; set; }
        public float BaseCraftTime { get; set; }
        public long BaseEssenceCost { get; set; }
        public Dictionary<string, int> Ingredients { get; set; } = new();
        public bool IsUnlocked { get; set; }
        public CrafterType RequiredCrafter { get; set; }
    }

    public static class CraftingRecipeDatabase
    {
        private static readonly Dictionary<string, CraftingRecipe> _recipes = new();

        static CraftingRecipeDatabase()
        {
            LoadDefaultRecipes();
        }

        public static void LoadDefaultRecipes()
        {
            _recipes.Clear();

            // BLACKSMITH recipes (Forge)
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_weapon_t1_shortsword",
                DisplayName = "Iron Shortsword",
                ResultItemId = "weapon_t1_shortsword",
                Category = "Weapon",
                Tier = 1,
                RequiredCrafterLevel = 1,
                RequiredDungeonRank = 1,
                BaseCraftTime = 30f,
                BaseEssenceCost = 30,
                Ingredients = new() { { "iron_ore", 3 }, { "coal", 1 } },
                RequiredCrafter = CrafterType.Blacksmith
            });
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_weapon_t2_longsword",
                DisplayName = "Steel Longsword",
                ResultItemId = "weapon_t2_longsword",
                Category = "Weapon",
                Tier = 2,
                RequiredCrafterLevel = 3,
                RequiredDungeonRank = 2,
                BaseCraftTime = 60f,
                BaseEssenceCost = 100,
                Ingredients = new() { { "steel_ingot", 2 }, { "coal", 2 }, { "leather_strips", 1 } },
                RequiredCrafter = CrafterType.Blacksmith
            });
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_weapon_t3_battleaxe",
                DisplayName = "Berserker Battleaxe",
                ResultItemId = "weapon_t3_battleaxe",
                Category = "Weapon",
                Tier = 3,
                RequiredCrafterLevel = 5,
                RequiredDungeonRank = 3,
                BaseCraftTime = 120f,
                BaseEssenceCost = 300,
                Ingredients = new() { { "mithral_ingot", 3 }, { "dragon_bone", 1 }, { "coal", 3 } },
                RequiredCrafter = CrafterType.Blacksmith
            });
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_armor_t2_chainmail",
                DisplayName = "Iron Chainmail",
                ResultItemId = "armor_t2_chainmail",
                Category = "Heavy Armor",
                Tier = 2,
                RequiredCrafterLevel = 2,
                RequiredDungeonRank = 2,
                BaseCraftTime = 90f,
                BaseEssenceCost = 120,
                Ingredients = new() { { "iron_ore", 5 }, { "coal", 3 } },
                RequiredCrafter = CrafterType.Blacksmith
            });
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_armor_t3_plate",
                DisplayName = "Full Steel Plate",
                ResultItemId = "armor_t3_plate",
                Category = "Heavy Armor",
                Tier = 3,
                RequiredCrafterLevel = 6,
                RequiredDungeonRank = 3,
                BaseCraftTime = 180f,
                BaseEssenceCost = 400,
                Ingredients = new() { { "steel_ingot", 8 }, { "mithral_ingot", 2 }, { "coal", 5 } },
                RequiredCrafter = CrafterType.Blacksmith
            });

            // TAILOR recipes (Loom)
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_armor_t1_leather_vest",
                DisplayName = "Leather Tunic",
                ResultItemId = "armor_t1_leather_vest",
                Category = "Light Armor",
                Tier = 1,
                RequiredCrafterLevel = 1,
                RequiredDungeonRank = 1,
                BaseCraftTime = 20f,
                BaseEssenceCost = 25,
                Ingredients = new() { { "cloth", 3 }, { "thread", 2 } },
                RequiredCrafter = CrafterType.Tailor
            });
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_accessory_t1_copper_ring",
                DisplayName = "Copper Ring of Vigor",
                ResultItemId = "accessory_t1_copper_ring",
                Category = "Accessory",
                Tier = 1,
                RequiredCrafterLevel = 2,
                RequiredDungeonRank = 1,
                BaseCraftTime = 15f,
                BaseEssenceCost = 35,
                Ingredients = new() { { "copper_ore", 2 }, { "gem_dust", 1 } },
                RequiredCrafter = CrafterType.Tailor
            });

            // LEATHERWORKER recipes (Tannery)
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_boots_t1_leather_boots",
                DisplayName = "Scout Trail Boots",
                ResultItemId = "boots_t1_leather_boots",
                Category = "Light Armor",
                Tier = 1,
                RequiredCrafterLevel = 1,
                RequiredDungeonRank = 1,
                BaseCraftTime = 25f,
                BaseEssenceCost = 15,
                Ingredients = new() { { "leather", 2 }, { "thread", 1 } },
                RequiredCrafter = CrafterType.Leatherworker
            });
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_helm_t1_iron_cap",
                DisplayName = "Iron Skullcap",
                ResultItemId = "helm_t1_iron_cap",
                Category = "Heavy Armor",
                Tier = 1,
                RequiredCrafterLevel = 2,
                RequiredDungeonRank = 1,
                BaseCraftTime = 30f,
                BaseEssenceCost = 20,
                Ingredients = new() { { "leather", 1 }, { "iron_ore", 2 } },
                RequiredCrafter = CrafterType.Leatherworker
            });

            // ALCHEMIST recipes (Alchemy Lab)
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_potion_t1_health",
                DisplayName = "Lesser Health Potion",
                ResultItemId = "potion_t1_health",
                Category = "Consumable",
                Tier = 1,
                RequiredCrafterLevel = 1,
                RequiredDungeonRank = 1,
                BaseCraftTime = 10f,
                BaseEssenceCost = 10,
                Ingredients = new() { { "herb_red", 2 }, { "water_flask", 1 } },
                RequiredCrafter = CrafterType.Alchemist
            });
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_potion_t2_mana",
                DisplayName = "Mana Potion",
                ResultItemId = "potion_t2_mana",
                Category = "Consumable",
                Tier = 2,
                RequiredCrafterLevel = 3,
                RequiredDungeonRank = 2,
                BaseCraftTime = 20f,
                BaseEssenceCost = 40,
                Ingredients = new() { { "herb_blue", 3 }, { "crystal_shard", 1 }, { "water_flask", 1 } },
                RequiredCrafter = CrafterType.Alchemist
            });
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_accessory_t3_ruby_pendant",
                DisplayName = "Ruby Essence Pendant",
                ResultItemId = "accessory_t3_ruby_pendant",
                Category = "Accessory",
                Tier = 3,
                RequiredCrafterLevel = 5,
                RequiredDungeonRank = 3,
                BaseCraftTime = 60f,
                BaseEssenceCost = 250,
                Ingredients = new() { { "ruby", 1 }, { "gold_ingot", 1 }, { "essence_dust", 5 } },
                RequiredCrafter = CrafterType.Alchemist
            });

            // ENCHANTER recipes (Arcane Shrine)
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_enchant_weapon_fire",
                DisplayName = "Flame Enchantment",
                ResultItemId = "enchant_weapon_fire",
                Category = "Weapon",
                Tier = 2,
                RequiredCrafterLevel = 3,
                RequiredDungeonRank = 3,
                BaseCraftTime = 45f,
                BaseEssenceCost = 150,
                Ingredients = new() { { "fire_essence", 3 }, { "enchanting_dust", 2 } },
                RequiredCrafter = CrafterType.Enchanter
            });
            AddRecipe(new CraftingRecipe
            {
                Id = "craft_enchant_armor_protection",
                DisplayName = "Protection Enchantment",
                ResultItemId = "enchant_armor_protection",
                Category = "Heavy Armor",
                Tier = 2,
                RequiredCrafterLevel = 4,
                RequiredDungeonRank = 3,
                BaseCraftTime = 50f,
                BaseEssenceCost = 180,
                Ingredients = new() { { "earth_essence", 3 }, { "enchanting_dust", 2 } },
                RequiredCrafter = CrafterType.Enchanter
            });
        }

        public static void AddRecipe(CraftingRecipe recipe)
        {
            if (recipe != null && !string.IsNullOrEmpty(recipe.Id))
                _recipes[recipe.Id] = recipe;
        }

        public static CraftingRecipe GetRecipe(string id)
        {
            return _recipes.TryGetValue(id, out var recipe) ? recipe : null;
        }

        public static List<CraftingRecipe> GetRecipesForCrafter(CrafterType crafterType, int crafterLevel, int dungeonRank)
        {
            var result = new List<CraftingRecipe>();
            foreach (var recipe in _recipes.Values)
            {
                if (recipe.RequiredCrafter == crafterType
                    && recipe.RequiredCrafterLevel <= crafterLevel
                    && recipe.RequiredDungeonRank <= dungeonRank)
                {
                    result.Add(recipe);
                }
            }
            return result;
        }

        public static List<CraftingRecipe> GetAllRecipes()
        {
            return new List<CraftingRecipe>(_recipes.Values);
        }

        public static void UnlockRecipe(string id)
        {
            if (_recipes.TryGetValue(id, out var recipe))
                recipe.IsUnlocked = true;
        }
    }

    public class CraftingManager
    {
        private readonly EssenceManager _essenceManager;
        private readonly List<CrafterNPC> _crafters = new();
        private readonly Queue<CraftingJob> _craftingQueue = new();
        private readonly Dictionary<CrafterType, int> _crafterCounts = new();
        private int _nextCrafterId = 1;

        public event Action<CrafterNPC> OnCrafterHired;
        public event Action<CrafterNPC> OnCrafterFired;
        public event Action<CraftingJob> OnJobStarted;
        public event Action<CraftingJob, string> OnJobCompleted;
        public event Action<string> OnRecipeUnlocked;

        public IReadOnlyList<CrafterNPC> Crafters => _crafters.AsReadOnly();
        public IReadOnlyCollection<CraftingJob> Queue => _craftingQueue;

        public CraftingManager(EssenceManager essenceManager)
        {
            _essenceManager = essenceManager;
        }

        public bool HireCrafter(CrafterType type)
        {
            var config = CrafterConfig.Get(type);
            if (config == null) return false;

            if (_essenceManager.CurrentEssence < config.EssenceCostToHire)
                return false;

            if (_essenceManager.DungeonRank < config.UnlockDungeonRank)
                return false;

            var crafter = new CrafterNPC(type, $"{type}_{_nextCrafterId++}");
            _crafters.Add(crafter);
            _crafterCounts[type] = _crafterCounts.GetValueOrDefault(type, 0) + 1;

            _essenceManager.SpendEssence(config.EssenceCostToHire);
            OnCrafterHired?.Invoke(crafter);
            return true;
        }

        public bool FireCrafter(string crafterId)
        {
            var crafter = _crafters.Find(c => c.Id == crafterId);
            if (crafter == null) return false;

            if (crafter.IsWorking)
                crafter.CancelJob();

            crafter.UnassignRoom();
            _crafters.Remove(crafter);
            _crafterCounts[crafter.Type] = Math.Max(0, _crafterCounts.GetValueOrDefault(crafter.Type, 0) - 1);

            OnCrafterFired?.Invoke(crafter);
            return true;
        }

        public bool AssignCrafterToRoom(string crafterId, RoomType roomType, Vector3I position)
        {
            var crafter = _crafters.Find(c => c.Id == crafterId);
            if (crafter == null) return false;

            return crafter.AssignRoom(roomType, position);
        }

        public bool QueueCraft(string recipeId, int quantity = 1)
        {
            var recipe = CraftingRecipeDatabase.GetRecipe(recipeId);
            if (recipe == null) return false;

            // Check if any crafter can make this
            var availableCrafter = _crafters.Find(c =>
                c.Type == recipe.RequiredCrafter &&
                c.Level >= recipe.RequiredCrafterLevel &&
                c.AssignedRoom != RoomType.None &&
                !c.IsWorking);

            if (availableCrafter == null) return false;

            if (_essenceManager.CurrentEssence < recipe.BaseEssenceCost * quantity)
                return false;

            var job = new CraftingJob
            {
                RecipeId = recipeId,
                ResultItemId = recipe.ResultItemId,
                ItemCategory = recipe.Category,
                CraftTime = recipe.BaseCraftTime,
                EssenceCost = recipe.BaseEssenceCost * quantity,
                Ingredients = new Dictionary<string, int>(recipe.Ingredients),
                Quantity = quantity,
                OnComplete = OnJobComplete
            };

            _craftingQueue.Enqueue(job);
            return true;
        }

        public void ProcessQueue(float deltaTime)
        {
            // Update working crafters
            foreach (var crafter in _crafters)
            {
                if (crafter.IsWorking)
                {
                    crafter.UpdateJob(deltaTime);
                }
            }

            // Assign queued jobs to idle crafters
            while (_craftingQueue.Count > 0)
            {
                var job = _craftingQueue.Peek();
                var recipe = CraftingRecipeDatabase.GetRecipe(job.RecipeId);
                if (recipe == null)
                {
                    _craftingQueue.Dequeue();
                    continue;
                }

                var availableCrafter = _crafters.Find(c =>
                    c.Type == recipe.RequiredCrafter &&
                    c.Level >= recipe.RequiredCrafterLevel &&
                    c.AssignedRoom != RoomType.None &&
                    !c.IsWorking);

                if (availableCrafter == null) break;

                _craftingQueue.Dequeue();

                if (!_essenceManager.SpendEssence(job.EssenceCost))
                {
                    // Can't afford, put back
                    _craftingQueue.Enqueue(job);
                    break;
                }

                if (availableCrafter.StartJob(job))
                {
                    OnJobStarted?.Invoke(job);
                }
                else
                {
                    // Refund essence if job couldn't start
                    _essenceManager.AddEssence(job.EssenceCost);
                }
            }
        }

        private void OnJobComplete(string itemId)
        {
            var job = _craftingQueue.Count > 0 ? _craftingQueue.Peek() : null;
            // Find which crafter completed
            var completedCrafter = _crafters.Find(c => c.CurrentJob != null && c.CurrentJob.ResultItemId == itemId && !c.IsWorking);
            
            if (completedCrafter != null)
            {
                completedCrafter.AddXP(50); // Base XP for crafting
                OnJobCompleted?.Invoke(completedCrafter.CurrentJob, itemId);
            }
        }

        public void CheckRecipeUnlocks(int dungeonRank)
        {
            var allRecipes = CraftingRecipeDatabase.GetAllRecipes();
            foreach (var recipe in allRecipes)
            {
                if (!recipe.IsUnlocked && recipe.RequiredDungeonRank <= dungeonRank)
                {
                    // Check if we have a crafter of sufficient level
                    var hasCrafter = _crafters.Exists(c =>
                        c.Type == recipe.RequiredCrafter && c.Level >= recipe.RequiredCrafterLevel);

                    if (hasCrafter)
                    {
                        CraftingRecipeDatabase.UnlockRecipe(recipe.Id);
                        OnRecipeUnlocked?.Invoke(recipe.Id);
                    }
                }
            }
        }

        public List<CraftingRecipe> GetAvailableRecipes(CrafterType crafterType, int crafterLevel, int dungeonRank)
        {
            return CraftingRecipeDatabase.GetRecipesForCrafter(crafterType, crafterLevel, dungeonRank);
        }

        public int GetCrafterCount(CrafterType type) => _crafterCounts.GetValueOrDefault(type, 0);
        public CrafterNPC GetCrafter(string id) => _crafters.Find(c => c.Id == id);
        public List<CrafterNPC> GetCraftersByType(CrafterType type) => _crafters.FindAll(c => c.Type == type);
        public List<CrafterNPC> GetIdleCrafters() => _crafters.FindAll(c => !c.IsWorking && c.AssignedRoom != RoomType.None);
    }
}