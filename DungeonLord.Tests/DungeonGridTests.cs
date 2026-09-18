using NUnit.Framework;
using DungeonLord.Scripts;

namespace DungeonLord.Tests
{
    /// <summary>
    /// Mirrors tests/test_grid.py (TestGrid3D + structure invariants) for the C# DungeonGrid.
    /// </summary>
    [TestFixture]
    public class DungeonGridTests
    {
        [Test]
        public void GridCreation_Dimensions()
        {
            var grid = new DungeonGrid(10, 10, 3);
            Assert.That(grid.Width, Is.EqualTo(10));
            Assert.That(grid.Height, Is.EqualTo(10));
            Assert.That(grid.Floors, Is.EqualTo(3));
        }

        [Test]
        public void DefaultConstruction_32By32By3()
        {
            var grid = new DungeonGrid();
            Assert.That(grid.Width, Is.EqualTo(32));
            Assert.That(grid.Height, Is.EqualTo(32));
            Assert.That(grid.Floors, Is.EqualTo(3));
        }

        [Test]
        public void SetAndGetTile_Coordinates()
        {
            var grid = new DungeonGrid(5, 5, 2);
            Assert.That(grid.SetTileType(2, 2, TileType.Room), Is.True);
            Assert.That(grid.SetTileType(3, 2, TileType.Corridor), Is.True);

            var room = grid.GetTile(2, 2, 0);
            Assert.That(room, Is.Not.Null);
            Assert.That(room.X, Is.EqualTo(2));
            Assert.That(room.Y, Is.EqualTo(2));
            Assert.That(room.Z, Is.EqualTo(0));
            Assert.That(room.Type, Is.EqualTo(TileType.Room));

            Assert.That(grid.GetTile(3, 2, 0).Type, Is.EqualTo(TileType.Corridor));
        }

        [Test]
        public void DefaultTile_EmptyAndGarrisonListPresent()
        {
            var grid = new DungeonGrid(5, 5, 2);
            Assert.That(grid.GetTile(0, 0, 0).Type, Is.EqualTo(TileType.Empty));
            Assert.That(grid.GetTile(0, 0, 0).GarrisonedMonsters, Is.Empty);
        }

        [Test]
        public void BoundsChecking_OutOfRange_ReturnsNull()
        {
            var grid = new DungeonGrid(5, 5, 2);
            Assert.That(grid.GetTile(5, 0, 0), Is.Null);
            Assert.That(grid.GetTile(0, 5, 0), Is.Null);
            Assert.That(grid.GetTile(0, 0, 2), Is.Null);
            Assert.That(grid.GetTile(-1, 0, 0), Is.Null);
            Assert.That(grid.GetTile(4, 4, 1), Is.Not.Null);
        }

        [Test]
        public void SetTileType_OutOfRange_ReturnsFalse()
        {
            var grid = new DungeonGrid(5, 5, 2);
            Assert.That(grid.SetTileType(10, 10, TileType.Room), Is.False);
            Assert.That(grid.SetTileType(10, 10, TileType.Room, 10), Is.False);
            Assert.That(grid.GetTile(0, 0, 0).Type, Is.EqualTo(TileType.Empty));
        }

        [Test]
        public void SetTileType_OnSpecificFloor()
        {
            var grid = new DungeonGrid(5, 5, 2);
            Assert.That(grid.SetTileType(1, 1, TileType.LordChamber, 1), Is.True);
            Assert.That(grid.GetTile(1, 1, 0).Type, Is.EqualTo(TileType.Empty));
            Assert.That(grid.GetTile(1, 1, 1).Type, Is.EqualTo(TileType.LordChamber));
        }

        [Test]
        public void Entrance_MustExistOnFloor0()
        {
            var grid = new DungeonGrid(10, 10, 3);
            grid.SetTileType(5, 0, TileType.SpawnPoint);

            var entrance = FindByType(grid, TileType.SpawnPoint);
            Assert.That(entrance, Is.Not.Null);
            Assert.That(entrance.Value.Item3, Is.EqualTo(0));
        }

        [Test]
        public void Core_Exists()
        {
            var grid = new DungeonGrid(10, 10, 3);
            grid.SetTileType(5, 9, TileType.LordChamber, 2);

            Assert.That(FindByType(grid, TileType.LordChamber), Is.Not.Null);
        }

        private static (int, int, int)? FindByType(DungeonGrid grid, TileType type)
        {
            for (int z = 0; z < grid.Floors; z++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    for (int y = 0; y < grid.Height; y++)
                    {
                        if (grid.GetTile(x, y, z)?.Type == type)
                            return (x, y, z);
                    }
                }
            }
            return null;
        }
    }
}