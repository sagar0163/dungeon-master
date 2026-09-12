using System;
using System.Collections.Generic;
using System.Globalization;

namespace DungeonLord.Scripts.Config
{
    /// <summary>
    /// Room placement parameters, loaded from TOML (data/dungeon_generation.toml).
    /// </summary>
    public class RoomConfig
    {
        public int MinWidth { get; set; }
        public int MaxWidth { get; set; }
        public int MinHeight { get; set; }
        public int MaxHeight { get; set; }
        public int TargetPerFloor { get; set; }
        public int PlacementAttempts { get; set; }
        public int Margin { get; set; }
        public int Spacing { get; set; }
    }

    /// <summary>
    /// Corridor parameters, loaded from TOML.
    /// </summary>
    public class CorridorConfig
    {
        public int Width { get; set; }
    }

    /// <summary>
    /// Bearer of all procedural-dungeon-generation parameters. The only source of
    /// tunables — no generation numbers are hardcoded in DungeonGenerator.
    /// </summary>
    public class DungeonGenerationConfig
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Floors { get; set; }
        public int DefaultSeed { get; set; }
        public RoomConfig Rooms { get; set; } = new RoomConfig();
        public CorridorConfig Corridors { get; set; } = new CorridorConfig();

        /// <summary>Ordered room-type pool (id -> weight) for tagging generated rooms.</summary>
        public List<KeyValuePair<string, int>> RoomWeights { get; } = new();

        public static DungeonGenerationConfig Load(string path)
        {
            var sections = TomlFile.ParseFile(path);
            var gen = sections["generator"];
            var rooms = sections["rooms"];
            var corr = sections["corridors"];

            var config = new DungeonGenerationConfig
            {
                Width = GetInt(gen, "width"),
                Height = GetInt(gen, "height"),
                Floors = GetInt(gen, "floors"),
                DefaultSeed = GetInt(gen, "default_seed"),
                Rooms = new RoomConfig
                {
                    MinWidth = GetInt(rooms, "min_width"),
                    MaxWidth = GetInt(rooms, "max_width"),
                    MinHeight = GetInt(rooms, "min_height"),
                    MaxHeight = GetInt(rooms, "max_height"),
                    TargetPerFloor = GetInt(rooms, "target_per_floor"),
                    PlacementAttempts = GetInt(rooms, "placement_attempts"),
                    Margin = GetInt(rooms, "margin"),
                    Spacing = GetInt(rooms, "spacing"),
                },
                Corridors = new CorridorConfig { Width = GetInt(corr, "width") },
            };

            if (sections.TryGetValue("rooms.weights", out var weights))
            {
                foreach (var pair in weights.Pairs)
                {
                    config.RoomWeights.Add(new KeyValuePair<string, int>(pair.Key, int.Parse(pair.Value, CultureInfo.InvariantCulture)));
                }
            }

            return config;
        }

        private static int GetInt(TomlSection section, string key)
        {
            string value = section?.Get(key);
            return value != null ? int.Parse(value, CultureInfo.InvariantCulture) : 0;
        }
    }
}