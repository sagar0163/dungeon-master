using System;
using System.Collections.Generic;
using Godot;

namespace DungeonLord.Scripts
{
    public enum CrafterType
    {
        Blacksmith,
        Tailor,
        Leatherworker,
        Alchemist,
        Enchanter
    }

    public enum RoomType
    {
        None,
        Forge,
        Tannery,
        AlchemyLab,
        Loom,
        ArcaneShrine
    }

    public static class CrafterConfig
    {
        public static readonly Dictionary<CrafterType, CrafterData> Data = new()
        {
            [CrafterType.Blacksmith] = new CrafterData
            {
                Type = CrafterType.Blacksmith,
                DisplayName = "Blacksmith",
                RequiredRoom = RoomType.Forge,
                BaseCraftSpeed = 1.0f,
                SpeedMultiplierPerLevel = 0.15f,
                EssenceCostToHire = 150,
                UnlockDungeonRank = 2,
                PrimaryCategories = new[] { "Weapon", "Heavy Armor" }
            },
            [CrafterType.Tailor] = new CrafterData
            {
                Type = CrafterType.Tailor,
                DisplayName = "Tailor",
                RequiredRoom = RoomType.Loom,
                BaseCraftSpeed = 1.0f,
                SpeedMultiplierPerLevel = 0.12f,
                EssenceCostToHire = 120,
                UnlockDungeonRank = 1,
                PrimaryCategories = new[] { "Light Armor", "Accessory" }
            },
            [CrafterType.Leatherworker] = new CrafterData
            {
                Type = CrafterType.Leatherworker,
                DisplayName = "Leatherworker",
                RequiredRoom = RoomType.Tannery,
                BaseCraftSpeed = 1.0f,
                SpeedMultiplierPerLevel = 0.14f,
                EssenceCostToHire = 130,
                UnlockDungeonRank = 1,
                PrimaryCategories = new[] { "Light Armor", "Consumable" }
            },
            [CrafterType.Alchemist] = new CrafterData
            {
                Type = CrafterType.Alchemist,
                DisplayName = "Alchemist",
                RequiredRoom = RoomType.AlchemyLab,
                BaseCraftSpeed = 1.0f,
                SpeedMultiplierPerLevel = 0.10f,
                EssenceCostToHire = 200,
                UnlockDungeonRank = 3,
                PrimaryCategories = new[] { "Consumable", "Accessory" }
            },
            [CrafterType.Enchanter] = new CrafterData
            {
                Type = CrafterType.Enchanter,
                DisplayName = "Enchanter",
                RequiredRoom = RoomType.ArcaneShrine,
                BaseCraftSpeed = 1.0f,
                SpeedMultiplierPerLevel = 0.08f,
                EssenceCostToHire = 300,
                UnlockDungeonRank = 4,
                PrimaryCategories = new[] { "Weapon", "Heavy Armor", "Light Armor", "Accessory" }
            }
        };

        public static CrafterData Get(CrafterType type) => Data.TryGetValue(type, out var d) ? d : null;
        public static CrafterData GetByRoom(RoomType room) => Data.Values.FirstOrDefault(d => d.RequiredRoom == room);
        public static bool CanCraft(CrafterType type, string itemCategory) => Data.TryGetValue(type, out var d) && d.PrimaryCategories.Contains(itemCategory);
    }

    public class CrafterData
    {
        public CrafterType Type { get; set; }
        public string DisplayName { get; set; }
        public RoomType RequiredRoom { get; set; }
        public float BaseCraftSpeed { get; set; }
        public float SpeedMultiplierPerLevel { get; set; }
        public long EssenceCostToHire { get; set; }
        public int UnlockDungeonRank { get; set; }
        public string[] PrimaryCategories { get; set; } = Array.Empty<string>();
    }

    public class CrafterNPC
    {
        public string Id { get; }
        public CrafterType Type { get; }
        public CrafterData Config { get; }
        public int Level { get; private set; } = 1;
        public long XP { get; private set; } = 0;
        public RoomType AssignedRoom { get; private set; } = RoomType.None;
        public Vector3I RoomPosition { get; private set; }
        public bool IsWorking { get; private set; }
        public CraftingJob CurrentJob { get; private set; }
        public float JobProgress { get; private set; }
        public long TotalItemsCrafted { get; private set; }

        public CrafterNPC(CrafterType type, string id = null)
        {
            Type = type;
            Id = id ?? $"{type}_{Guid.NewGuid().ToString()[..8]}";
            Config = CrafterConfig.Get(type);
        }

        public bool AssignRoom(RoomType roomType, Vector3I position)
        {
            if (Config.RequiredRoom != roomType) return false;
            AssignedRoom = roomType;
            RoomPosition = position;
            return true;
        }

        public void UnassignRoom()
        {
            AssignedRoom = RoomType.None;
            RoomPosition = default;
        }

        public float GetCraftSpeedMultiplier()
        {
            return Config.BaseCraftSpeed * (1f + (Level - 1) * Config.SpeedMultiplierPerLevel);
        }

        public long GetXpForNextLevel()
        {
            return 100L * Level * Level;
        }

        public bool AddXP(long amount)
        {
            XP += amount;
            long needed = GetXpForNextLevel();
            bool leveledUp = false;

            while (XP >= needed)
            {
                XP -= needed;
                Level++;
                needed = GetXpForNextLevel();
                leveledUp = true;
            }

            return leveledUp;
        }

        public bool StartJob(CraftingJob job)
        {
            if (IsWorking || CurrentJob != null) return false;
            if (!CrafterConfig.CanCraft(Type, job.ItemCategory)) return false;

            CurrentJob = job;
            IsWorking = true;
            JobProgress = 0f;
            return true;
        }

        public bool UpdateJob(float deltaTime)
        {
            if (!IsWorking || CurrentJob == null) return false;

            float speed = GetCraftSpeedMultiplier();
            JobProgress += deltaTime * speed;

            if (JobProgress >= CurrentJob.CraftTime)
            {
                CompleteJob();
                return true;
            }
            return false;
        }

        private void CompleteJob()
        {
            if (CurrentJob == null) return;

            TotalItemsCrafted++;
            CurrentJob.OnComplete?.Invoke(CurrentJob.ResultItemId);
            CurrentJob = null;
            IsWorking = false;
            JobProgress = 0f;
        }

        public void CancelJob()
        {
            CurrentJob = null;
            IsWorking = false;
            JobProgress = 0f;
        }
    }

    public class CraftingJob
    {
        public string RecipeId { get; set; }
        public string ResultItemId { get; set; }
        public string ItemCategory { get; set; }
        public float CraftTime { get; set; }
        public long EssenceCost { get; set; }
        public Dictionary<string, int> Ingredients { get; set; } = new();
        public int Quantity { get; set; } = 1;
        public Action<string> OnComplete { get; set; }
    }
}