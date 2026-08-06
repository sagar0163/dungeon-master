using System;
using Godot;

namespace DungeonLord.Scripts
{
    /// <summary>
    /// Top-down/isometric Builder Mode controller.
    /// Handles tile selection, room/trap placement, camera controls.
    /// </summary>
    [GlobalClass]
    public partial class BuilderController : Node3D
    {
        // Configuration
        [Export] public float CameraMoveSpeed { get; set; } = 20.0f;
        [Export] public float CameraZoomSpeed { get; set; } = 10.0f;
        [Export] public float MinZoom { get; set; } = 5.0f;
        [Export] public float MaxZoom { get; set; } = 50.0f;
        [Export] public float TileSize { get; set; } = 2.0f;
        [Export] public int GridWidth { get; set; } = 32;
        [Export] public int GridHeight { get; set; } = 32;
        [Export] public int MaxFloors { get; set; } = 3;

        // Components
        [Export] public Camera3D Camera { get; private set; }
        [Export] public MeshInstance3D SelectionIndicator { get; private set; }
        [Export] public MeshInstance3D PlacementPreview { get; private set; }

        // State
        private DungeonGrid _dungeonGrid;
        private EssenceManager _essenceManager;
        private Vector3I _hoveredTile = new Vector3I(-1, -1, 0);
        private Vector3I _selectedTile = new Vector3I(-1, -1, 0);
        private int _currentFloor = 0;
        private BuildTool _currentTool = BuildTool.Select;
        private string _selectedRoomType = "corridor";
        private string _selectedTrapType = "";
        private bool _isPlacing = false;
        private bool _isPanning = false;
        private Vector2 _lastMousePos;

        // Input actions
        private const string ACTION_CAMERA_PAN = "builder_pan";
        private const string ACTION_CAMERA_ZOOM_IN = "builder_zoom_in";
        private const string ACTION_CAMERA_ZOOM_OUT = "builder_zoom_out";
        private const string ACTION_FLOOR_UP = "builder_floor_up";
        private const string ACTION_FLOOR_DOWN = "builder_floor_down";
        private const string ACTION_PLACE = "builder_place";
        private const string ACTION_CANCEL = "builder_cancel";
        private const string ACTION_SWITCH_MODE = "builder_switch_mode";
        private const string ACTION_TOOL_SELECT = "builder_tool_select";
        private const string ACTION_TOOL_ROOM = "builder_tool_room";
        private const string ACTION_TOOL_TRAP = "builder_tool_trap";
        private const string ACTION_TOOL_SPAWN = "builder_tool_spawn";
        private const string ACTION_TOOL_DELETE = "builder_tool_delete";

        public event Action<Vector3I, BuildTool> OnTileInteraction;
        public event Action<string, Vector3I> OnRoomPlaced;
        public event Action<string, Vector3I> OnTrapPlaced;
        public event Action<Vector3I> OnSpawnPointPlaced;
        public event Action OnModeSwitchRequested;
        public event Action<int> OnFloorChanged;

        public enum BuildTool
        {
            Select,
            Room,
            Trap,
            SpawnPoint,
            Delete
        }

        public DungeonGrid DungeonGrid => _dungeonGrid;
        public EssenceManager EssenceManager => _essenceManager;
        public int CurrentFloor => _currentFloor;
        public BuildTool CurrentTool => _currentTool;
        public Vector3I HoveredTile => _hoveredTile;
        public Vector3I SelectedTile => _selectedTile;

        public override void _Ready()
        {
            // Find components
            Camera ??= GetNodeOrNull<Camera3D>("Camera3D");
            SelectionIndicator ??= GetNodeOrNull<MeshInstance3D>("SelectionIndicator");
            PlacementPreview ??= GetNodeOrNull<MeshInstance3D>("PlacementPreview");

            if (Camera == null)
            {
                Camera = new Camera3D();
                AddChild(Camera);
            }

            // Configure camera for top-down view
            Camera.Position = new Vector3(GridWidth * TileSize / 2f, 30f, GridHeight * TileSize / 2f);
            Camera.Rotation = new Vector3(-Mathf.Pi / 2f, 0, 0); // Look straight down

            // Hide preview indicators initially
            if (SelectionIndicator != null) SelectionIndicator.Visible = false;
            if (PlacementPreview != null) PlacementPreview.Visible = false;

            SetupInputMap();

            GD.Print("BuilderController initialized");
        }

        public override void _Process(double delta)
        {
            if (ProcessMode == ProcessModeEnum.Disabled) return;

            HandleCameraInput((float)delta);
            HandleToolInput();
            UpdateHoveredTile();
            UpdateSelectionIndicator();
            UpdatePlacementPreview();
        }

