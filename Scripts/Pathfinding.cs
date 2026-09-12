using System;
using System.Collections.Generic;

namespace DungeonLord.Scripts
{
    /// <summary>
    /// Godot-free A* pathfinding on the dungeon grid. Extracted from InvaderAI so the
    /// engine math can be unit-tested outside the game runtime. Mirrors the Python
    /// reference implementation in tests/test_grid.py (same heuristic and walkability).
    /// </summary>
    public static class Pathfinding
    {
        public readonly struct Cell : IEquatable<Cell>
        {
            public int X { get; }
            public int Y { get; }
            public int Z { get; }

            public Cell(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public bool Equals(Cell other) => X == other.X && Y == other.Y && Z == other.Z;
            public override bool Equals(object obj) => obj is Cell other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(X, Y, Z);
            public override string ToString() => $"({X}, {Y}, {Z})";
        }

        public static List<Cell> FindPath(DungeonGrid grid, Cell start, Cell goal)
        {
            if (grid == null) return new List<Cell>();

            var openSet = new PriorityQueue<Cell, float>();
            var cameFrom = new Dictionary<Cell, Cell>();
            var gScore = new Dictionary<Cell, float>();
            var fScore = new Dictionary<Cell, float>();

            openSet.Enqueue(start, Heuristic(start, goal));
            gScore[start] = 0f;
            fScore[start] = Heuristic(start, goal);

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current.Equals(goal))
                    return ReconstructPath(cameFrom, current);

                foreach (var neighbor in GetWalkableNeighbors(grid, current))
                {
                    float tentativeG = gScore[current] + 1f;

                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                        if (!openSet.Contains(n => n.Equals(neighbor)))
                            openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }

            return new List<Cell>(); // No path found
        }

        public static float Heuristic(Cell a, Cell b)
        {
            // Manhattan distance + floor penalty
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z) * 10f;
        }

        public static IEnumerable<Cell> GetWalkableNeighbors(DungeonGrid grid, Cell pos)
        {
            // 4-directional + floor connections (stairs)
            var dirs = new[]
            {
                new Cell(1, 0, 0), new Cell(-1, 0, 0),
                new Cell(0, 1, 0), new Cell(0, -1, 0)
            };

            foreach (var dir in dirs)
            {
                var next = new Cell(pos.X + dir.X, pos.Y + dir.Y, pos.Z + dir.Z);
                var tile = grid.GetTile(next.X, next.Y, next.Z);
                if (tile != null && IsWalkable(tile.Type))
                    yield return next;
            }

            // Floor transitions (stairs/ladders)
            var tileHere = grid.GetTile(pos.X, pos.Y, pos.Z);
            if (tileHere?.Type == TileType.Corridor || tileHere?.Type == TileType.Room)
            {
                // Check floor above
                if (pos.Z + 1 < grid.Floors)
                {
                    var up = grid.GetTile(pos.X, pos.Y, pos.Z + 1);
                    if (up != null && IsWalkable(up.Type))
                        yield return new Cell(pos.X, pos.Y, pos.Z + 1);
                }
                // Check floor below
                if (pos.Z - 1 >= 0)
                {
                    var down = grid.GetTile(pos.X, pos.Y, pos.Z - 1);
                    if (down != null && IsWalkable(down.Type))
                        yield return new Cell(pos.X, pos.Y, pos.Z - 1);
                }
            }
        }

        public static bool IsWalkable(TileType type)
        {
            return type == TileType.Corridor
                || type == TileType.Room
                || type == TileType.SpawnPoint
                || type == TileType.LordChamber;
        }

        private static List<Cell> ReconstructPath(Dictionary<Cell, Cell> cameFrom, Cell current)
        {
            var path = new List<Cell> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }
    }

    internal sealed class PriorityQueue<T, TPriority> where TPriority : IComparable<TPriority>
    {
        private readonly List<(T item, TPriority priority)> _items = new();

        public int Count => _items.Count;

        public void Enqueue(T item, TPriority priority)
        {
            _items.Add((item, priority));
            _items.Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        public T Dequeue()
        {
            var item = _items[0].item;
            _items.RemoveAt(0);
            return item;
        }

        public bool Contains(Func<T, bool> predicate)
        {
            return _items.Exists(x => predicate(x.item));
        }
    }
}