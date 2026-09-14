using System.Collections.Generic;
using Godot;

namespace DungeonLord.Scripts
{
    /// <summary>
    /// Renders the shared DungeonGrid as 3D tiles so the same structure is visible
    /// both top-down (Builder) and first-person (Crawl) - single data model.
    /// </summary>
    [GlobalClass]
    public partial class GridVisual : Node3D
    {
        private DungeonGrid _grid;
        private float _tileSize = 2.0f;
        private readonly Node3D _container = new Node3D { Name = "Tiles" };
        private readonly Dictionary<TileType, StandardMaterial3D> _materials = new();

        public override void _Ready()
        {
            AddChild(_container);
        }

        public void Initialize(DungeonGrid grid, float tileSize = 2.0f)
        {
            _grid = grid;
            _tileSize = tileSize;
        }

        public void Rebuild()
        {
            foreach (Node child in _container.GetChildren())
            {
                _container.RemoveChild(child);
                child.QueueFree();
            }

            if (_grid == null) return;

            var ground = new MeshInstance3D { Name = "Ground" };
            ground.Mesh = new PlaneMesh
            {
                Size = new Vector2(_grid.Width * _tileSize, _grid.Height * _tileSize)
            };
            ground.Position = new Vector3(_grid.Width * _tileSize / 2f, -0.03f, _grid.Height * _tileSize / 2f);
            _container.AddChild(ground);

            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    var tile = _grid.GetTile(x, y, 0);
                    if (tile == null) continue;

                    Vector3 center = new Vector3(x * _tileSize + _tileSize / 2f, 0, y * _tileSize + _tileSize / 2f);

                    var floorLid = new MeshInstance3D();
                    floorLid.Mesh = new BoxMesh { Size = new Vector3(_tileSize * 0.98f, 0.06f, _tileSize * 0.98f) };
                    floorLid.MaterialOverride = MaterialFor(TileType.Empty);
                    floorLid.Position = center + new Vector3(0, 0.03f, 0);
                    _container.AddChild(floorLid);

                    if (tile.Type == TileType.Empty) continue;

                    var block = new MeshInstance3D();
                    block.Mesh = new BoxMesh { Size = new Vector3(_tileSize * 0.95f, 1.6f, _tileSize * 0.95f) };
                    block.MaterialOverride = MaterialFor(tile.Type);
                    block.Position = center + new Vector3(0, 0.8f, 0);
                    _container.AddChild(block);
                }
            }
        }

        private StandardMaterial3D MaterialFor(TileType type)
        {
            if (!_materials.TryGetValue(type, out var mat))
            {
                mat = new StandardMaterial3D { AlbedoColor = ColorFor(type) };
                _materials[type] = mat;
            }
            return mat;
        }

        private static Color ColorFor(TileType type)
        {
            return type switch
            {
                TileType.Room => new Color(0.30f, 0.60f, 0.35f),
                TileType.Trap => new Color(0.80f, 0.25f, 0.20f),
                TileType.SpawnPoint => new Color(0.25f, 0.40f, 0.80f),
                TileType.Corridor => new Color(0.55f, 0.45f, 0.35f),
                TileType.LordChamber => new Color(0.85f, 0.80f, 0.20f),
                _ => new Color(0.25f, 0.25f, 0.27f)
            };
        }
    }
}