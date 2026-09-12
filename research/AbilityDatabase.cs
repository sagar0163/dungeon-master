using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace DungeonLord.Scripts
{
    public enum AbilityType
    {
        Active,
        Passive
    }

    public enum AbilityCategory
    {
        Combat,
        Movement,
        Aoe,
        Defense,
        Utility
    }

    public enum TargetType
    {
        SingleEnemy,
        EmptyTileNearEnemy,
        ConeAoe,
        Self,
        FriendlyMonster
    }

    public class AbilityData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public string TypeString { get; set; }
        public AbilityType Type => Enum.TryParse(TypeString, true, out AbilityType t) ? t : AbilityType.Active;

        [JsonPropertyName("category")]
        public string CategoryString { get; set; }
        public AbilityCategory Category => Enum.TryParse(CategoryString, true, out AbilityCategory c) ? c : AbilityCategory.Combat;

        public float Cooldown { get; set; }
        public long EssenceCost { get; set; }
        public int BaseDamage { get; set; }

        [JsonPropertyName("damage_scaling")]
        public string DamageScalingString { get; set; }
        public DamageScaling DamageScaling => Enum.TryParse(DamageScalingString, true, out DamageScaling d) ? d : DamageScaling.None;

        public int Range { get; set; }

        [JsonPropertyName("target_type")]
        public string TargetTypeString { get; set; }
        public TargetType TargetType => Enum.TryParse(TargetTypeString, true, out TargetType t) ? t : TargetType.SingleEnemy;

        [JsonPropertyName("unlock_level")]
        public int UnlockLevel { get; set; }

        [JsonPropertyName("required_dungeon_rank")]
        public int RequiredDungeonRank { get; set; }

        public string Icon { get; set; }
        public string Animation { get; set; }

        [JsonPropertyName("sound_effect")]
        public string SoundEffect { get; set; }

        [JsonPropertyName("special_effects")]
        public List<string> SpecialEffects { get; set; } = new();

        // Cone AOE specific
        [JsonPropertyName("cone_angle")]
        public int ConeAngle { get; set; }

        [JsonPropertyName("cone_length")]
        public int ConeLength { get; set; }

        // Shield specific
        [JsonPropertyName("shield_hp")]
        public int ShieldHp { get; set; }

        [JsonPropertyName("shield_hp_scaling")]
        public string ShieldHpScalingString { get; set; }
        public DamageScaling ShieldHpScaling => Enum.TryParse(ShieldHpScalingString, true, out DamageScaling d) ? d : DamageScaling.None;

        [JsonPropertyName("reflect_percentage")]
        public int ReflectPercentage { get; set; }

        // Possession specific
        [JsonPropertyName("max_duration")]
        public float MaxDuration { get; set; }

        [JsonPropertyName("cooldown_reduction_per_rank")]
        public float CooldownReductionPerRank { get; set; }

        // Runtime state (not serialized)
        [JsonIgnore]
        public float CurrentCooldown { get; set; }

        [JsonIgnore]
        public bool IsOnCooldown => CurrentCooldown > 0f;

        public bool CanUse(int lordLevel, int dungeonRank, long currentEssence)
        {
            if (IsOnCooldown) return false;
            if (lordLevel < UnlockLevel) return false;
            if (dungeonRank < RequiredDungeonRank) return false;
            if (currentEssence < EssenceCost) return false;
            return true;
        }

        public float GetEffectiveCooldown(int dungeonRank)
        {
            if (CooldownReductionPerRank > 0 && dungeonRank > 1)
            {
                return Math.Max(0.5f, Cooldown - (CooldownReductionPerRank * (dungeonRank - 1)));
            }
            return Cooldown;
        }

        public int GetScaledDamage(int lordLevel, int weaponDamage = 0)
        {
            return DamageScaling switch
            {
                DamageScaling.Weapon => BaseDamage + weaponDamage,
                DamageScaling.Level => BaseDamage + (lordLevel * 2),
                DamageScaling.None => BaseDamage,
                _ => BaseDamage
            };
        }

        public int GetScaledShieldHp(int lordLevel)
        {
            return ShieldHpScaling switch
            {
                DamageScaling.Level => ShieldHp + (lordLevel * 5),
                DamageScaling.None => ShieldHp,
                _ => ShieldHp
            };
        }
    }

    public enum DamageScaling
    {
        None,
        Weapon,
        Level
    }

    public static class AbilityDatabase
    {
        private static readonly Dictionary<string, AbilityData> _abilities = new();
        private static bool _loaded = false;

        public static event Action<string> OnAbilityUsed;
        public static event Action<string, float> OnCooldownStarted;
        public static event Action<string> OnCooldownEnded;

        public static void LoadFromJson(string path = "res://data/abilities.json")
        {
            if (_loaded) return;

            try
            {
                string jsonPath = ProjectSettings.GlobalizePath(path);
                if (!File.Exists(jsonPath))
                {
                    GD.PrintErr($"Ability database not found at {jsonPath}");
                    LoadDefaultAbilities();
                    return;
                }

                string json = File.ReadAllText(jsonPath);
                var abilities = JsonSerializer.Deserialize<List<AbilityData>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (abilities != null)
                {
                    _abilities.Clear();
                    foreach (var ability in abilities)
                    {
                        _abilities[ability.Id] = ability;
                    }
                    GD.Print($"Loaded {_abilities.Count} abilities from {path}");
                }
                else
                {
                    GD.PrintErr("Failed to deserialize abilities.json");
                    LoadDefaultAbilities();
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error loading abilities.json: {ex.Message}");
                LoadDefaultAbilities();
            }

            _loaded = true;
        }

        private static void LoadDefaultAbilities()
        {
            _abilities.Clear();

            AddAbility(new AbilityData
            {
                Id = "basic_attack",
                Name = "Basic Attack",
                Description = "A simple melee strike with your equipped weapon.",
                TypeString = "Active",
                CategoryString = "Combat",
                Cooldown = 0.8f,
                EssenceCost = 0,
                BaseDamage = 10,
                DamageScalingString = "Weapon",
                Range = 1,
                TargetTypeString = "SingleEnemy",
                UnlockLevel = 1,
                RequiredDungeonRank = 1,
                Icon = "res://assets/icons/abilities/basic_attack.png",
                Animation = "attack_basic",
                SoundEffect = "res://assets/sfx/attack_basic.wav"
            });

            AddAbility(new AbilityData
            {
                Id = "essence_strike",
                Name = "Essence Strike",
                Description = "Channel raw Essence into a devastating blow that ignores armor.",
                TypeString = "Active",
                CategoryString = "Combat",
                Cooldown = 6.0f,
                EssenceCost = 25,
                BaseDamage = 35,
                DamageScalingString = "Level",
                Range = 1,
                TargetTypeString = "SingleEnemy",
                UnlockLevel = 5,
                RequiredDungeonRank = 1,
                Icon = "res://assets/icons/abilities/essence_strike.png",
                Animation = "attack_essence",
                SoundEffect = "res://assets/sfx/essence_strike.wav",
                SpecialEffects = new List<string> { "armor_pierce", "essence_burn" }
            });

            AddAbility(new AbilityData
            {
                Id = "shadow_step",
                Name = "Shadow Step",
                Description = "Teleport behind a target within range, gaining a damage bonus on next attack.",
                TypeString = "Active",
                CategoryString = "Movement",
                Cooldown = 12.0f,
                EssenceCost = 30,
                BaseDamage = 0,
                DamageScalingString = "None",
                Range = 4,
                TargetTypeString = "EmptyTileNearEnemy",
                UnlockLevel = 10,
                RequiredDungeonRank = 2,
                Icon = "res://assets/icons/abilities/shadow_step.png",
                Animation = "shadow_step",
                SoundEffect = "res://assets/sfx/shadow_step.wav",
                SpecialEffects = new List<string> { "teleport", "next_attack_bonus_50" }
            });

            AddAbility(new AbilityData
            {
                Id = "dungeon_roar",
                Name = "Dungeon Roar",
                Description = "Unleash a terrifying roar that damages and fears all enemies in a cone.",
                TypeString = "Active",
                CategoryString = "Aoe",
                Cooldown = 18.0f,
                EssenceCost = 40,
                BaseDamage = 20,
                DamageScalingString = "Level",
                Range = 3,
                TargetTypeString = "ConeAoe",
                UnlockLevel = 15,
                RequiredDungeonRank = 2,
                Icon = "res://assets/icons/abilities/dungeon_roar.png",
                Animation = "roar",
                SoundEffect = "res://assets/sfx/dungeon_roar.wav",
                SpecialEffects = new List<string> { "fear", "knockback_1_tile" },
                ConeAngle = 90,
                ConeLength = 3
            });

            AddAbility(new AbilityData
            {
                Id = "demonic_shield",
                Name = "Demonic Shield",
                Description = "Manifest a shield of dark energy that absorbs damage and reflects melee attacks.",
                TypeString = "Active",
                CategoryString = "Defense",
                Cooldown = 20.0f,
                EssenceCost = 35,
                BaseDamage = 0,
                DamageScalingString = "None",
                Range = 0,
                TargetTypeString = "Self",
                UnlockLevel = 20,
                RequiredDungeonRank = 3,
                Icon = "res://assets/icons/abilities/demonic_shield.png",
                Animation = "shield_cast",
                SoundEffect = "res://assets/sfx/demonic_shield.wav",
                SpecialEffects = new List<string> { "damage_absorption", "melee_reflect", "duration_8s" },
                ShieldHp = 150,
                ShieldHpScalingString = "Level",
                ReflectPercentage = 50
            });

            AddAbility(new AbilityData
            {
                Id = "monster_possession",
                Name = "Monster Possession",
                Description = "Take direct control of a nearby garrisoned monster, seeing through its eyes and using its abilities.",
                TypeString = "Active",
                CategoryString = "Utility",
                Cooldown = 30.0f,
                EssenceCost = 50,
                BaseDamage = 0,
                DamageScalingString = "None",
                Range = 5,
                TargetTypeString = "FriendlyMonster",
                UnlockLevel = 8,
                RequiredDungeonRank = 1,
                Icon = "res://assets/icons/abilities/monster_possession.png",
                Animation = "possession",
                SoundEffect = "res://assets/sfx/possession.wav",
                SpecialEffects = new List<string> { "possession", "duration_15s", "lord_body_vulnerable" },
                MaxDuration = 15.0f,
                CooldownReductionPerRank = 2.0f
            });
        }

        public static void AddAbility(AbilityData ability)
        {
            if (ability != null && !string.IsNullOrEmpty(ability.Id))
            {
                _abilities[ability.Id] = ability;
            }
        }

        public static AbilityData GetAbility(string id)
        {
            return _abilities.TryGetValue(id, out var ability) ? ability : null;
        }

        public static List<AbilityData> GetAllAbilities()
        {
            return _abilities.Values.ToList();
        }

        public static List<AbilityData> GetUnlockedAbilities(int lordLevel, int dungeonRank)
        {
            return _abilities.Values
                .Where(a => a.UnlockLevel <= lordLevel && a.RequiredDungeonRank <= dungeonRank)
                .ToList();
        }

        public static List<AbilityData> GetAbilitiesByCategory(AbilityCategory category)
        {
            return _abilities.Values.Where(a => a.Category == category).ToList();
        }

        public static bool TryUseAbility(string abilityId, int lordLevel, int dungeonRank, ref long currentEssence, int weaponDamage = 0)
        {
            var ability = GetAbility(abilityId);
            if (ability == null) return false;

            if (!ability.CanUse(lordLevel, dungeonRank, currentEssence)) return false;

            // Spend essence
            currentEssence -= ability.EssenceCost;

            // Start cooldown
            ability.CurrentCooldown = ability.GetEffectiveCooldown(dungeonRank);
            OnCooldownStarted?.Invoke(abilityId, ability.CurrentCooldown);

            // Fire event
            OnAbilityUsed?.Invoke(abilityId);

            GD.Print($"Used ability: {ability.Name} (Essence cost: {ability.EssenceCost}, Cooldown: {ability.CurrentCooldown}s)");
            return true;
        }

        public static void UpdateCooldowns(float deltaTime)
        {
            var completedCooldowns = new List<string>();

            foreach (var ability in _abilities.Values)
            {
                if (ability.IsOnCooldown)
                {
                    ability.CurrentCooldown -= deltaTime;
                    if (ability.CurrentCooldown <= 0f)
                    {
                        ability.CurrentCooldown = 0f;
                        completedCooldowns.Add(ability.Id);
                    }
                }
            }

            foreach (var id in completedCooldowns)
            {
                OnCooldownEnded?.Invoke(id);
            }
        }

        public static float GetCooldownRemaining(string abilityId)
        {
            var ability = GetAbility(abilityId);
            return ability?.CurrentCooldown ?? 0f;
        }

        public static float GetCooldownProgress(string abilityId)
        {
            var ability = GetAbility(abilityId);
            if (ability == null || ability.Cooldown <= 0) return 1f;
            return 1f - (ability.CurrentCooldown / ability.Cooldown);
        }

        public static bool IsAbilityReady(string abilityId)
        {
            var ability = GetAbility(abilityId);
            return ability != null && !ability.IsOnCooldown;
        }

        public static void ResetAllCooldowns()
        {
            foreach (var ability in _abilities.Values)
            {
                ability.CurrentCooldown = 0f;
            }
        }

        public static void Reload(string path = "res://data/abilities.json")
        {
            _loaded = false;
            LoadFromJson(path);
        }

        public static int Count => _abilities.Count;
    }
}