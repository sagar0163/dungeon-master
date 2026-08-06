using System;
using System.Collections.Generic;

namespace DungeonLord.Scripts
{
    public enum TileType
    {
        Empty,
        Corridor,
        Room,
        Trap,
        SpawnPoint,
        LordChamber
    }

    public class DungeonTile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public TileType Type { get; set; } = TileType.Empty;
        public string RoomId { get; set; }
        public string TrapId { get; set; }
        public List<string> GarrisonedMonsters { get; set; } = new List<string>();
    }

    public class DungeonGrid
    {
        public int Width { get; }
        public int Height { get; }
        public int Floors { get; }
        private readonly DungeonTile[,,] _grid;

        public DungeonGrid(int width = 32, int height = 32, int floors = 3)
        {
            Width = width;
            Height = height;
            Floors = floors;
            _grid = new DungeonTile[width, height, floors];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < floors; z++)
                    {
                        _grid[x, y, z] = new DungeonTile { X = x, Y = y, Z = z };
                    }
                }
            }
        }

        public DungeonTile GetTile(int x, int y, int z = 0)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Floors)
                return null;
            return _grid[x, y, z];
        }

        public bool SetTileType(int x, int y, TileType type, int z = 0)
        {
            var tile = GetTile(x, y, z);
            if (tile == null) return false;
            tile.Type = type;
            return true;
        }
    }
}