        public override void _Input(InputEvent @event)
        {
            if (ProcessMode == ProcessModeEnum.Disabled) return;

            // Handle mouse panning (middle mouse or Alt+Left)
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex == MouseButton.Middle || 
                    (mouseButton.ButtonIndex == MouseButton.Left && Input.IsKeyPressed(Key.Alt)))
                {
                    if (mouseButton.Pressed)
                    {
                        _isPanning = true;
                        _lastMousePos = mouseButton.Position;
                    }
                    else
                    {
                        _isPanning = false;
                    }
                }
            }
            else if (@event is InputEventMouseMotion mouseMotion && _isPanning)
            {
                Vector2 delta = mouseMotion.Position - _lastMousePos;
                Camera.Position += new Vector3(-delta.X * CameraMoveSpeed * 0.01f, 0, -delta.Y * CameraMoveSpeed * 0.01f);
                ClampCameraPosition();
                _lastMousePos = mouseMotion.Position;
            }
            else if (@event is InputEventMouseButton click && click.ButtonIndex == MouseButton.Left && click.Pressed)
            {
                HandleLeftClick();
            }
            else if (@event is InputEventMouseButton rightClick && rightClick.ButtonIndex == MouseButton.Right && rightClick.Pressed)
            {
                HandleRightClick();
            }
            else if (@event is InputEventKey key && key.Pressed)
            {
                HandleKeyInput(key);
            }
        }

        private void SetupInputMap()
        {
            if (!InputMap.HasAction(ACTION_CAMERA_PAN))
            {
                InputMap.AddAction(ACTION_CAMERA_PAN);
                InputMap.ActionAddEvent(ACTION_CAMERA_PAN, new InputEventKey { Keycode = Key.Space });

                InputMap.AddAction(ACTION_CAMERA_ZOOM_IN);
                InputMap.ActionAddEvent(ACTION_CAMERA_ZOOM_IN, new InputEventKey { Keycode = Key.Equal });
                InputMap.ActionAddEvent(ACTION_CAMERA_ZOOM_IN, new InputEventKey { Keycode = Key.PageUp });

                InputMap.AddAction(ACTION_CAMERA_ZOOM_OUT);
                InputMap.ActionAddEvent(ACTION_CAMERA_ZOOM_OUT, new InputEventKey { Keycode = Key.Minus });
                InputMap.ActionAddEvent(ACTION_CAMERA_ZOOM_OUT, new InputEventKey { Keycode = Key.PageDown });

                InputMap.AddAction(ACTION_FLOOR_UP);
                InputMap.ActionAddEvent(ACTION_FLOOR_UP, new InputEventKey { Keycode = Key.E });

                InputMap.AddAction(ACTION_FLOOR_DOWN);
                InputMap.ActionAddEvent(ACTION_FLOOR_DOWN, new InputEventKey { Keycode = Key.Q });

                InputMap.AddAction(ACTION_PLACE);
                InputMap.ActionAddEvent(ACTION_PLACE, new InputEventKey { Keycode = Key.Enter });

                InputMap.AddAction(ACTION_CANCEL);
                InputMap.ActionAddEvent(ACTION_CANCEL, new InputEventKey { Keycode = Key.Escape });

                InputMap.AddAction(ACTION_SWITCH_MODE);
                InputMap.ActionAddEvent(ACTION_SWITCH_MODE, new InputEventKey { Keycode = Key.Tab });

                InputMap.AddAction(ACTION_TOOL_SELECT);
                InputMap.ActionAddEvent(ACTION_TOOL_SELECT, new InputEventKey { Keycode = Key.Key1 });

                InputMap.AddAction(ACTION_TOOL_ROOM);
                InputMap.ActionAddEvent(ACTION_TOOL_ROOM, new InputEventKey { Keycode = Key.Key2 });

                InputMap.AddAction(ACTION_TOOL_TRAP);
                InputMap.ActionAddEvent(ACTION_TOOL_TRAP, new InputEventKey { Keycode = Key.Key3 });

                InputMap.AddAction(ACTION_TOOL_SPAWN);
                InputMap.ActionAddEvent(ACTION_TOOL_SPAWN, new InputEventKey { Keycode = Key.Key4 });

                InputMap.AddAction(ACTION_TOOL_DELETE);
                InputMap.ActionAddEvent(ACTION_TOOL_DELETE, new InputEventKey { Keycode = Key.Key5 });
            }
        }

        private void HandleCameraInput(float delta)
        {
            // Keyboard panning
            Vector2 inputDir = Vector2.Zero;
            if (Input.IsActionPressed("ui_up")) inputDir.Y -= 1;
            if (Input.IsActionPressed("ui_down")) inputDir.Y += 1;
            if (Input.IsActionPressed("ui_left")) inputDir.X -= 1;
            if (Input.IsActionPressed("ui_right")) inputDir.X += 1;

            if (inputDir != Vector2.Zero)
            {
                inputDir = inputDir.Normalized();
                Camera.Position += new Vector3(inputDir.X * CameraMoveSpeed * delta, 0, inputDir.Y * CameraMoveSpeed * delta);
                ClampCameraPosition();
            }

            // Zoom
            if (Input.IsActionPressed(ACTION_CAMERA_ZOOM_IN))
            {
                Camera.Position = new Vector3(Camera.Position.X, Mathf.Max(Camera.Position.Y - CameraZoomSpeed * delta, MinZoom), Camera.Position.Z);
            }
            if (Input.IsActionPressed(ACTION_CAMERA_ZOOM_OUT))
            {
                Camera.Position = new Vector3(Camera.Position.X, Mathf.Min(Camera.Position.Y + CameraZoomSpeed * delta, MaxZoom), Camera.Position.Z);
            }

            // Floor switching
            if (Input.IsActionJustPressed(ACTION_FLOOR_UP))
            {
                ChangeFloor(_currentFloor + 1);
            }
            if (Input.IsActionJustPressed(ACTION_FLOOR_DOWN))
            {
                ChangeFloor(_currentFloor - 1);
            }
        }

        private void HandleToolInput()
        {
            if (Input.IsActionJustPressed(ACTION_TOOL_SELECT))
                SetTool(BuildTool.Select);
            else if (Input.IsActionJustPressed(ACTION_TOOL_ROOM))
                SetTool(BuildTool.Room);
            else if (Input.IsActionJustPressed(ACTION_TOOL_TRAP))
                SetTool(BuildTool.Trap);
            else if (Input.IsActionJustPressed(ACTION_TOOL_SPAWN))
                SetTool(BuildTool.SpawnPoint);
            else if (Input.IsActionJustPressed(ACTION_TOOL_DELETE))
                SetTool(BuildTool.Delete);

            if (Input.IsActionJustPressed(ACTION_SWITCH_MODE))
            {
                OnModeSwitchRequested?.Invoke();
            }

            if (Input.IsActionJustPressed(ACTION_CANCEL))
            {
                _selectedTile = new Vector3I(-1, -1, -1);
                _isPlacing = false;
                if (SelectionIndicator != null) SelectionIndicator.Visible = false;
                if (PlacementPreview != null) PlacementPreview.Visible = false;
            }
        }

        private void HandleKeyInput(InputEventKey key)
        {
            // Number keys for room/trap types could be added here
        }

        private void HandleLeftClick()
        {
            if (_hoveredTile.X < 0 || _hoveredTile.Y < 0) return;

            _selectedTile = _hoveredTile;

            switch (_currentTool)
            {
                case BuildTool.Room:
                    PlaceRoom(_selectedTile);
                    break;
                case BuildTool.Trap:
                    PlaceTrap(_selectedTile);
                    break;
                case BuildTool.SpawnPoint:
                    PlaceSpawnPoint(_selectedTile);
                    break;
                case BuildTool.Delete:
                    DeleteAt(_selectedTile);
                    break;
                case BuildTool.Select:
                default:
                    OnTileInteraction?.Invoke(_selectedTile, _currentTool);
                    break;
            }
        }

        private void HandleRightClick()
        {
            // Cancel placement or show context menu
            _isPlacing = false;
            if (SelectionIndicator != null) SelectionIndicator.Visible = false;
            if (PlacementPreview != null) PlacementPreview.Visible = false;
        }

        private void UpdateHoveredTile()
        {
            var mousePos = GetViewport().GetMousePosition();
            var camera = Camera;

            if (camera == null) return;

            // Raycast from camera to grid plane (Y = 0)
            var from = camera.ProjectRayOrigin(mousePos);
            var to = from + camera.ProjectRayNormal(mousePos) * 1000f;

            // Intersect with Y = 0 plane
            float t = -from.Y / (to.Y - from.Y);
            if (t >= 0 && t <= 1)
            {
                Vector3 intersection = from + (to - from) * t;
                int x = Mathf.FloorToInt(intersection.X / TileSize);
                int y = Mathf.FloorToInt(intersection.Z / TileSize);

                x = Mathf.Clamp(x, 0, GridWidth - 1);
                y = Mathf.Clamp(y, 0, GridHeight - 1);

                _hoveredTile = new Vector3I(x, y, _currentFloor);
            }
            else
            {
                _hoveredTile = new Vector3I(-1, -1, _currentFloor);
            }
        }

        private void UpdateSelectionIndicator()
        {
            if (SelectionIndicator == null) return;

            if (_hoveredTile.X >= 0 && _hoveredTile.Y >= 0)
            {
                SelectionIndicator.Visible = true;
                SelectionIndicator.Position = GridToWorld(_hoveredTile) + new Vector3(0, 0.1f, 0);
            }
            else
            {
                SelectionIndicator.Visible = false;
            }
        }

        private void UpdatePlacementPreview()
        {
            if (PlacementPreview == null) return;

            if (_currentTool != BuildTool.Select && _hoveredTile.X >= 0 && CanPlaceAt(_hoveredTile, _currentTool))
            {
                PlacementPreview.Visible = true;
                PlacementPreview.Position = GridToWorld(_hoveredTile) + new Vector3(0, 0.2f, 0);
                
                // Color based on tool
                if (PlacementPreview.MaterialOverride is StandardMaterial3D mat)
                {
                    mat.AlbedoColor = _currentTool switch
                    {
                        BuildTool.Room => new Color(0.2f, 0.8f, 0.2f, 0.5f),
                        BuildTool.Trap => new Color(0.8f, 0.2f, 0.2f, 0.5f),
                        BuildTool.SpawnPoint => new Color(0.2f, 0.2f, 0.8f, 0.5f),
                        _ => new Color(1, 1, 1, 0.5f)
                    };
                }
            }
            else
            {
                PlacementPreview.Visible = false;
            }
        }

        private bool CanPlaceAt(Vector3I pos, BuildTool tool)
        {
            if (_dungeonGrid == null) return false;
            var tile = _dungeonGrid.GetTile(pos.X, pos.Y, pos.Z);
            if (tile == null) return false;

            return tool switch
            {
                BuildTool.Room => tile.Type == DungeonGrid.TileType.Empty || tile.Type == DungeonGrid.TileType.Corridor,
                BuildTool.Trap => tile.Type == DungeonGrid.TileType.Corridor || tile.Type == DungeonGrid.TileType.Room,
                BuildTool.SpawnPoint => tile.Type == DungeonGrid.TileType.Room,
                BuildTool.Delete => tile.Type != DungeonGrid.TileType.Empty,
                _ => false
            };
        }

        private void PlaceRoom(Vector3I pos)
        {
            if (_dungeonGrid == null || _essenceManager == null) return;
            if (!CanPlaceAt(pos, BuildTool.Room)) return;

            // Simple cost for now - could be config-driven
            const long roomCost = 100;
            if (!_essenceManager.SpendEssence(roomCost))
            {
                GD.PrintErr("Not enough Essence to place room!");
                return;
            }

            _dungeonGrid.SetTileType(pos.X, pos.Y, DungeonGrid.TileType.Room, pos.Z);
            var tile = _dungeonGrid.GetTile(pos.X, pos.Y, pos.Z);
            if (tile != null)
            {
                tile.RoomId = _selectedRoomType;
            }

            OnRoomPlaced?.Invoke(_selectedRoomType, pos);
            GD.Print($"Placed room '{_selectedRoomType}' at {pos} (Cost: {roomCost} Essence)");
        }

        private void PlaceTrap(Vector3I pos)
        {
            if (_dungeonGrid == null || _essenceManager == null) return;
            if (!CanPlaceAt(pos, BuildTool.Trap)) return;
            if (string.IsNullOrEmpty(_selectedTrapType))
            {
                GD.PrintErr("No trap type selected!");
                return;
            }

            const long trapCost = 50;
            if (!_essenceManager.SpendEssence(trapCost))
            {
                GD.PrintErr("Not enough Essence to place trap!");
                return;
            }

            _dungeonGrid.SetTileType(pos.X, pos.Y, DungeonGrid.TileType.Trap, pos.Z);
            var tile = _dungeonGrid.GetTile(pos.X, pos.Y, pos.Z);
            if (tile != null)
            {
                tile.TrapId = _selectedTrapType;
            }

            OnTrapPlaced?.Invoke(_selectedTrapType, pos);
            GD.Print($"Placed trap '{_selectedTrapType}' at {pos} (Cost: {trapCost} Essence)");
        }

        private void PlaceSpawnPoint(Vector3I pos)
        {
            if (_dungeonGrid == null || _essenceManager == null) return;
            if (!CanPlaceAt(pos, BuildTool.SpawnPoint)) return;

            const long spawnCost = 200;
            if (!_essenceManager.SpendEssence(spawnCost))
            {
                GD.PrintErr("Not enough Essence to place spawn point!");
                return;
            }

            _dungeonGrid.SetTileType(pos.X, pos.Y, DungeonGrid.TileType.SpawnPoint, pos.Z);
            OnSpawnPointPlaced?.Invoke(pos);
            GD.Print($"Placed spawn point at {pos} (Cost: {spawnCost} Essence)");
        }

        private void DeleteAt(Vector3I pos)
        {
            if (_dungeonGrid == null) return;
            var tile = _dungeonGrid.GetTile(pos.X, pos.Y, pos.Z);
            if (tile == null || tile.Type == DungeonGrid.TileType.Empty) return;

            // Refund some essence based on tile type (simplified)
            long refund = tile.Type switch
            {
                DungeonGrid.TileType.Room => 50,
                DungeonGrid.TileType.Trap => 25,
                DungeonGrid.TileType.SpawnPoint => 100,
                DungeonGrid.TileType.Corridor => 10,
                _ => 0
            };

            if (refund > 0 && _essenceManager != null)
            {
                _essenceManager.AddEssence(refund);
            }

            tile.Type = DungeonGrid.TileType.Empty;
            tile.RoomId = null;
            tile.TrapId = null;
            tile.GarrisonedMonsters.Clear();

            GD.Print($"Deleted tile at {pos}, refunded {refund} Essence");
        }

        public void SetTool(BuildTool tool)
        {
            _currentTool = tool;
            GD.Print($"Builder tool changed to: {tool}");
        }

        public void SetSelectedRoomType(string roomType)
        {
            _selectedRoomType = roomType;
            if (_currentTool == BuildTool.Room)
            {
                GD.Print($"Selected room type: {roomType}");
            }
        }

        public void SetSelectedTrapType(string trapType)
        {
            _selectedTrapType = trapType;
            if (_currentTool == BuildTool.Trap)
            {
                GD.Print($"Selected trap type: {trapType}");
            }
        }

        public void ChangeFloor(int floor)
        {
            floor = Mathf.Clamp(floor, 0, MaxFloors - 1);
            if (floor != _currentFloor)
            {
                _currentFloor = floor;
                _hoveredTile = new Vector3I(_hoveredTile.X, _hoveredTile.Y, _currentFloor);
                _selectedTile = new Vector3I(_selectedTile.X, _selectedTile.Y, _currentFloor);
                OnFloorChanged?.Invoke(_currentFloor);
                GD.Print($"Changed to floor {_currentFloor}");
            }
        }

        public void Initialize(DungeonGrid dungeonGrid, EssenceManager essenceManager)
        {
            _dungeonGrid = dungeonGrid;
            _essenceManager = essenceManager;
        }

        public void EnterBuilderMode()
        {
            ProcessMode = ProcessModeEnum.Inherit;
            if (Camera != null)
            {
                Camera.Current = true;
            }
        }

        public void ExitBuilderMode()
        {
            ProcessMode = ProcessModeEnum.Disabled;
            if (Camera != null)
            {
                Camera.Current = false;
            }
            if (SelectionIndicator != null) SelectionIndicator.Visible = false;
            if (PlacementPreview != null) PlacementPreview.Visible = false;
        }

        private void ClampCameraPosition()
        {
            float halfWidth = GridWidth * TileSize / 2f;
            float halfHeight = GridHeight * TileSize / 2f;
            float margin = 10f;

            Camera.Position = new Vector3(
                Mathf.Clamp(Camera.Position.X, -margin, GridWidth * TileSize + margin),
                Mathf.Clamp(Camera.Position.Y, MinZoom, MaxZoom),
                Mathf.Clamp(Camera.Position.Z, -margin, GridHeight * TileSize + margin)
            );
        }

        private Vector3 GridToWorld(Vector3I gridPos)
        {
            return new Vector3(
                gridPos.X * TileSize + TileSize / 2f,
                0,
                gridPos.Y * TileSize + TileSize / 2f
            );
        }

        public Vector3I WorldToGrid(Vector3 worldPos)
        {
            return new Vector3I(
                Mathf.FloorToInt(worldPos.X / TileSize),
                Mathf.FloorToInt(worldPos.Z / TileSize),
                _currentFloor
            );
        }
    }
}