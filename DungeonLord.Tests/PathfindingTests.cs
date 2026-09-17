using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using DungeonLord.Scripts;

namespace DungeonLord.Tests
{
    /// <summary>
    /// Mirrors tests/test_grid.py (TestAStarPathfinding + reachability invariant) against
    /// the C# Pathfinding engine extracted from InvaderAI.
    /// </summary>
    [TestFixture]
    public class PathfindingTests
    {
        private static List<(int x, int y, int z)> Path(
            DungeonGrid grid, int sx, int sy, int sz, int gx, int gy, int gz)
        {
            return Pathfinding.FindPath(
                    grid, new Pathfinding.Cell(sx, sy, sz), new Pathfinding.Cell(gx, gy, gz))
                .Select(c => (c.X, c.Y, c.Z))
                .ToList();
        }

        [Test]
        public void SimpleStraightPath()
        {
            // Corridor along y=1 from x=0 to x=9.
            var grid = new DungeonGrid(10, 3, 1);
            for (int x = 0; x < 10; x++)
                grid.SetTileType(x, 1, TileType.Corridor);

            var path = Path(grid, 0, 1, 0, 9, 1, 0);
            Assert.That(path, Has.Count.EqualTo(10));
            Assert.That(path[0].x, Is.EqualTo(0));
            Assert.That(path[0].y, Is.EqualTo(1));
            Assert.That(path[^1].x, Is.EqualTo(9));
            Assert.That(path[^1].y, Is.EqualTo(1));
        }

        [Test]
        public void PathAvoidsObstacle()
        {
            // Ring of corridors around an empty (non-walkable) center cell.
            var grid = new DungeonGrid(5, 5, 1);
            for (int x = 0; x < 5; x++)
            {
                grid.SetTileType(x, 1, TileType.Corridor);
                grid.SetTileType(x, 3, TileType.Corridor);
            }
            for (int y = 1; y <= 3; y++)
            {
                grid.SetTileType(1, y, TileType.Corridor);
                grid.SetTileType(3, y, TileType.Corridor);
            }

            var path = Path(grid, 0, 1, 0, 4, 1, 0);
            Assert.That(path, Is.Not.Empty);
            Assert.That(path.All(c => c != (2, 2, 0)), Is.True, "path must avoid the obstacle cell");
        }

        [Test]
        public void MultiFloorPath_ViaStairs()
        {
            // Floor 0: full corridor. Floor 1: corridor reached only via the (2,2) stairs,
            // so the path is forced through the stair column (mirrors 5x5 floor-1 corridor
            // + stairs at (2,2) from test_grid.py's multi-floor case).
            var grid = new DungeonGrid(5, 5, 2);
            for (int x = 0; x < 5; x++)
                grid.SetTileType(x, 2, TileType.Corridor, 0);
            for (int x = 2; x < 5; x++)
                grid.SetTileType(x, 2, TileType.Corridor, 1);

            var path = Path(grid, 0, 2, 0, 4, 2, 1);
            Assert.That(path, Is.Not.Empty);
            Assert.That(path.Any(c => c == (2, 2, 0)), Is.True, "path must pass the stairs on floor 0");
            Assert.That(path.Any(c => c == (2, 2, 1)), Is.True, "path must pass the stairs on floor 1");
        }

        [Test]
        public void NoPath_ReturnsEmptyList()
        {
            // Two isolated rooms with no connecting walkable cells.
            var grid = new DungeonGrid(5, 5, 1);
            grid.SetTileType(0, 0, TileType.Room);
            grid.SetTileType(4, 4, TileType.Room);

            Assert.That(Path(grid, 0, 0, 0, 4, 4, 0), Is.Empty);
        }

        [Test]
        public void WalkableNeighbors_4Directional()
        {
            var grid = new DungeonGrid(5, 5, 1);
            grid.SetTileType(2, 2, TileType.Corridor);
            grid.SetTileType(3, 2, TileType.Corridor);
            grid.SetTileType(2, 3, TileType.Room);

            var center = new Pathfinding.Cell(2, 2, 0);
            var neighbors = Pathfinding.GetWalkableNeighbors(grid, center).ToList();

            Assert.That(neighbors, Does.Contain(new Pathfinding.Cell(3, 2, 0)));
            Assert.That(neighbors, Does.Contain(new Pathfinding.Cell(2, 3, 0)));
            Assert.That(neighbors.Any(n => n.X == 1 && n.Y == 2), Is.False, "empty cell must not be walkable");
            Assert.That(neighbors.Any(n => n.X == 2 && n.Y == 1), Is.False, "empty cell must not be walkable");
        }

        [Test]
        public void IsWalkable_OnlyTraversableTypes()
        {
            Assert.That(Pathfinding.IsWalkable(TileType.Corridor), Is.True);
            Assert.That(Pathfinding.IsWalkable(TileType.Room), Is.True);
            Assert.That(Pathfinding.IsWalkable(TileType.SpawnPoint), Is.True);
            Assert.That(Pathfinding.IsWalkable(TileType.LordChamber), Is.True);
            Assert.That(Pathfinding.IsWalkable(TileType.Trap), Is.True);
            Assert.That(Pathfinding.IsWalkable(TileType.Empty), Is.False);
        }

        [Test]
        public void Heuristic_ManhattanPlusFloorPenalty()
        {
            Assert.That(Pathfinding.Heuristic(new Pathfinding.Cell(0, 1, 0), new Pathfinding.Cell(9, 1, 0)), Is.EqualTo(9f));
            Assert.That(Pathfinding.Heuristic(new Pathfinding.Cell(0, 2, 0), new Pathfinding.Cell(4, 2, 1)), Is.EqualTo(4f + 10f));
        }

        [Test]
        public void AllWalkableCells_ReachableFromEntrance()
        {
            var grid = new DungeonGrid(5, 5, 1);
            grid.SetTileType(0, 0, TileType.SpawnPoint);
            grid.SetTileType(1, 0, TileType.Corridor);
            grid.SetTileType(2, 0, TileType.Room);

            var walkable = new List<(int, int, int)> { (0, 0, 0), (1, 0, 0), (2, 0, 0) };

            var visited = new HashSet<(int, int, int)> { (0, 0, 0) };
            var queue = new Queue<(int, int, int)>();
            queue.Enqueue((0, 0, 0));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var cell in Pathfinding.GetWalkableNeighbors(grid, new Pathfinding.Cell(current.Item1, current.Item2, current.Item3)))
                {
                    var key = (cell.X, cell.Y, cell.Z);
                    if (visited.Add(key))
                        queue.Enqueue(key);
                }
            }

            foreach (var cell in walkable)
                Assert.That(visited.Contains(cell), Is.True, $"cell {cell} must be reachable from entrance");
        }
    }
}