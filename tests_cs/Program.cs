using System;
using System.Collections.Generic;
using System.IO;
using DungeonLord.Scripts;
using DungeonLord.Scripts.Config;

namespace DungeonLord.Tests
{
    /// <summary>
    /// Headless C# harness that mirrors the Python generator tests
    /// (tests/test_generator.py). Same RNG, same algorithm, same TOML config,
    /// so both layers must agree on seed determinism.
    ///
    /// Usage:
    ///   DungeonLord.Tests                -> run assertions (nonzero exit on failure)
    ///   DungeonLord.Tests --signature N  -> print canonical grid signature for seed N
    ///      (used by the Python cross-layer agreement test)
    /// </summary>
    internal static class Program
    {
        private static int _checks;
        private static int _failures;

        private static void Check(bool condition, string name)
        {
            _checks++;
            if (condition)
            {
                Console.WriteLine($"  ok: {name}");
            }
            else
            {
                _failures++;
                Console.WriteLine($"  FAIL: {name}");
            }
        }

        private static string FindConfigPath()
        {
            // Run from the repo root (dotnet run --project tests_cs) or from tests_cs/.
            foreach (var candidate in new[] { "data/dungeon_generation.toml", "../data/dungeon_generation.toml" })
            {
                if (File.Exists(candidate)) return candidate;
            }
            throw new FileNotFoundException("data/dungeon_generation.toml not found");
        }

        private static int Main(string[] args)
        {
            var config = DungeonGenerationConfig.Load(FindConfigPath());
            var generator = new DungeonGenerator(config);

            if (args.Length >= 2 && args[0] == "--signature")
            {
                int seed = int.Parse(args[1]);
                Console.WriteLine("SIG:" + generator.Signature(generator.Generate(seed)));
                return 0;
            }

            Console.WriteLine("DungeonLord.Tests - C# seeded dungeon generator");
            TestTomlConfig(config);
            TestSeedDeterminism(generator);
            TestConnectivity(generator);
            TestWithinMargin(generator);

            Console.WriteLine($"\n{_checks - _failures}/{_checks} checks passed");
            Console.WriteLine(_failures == 0 ? "ALL CHECKS PASSED" : "CHECKS FAILED");
            return _failures == 0 ? 0 : 1;
        }

        private static void TestTomlConfig(DungeonGenerationConfig config)
        {
            Console.WriteLine("[TOML config]");
            Check(config.Width == 32, "width == 32");
            Check(config.Height == 32, "height == 32");
            Check(config.Floors == 3, "floors == 3");
            Check(config.DefaultSeed == 1337, "default_seed == 1337");

            Check(config.Rooms.MinWidth == 3, "rooms.min_width == 3");
            Check(config.Rooms.MaxWidth == 8, "rooms.max_width == 8");
            Check(config.Rooms.MinHeight == 3, "rooms.min_height == 3");
            Check(config.Rooms.MaxHeight == 8, "rooms.max_height == 8");
            Check(config.Rooms.TargetPerFloor == 8, "rooms.target_per_floor == 8");
            Check(config.Rooms.PlacementAttempts == 200, "rooms.placement_attempts == 200");
            Check(config.Rooms.Margin == 2, "rooms.margin == 2");
            Check(config.Rooms.Spacing == 1, "rooms.spacing == 1");

            Check(config.Corridors.Width == 1, "corridors.width == 1");

            Check(config.RoomWeights.Count == 6, "room weights: 6 entries");
            Check(RoomWeight(config, "spawn") == 3, "weights[spawn] == 3");
            Check(RoomWeight(config, "treasure") == 2, "weights[treasure] == 2");
            Check(RoomWeight(config, "barracks") == 2, "weights[barracks] == 2");
            Check(RoomWeight(config, "shrine") == 1, "weights[shrine] == 1");
            Check(RoomWeight(config, "trap_room") == 1, "weights[trap_room] == 1");
            Check(RoomWeight(config, "boss") == 1, "weights[boss] == 1");
        }

        private static int RoomWeight(DungeonGenerationConfig config, string id)
        {
            foreach (var kv in config.RoomWeights)
            {
                if (kv.Key == id) return kv.Value;
            }
            return -1;
        }

        private static void TestSeedDeterminism(DungeonGenerator generator)
        {
            Console.WriteLine("[seed determinism]");
            Check(generator.Signature(generator.Generate(1337)) == generator.Signature(generator.Generate(1337)),
                "same seed 1337 -> identical grid");
            Check(generator.Signature(generator.Generate(42)) == generator.Signature(generator.Generate(42)),
                "same seed 42 -> identical grid");
            Check(generator.Signature(generator.Generate(12345)) != generator.Signature(generator.Generate(54321)),
                "different seeds -> different grids");

            var grid42 = generator.Generate(42);
            Check(grid42.GetTile(0, 0, 0).Type == TileType.Empty, "corner tiles stay empty (margin)");
        }

        private static void TestConnectivity(DungeonGenerator generator)
        {
            Console.WriteLine("[connectivity]");
            foreach (var seed in new[] { 1, 1337, 2024, 99991 })
            {
                var grid = generator.Generate(seed);
                for (int z = 0; z < grid.Floors; z++)
                {
                    Check(FloorConnected(grid, z), $"seed {seed} floor {z} fully connected");
                }

                int rooms = CountTiles(grid, TileType.Room);
                int corridors = CountTiles(grid, TileType.Corridor);
                Check(rooms > 0, $"seed {seed}: rooms placed ({rooms})");
                Check(corridors > 0, $"seed {seed}: corridors carved ({corridors})");
            }
        }

        private static void TestWithinMargin(DungeonGenerator generator)
        {
            Console.WriteLine("[room margin]");
            var grid = generator.Generate(1337);
            const int margin = 2;
            bool allWithin = true;
            for (int z = 0; z < grid.Floors && allWithin; z++)
            {
                for (int x = 0; x < grid.Width && allWithin; x++)
                {
                    for (int y = 0; y < grid.Height && allWithin; y++)
                    {
                        var tile = grid.GetTile(x, y, z);
                        if (tile != null && tile.Type == TileType.Room)
                        {
                            bool within = x >= margin && y >= margin && x < grid.Width - margin && y < grid.Height - margin;
                            if (!within) allWithin = false;
                        }
                    }
                }
            }
            Check(allWithin, "all rooms respect the configured margin");
        }

        private static int CountTiles(DungeonGrid grid, TileType type)
        {
            int count = 0;
            for (int z = 0; z < grid.Floors; z++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    for (int y = 0; y < grid.Height; y++)
                    {
                        if (grid.GetTile(x, y, z).Type == type) count++;
                    }
                }
            }
            return count;
        }

        private static bool FloorConnected(DungeonGrid grid, int floor)
        {
            var walkable = new HashSet<(int, int)>();
            (int, int)? start = null;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var tile = grid.GetTile(x, y, floor);
                    if (tile != null && (tile.Type == TileType.Room || tile.Type == TileType.Corridor))
                    {
                        walkable.Add((x, y));
                        if (start == null) start = (x, y);
                    }
                }
            }

            if (start == null) return true; // trivially connected empty floor

            var seen = new HashSet<(int, int)> { start.Value };
            var queue = new Queue<(int, int)>();
            queue.Enqueue(start.Value);

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                foreach (var neighbor in new[] { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) })
                {
                    if (walkable.Contains(neighbor) && !seen.Contains(neighbor))
                    {
                        seen.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return seen.SetEquals(walkable);
        }
    }
}