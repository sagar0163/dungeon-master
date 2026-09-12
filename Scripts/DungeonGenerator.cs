using System;
using System.Collections.Generic;
using System.Text;
using DungeonLord.Scripts.Config;

namespace DungeonLord.Scripts
{
    /// <summary>
    /// Deterministic PRNG used by dungeon generation. xorshift32 seeded via
    /// splitmix32 — bit-exact mirror of dungeon_master.generator.SeededRandom
    /// (Python), so the Python tooling and this C# generator produce identical
    /// dungeons for the same seed.
    /// </summary>
    public class SeededRandom
    {
        private uint _state;

        public SeededRandom(int seed)
        {
            uint state = SplitMix32((uint)seed);
            _state = state != 0 ? state : 0x9E3779B9u;
        }

        public uint NextU32()
        {
            uint x = _state;
            x ^= (x << 13);
            x ^= (x >> 17);
            x ^= (x << 5);
            _state = x;
            return _state;
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            int width = maxExclusive - minInclusive;
            if (width <= 0) return minInclusive;
            return minInclusive + (int)(NextU32() % (uint)width);
        }

        private static uint SplitMix32(uint state)
        {
            state += 0x9E3779B9u;
            state = (state ^ (state >> 16)) * 0x85EBCA6Bu;
            state = (state ^ (state >> 13)) * 0xC2B2AE35u;
            return state ^ (state >> 16);
        }
    }

    /// <summary>
    /// Seeded procedural dungeon generator: places connected rooms connected by
    /// corridors on a <see cref="DungeonGrid"/>. All randomness flows through a
    /// single <see cref="SeededRandom"/>; the same seed always yields the
    /// identical grid (NFR-4). All tunables come from <see cref="DungeonGenerationConfig"/>
    /// backed by data/dungeon_generation.toml — no hardcoded numbers.
    ///
    /// The algorithm mirrors dungeon_master/generator.py one-to-one.
    /// </summary>
    public class DungeonGenerator
    {
        private readonly DungeonGenerationConfig _config;

        public DungeonGenerator(DungeonGenerationConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public static DungeonGenerationConfig LoadConfig(string path)
        {
            return DungeonGenerationConfig.Load(path);
        }

        /// <summary>Generate a fresh grid of the configured size for the given seed.</summary>
        public DungeonGrid Generate(int seed)
        {
            var grid = new DungeonGrid(_config.Width, _config.Height, _config.Floors);
            Generate(seed, grid);
            return grid;
        }

        /// <summary>Fill an existing grid (cleared first) for the given seed.</summary>
        public void Generate(int seed, DungeonGrid grid)
        {
            var rng = new SeededRandom(seed);
            ClearGrid(grid);

            for (int z = 0; z < _config.Floors; z++)
            {
                var rooms = PlaceRooms(rng, z);
                foreach (var room in rooms)
                {
                    CarveRoom(rng, room, z, grid);
                }
                for (int i = 1; i < rooms.Count; i++)
                {
                    Connect(rng, rooms[i - 1], rooms[i], z, grid);
                }
            }
        }

        /// <summary>
        /// Canonical signature of a generated grid (position:type:roomId pairs).
        /// Identical grids have identical signatures; used to assert determinism.
        /// </summary>
        public string Signature(DungeonGrid grid)
        {
            var sb = new StringBuilder();
            for (int z = 0; z < grid.Floors; z++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    for (int x = 0; x < grid.Width; x++)
                    {
                        var tile = grid.GetTile(x, y, z);
                        if (tile == null || tile.Type == TileType.Empty) continue;

                        if (sb.Length > 0) sb.Append('|');
                        sb.Append(x).Append(',').Append(y).Append(',').Append(z).Append(':');
                        sb.Append(TileTypeName(tile.Type)).Append(':').Append(tile.RoomId ?? "");
                    }
                }
            }
            return sb.ToString();
        }

        private static string TileTypeName(TileType type)
        {
            return type switch
            {
                TileType.Room => "room",
                TileType.Corridor => "corridor",
                _ => type.ToString().ToLowerInvariant(),
            };
        }

        // -- room placement ------------------------------------------------

        private List<Room> PlaceRooms(SeededRandom rng, int floor)
        {
            var rooms = new List<Room>();
            int attempts = 0;

            while (attempts < _config.Rooms.PlacementAttempts && rooms.Count < _config.Rooms.TargetPerFloor)
            {
                attempts++;

                int w = rng.NextInt(_config.Rooms.MinWidth, _config.Rooms.MaxWidth + 1);
                int h = rng.NextInt(_config.Rooms.MinHeight, _config.Rooms.MaxHeight + 1);

                int xMax = _config.Width - _config.Rooms.Margin - w;
                int yMax = _config.Height - _config.Rooms.Margin - h;
                if (xMax < _config.Rooms.Margin || yMax < _config.Rooms.Margin) continue;

                int x = rng.NextInt(_config.Rooms.Margin, xMax + 1);
                int y = rng.NextInt(_config.Rooms.Margin, yMax + 1);

                var candidate = new Room(x, y, w, h);
                bool overlaps = false;
                foreach (var other in rooms)
                {
                    if (Overlaps(candidate, other)) { overlaps = true; break; }
                }
                if (overlaps) continue;

                rooms.Add(candidate);
            }

            rooms.Sort((a, b) =>
            {
                int byY = a.Y.CompareTo(b.Y);
                return byY != 0 ? byY : a.X.CompareTo(b.X);
            });

            return rooms;
        }

        private bool Overlaps(Room a, Room b)
        {
            int spacing = _config.Rooms.Spacing;
            return a.X - spacing < b.X + b.W
                && b.X - spacing < a.X + a.W
                && a.Y - spacing < b.Y + b.H
                && b.Y - spacing < a.Y + a.H;
        }

        // -- carving -------------------------------------------------------

        private void CarveRoom(SeededRandom rng, Room room, int floor, DungeonGrid grid)
        {
            string roomId = PickWeighted(rng, _config.RoomWeights);
            for (int dx = 0; dx < room.W; dx++)
            {
                for (int dy = 0; dy < room.H; dy++)
                {
                    int x = room.X + dx;
                    int y = room.Y + dy;
                    grid.SetTileType(x, y, TileType.Room, floor);
                    var tile = grid.GetTile(x, y, floor);
                    if (tile != null) tile.RoomId = roomId;
                }
            }
        }

        private void Connect(SeededRandom rng, Room a, Room b, int floor, DungeonGrid grid)
        {
            int ax = a.X + a.W / 2;
            int ay = a.Y + a.H / 2;
            int bx = b.X + b.W / 2;
            int by = b.Y + b.H / 2;
            int corridorWidth = Math.Max(1, _config.Corridors.Width);

            bool horizontalFirst = (rng.NextU32() & 1) == 0;
            if (horizontalFirst)
            {
                CarveHLine(ay, ax, bx, corridorWidth, floor, grid);
                CarveVLine(bx, ay, by, corridorWidth, floor, grid);
            }
            else
            {
                CarveVLine(ax, ay, by, corridorWidth, floor, grid);
                CarveHLine(by, ax, bx, corridorWidth, floor, grid);
            }
        }

        private void CarveHLine(int row, int x0, int x1, int corridorWidth, int floor, DungeonGrid grid)
        {
            int lo = Math.Min(x0, x1);
            int hi = Math.Max(x0, x1);
            int offset = -(corridorWidth / 2);
            for (int dx = lo; dx <= hi; dx++)
            {
                for (int d = 0; d < corridorWidth; d++)
                {
                    SetCorridor(dx, row + offset + d, floor, grid);
                }
            }
        }

        private void CarveVLine(int col, int y0, int y1, int corridorWidth, int floor, DungeonGrid grid)
        {
            int lo = Math.Min(y0, y1);
            int hi = Math.Max(y0, y1);
            int offset = -(corridorWidth / 2);
            for (int dy = lo; dy <= hi; dy++)
            {
                for (int d = 0; d < corridorWidth; d++)
                {
                    SetCorridor(col + offset + d, dy, floor, grid);
                }
            }
        }

        private void SetCorridor(int x, int y, int floor, DungeonGrid grid)
        {
            if (x < 0 || x >= _config.Width || y < 0 || y >= _config.Height) return;

            var tile = grid.GetTile(x, y, floor);
            if (tile == null || tile.Type == TileType.Room) return; // never downgrade rooms

            grid.SetTileType(x, y, TileType.Corridor, floor);
        }

        // -- helpers -------------------------------------------------------

        private static string PickWeighted(SeededRandom rng, List<KeyValuePair<string, int>> weights)
        {
            long total = 0;
            foreach (var kv in weights) total += kv.Value;

            if (weights.Count == 0) return "spawn";
            if (total <= 0) return weights[weights.Count - 1].Key;

            uint roll = rng.NextU32() % (uint)total;
            long acc = 0;
            foreach (var kv in weights)
            {
                acc += kv.Value;
                if ((long)roll < acc) return kv.Key;
            }
            return weights[weights.Count - 1].Key;
        }

        private void ClearGrid(DungeonGrid grid)
        {
            for (int z = 0; z < grid.Floors; z++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    for (int y = 0; y < grid.Height; y++)
                    {
                        grid.SetTileType(x, y, TileType.Empty, z);
                    }
                }
            }
        }

        private readonly struct Room
        {
            public readonly int X;
            public readonly int Y;
            public readonly int W;
            public readonly int H;

            public Room(int x, int y, int w, int h)
            {
                X = x;
                Y = y;
                W = w;
                H = h;
            }
        }
    }
}