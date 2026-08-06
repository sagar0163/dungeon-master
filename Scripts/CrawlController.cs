using System;
using Godot;

namespace DungeonLord.Scripts
{
    /// <summary>
    /// First-person grid-based Crawl Mode controller.
    /// Discrete tile movement and 90-degree turning (Dungeon Master / Legend of Grimrock style).
    /// </summary>
    [GlobalClass]
    public partial class CrawlController : Node3D
    {
        // Configuration
        [Export] public float MoveDuration { get; set; } = 0.3f;
        [Export] public float TurnDuration { get; set; } = 0.2f;
        [Export] public float CameraHeight { get; set; } = 1.6f;
        [Export] public float TileSize { get; set; } = 2.0f;

        // Components
        [Export] public Camera3D Camera { get; private set; }
        [Export] public CharacterBody3D Body { get; private set; }

        // State
        private DungeonGrid _dungeonGrid;
        private Vector3I _gridPosition = new Vector3I(1, 1, 0); // x, y, floor
        private Direction _facing = Direction.North;
        private bool _isMoving = false;
        private bool _isTurning = false;
        private Tween _currentTween;

        // Input actions
        private const string ACTION_MOVE_FORWARD = "crawl_move_forward";
        private const string ACTION_MOVE_BACKWARD = "crawl_move_backward";
        private const string ACTION_TURN_LEFT = "crawl_turn_left";
        private const string ACTION_TURN_RIGHT = "crawl_turn_right";
        private const string ACTION_SWITCH_MODE = "crawl_switch_mode";

        public event Action<Vector3I, Direction> OnPositionChanged;
        public event Action OnModeSwitchRequested;

        public enum Direction
        {
            North = 0,
            East = 1,
            South = 2,
            West = 3
        }

        public Vector3I GridPosition => _gridPosition;
        public Direction Facing => _facing;
        public bool IsBusy => _isMoving || _isTurning;

        public override void _Ready()
        {
            // Find components if not assigned in editor
            Camera ??= GetNodeOrNull<Camera3D>("Camera3D");
            Body ??= GetNodeOrNull<CharacterBody3D>("CharacterBody3D");

            if (Camera == null)
            {
                Camera = new Camera3D();
                AddChild(Camera);
            }

            // Position camera at eye height
            Camera.Position = new Vector3(0, CameraHeight, 0);

            // Set up input map if not already configured
            SetupInputMap();

            GD.Print("CrawlController initialized at grid position: " + _gridPosition);
        }

        public override void _Process(double delta)
        {
            if (_isMoving || _isTurning) return;

            HandleInput();
        }

        private void SetupInputMap()
        {
            // Only add if not already present
            if (!InputMap.HasAction(ACTION_MOVE_FORWARD))
            {
                InputMap.AddAction(ACTION_MOVE_FORWARD);
                InputMap.ActionAddEvent(ACTION_MOVE_FORWARD, new InputEventKey { Keycode = Key.W });
                InputMap.ActionAddEvent(ACTION_MOVE_FORWARD, new InputEventKey { Keycode = Key.Up });

                InputMap.AddAction(ACTION_MOVE_BACKWARD);
                InputMap.ActionAddEvent(ACTION_MOVE_BACKWARD, new InputEventKey { Keycode = Key.S });
                InputMap.ActionAddEvent(ACTION_MOVE_BACKWARD, new InputEventKey { Keycode = Key.Down });

                InputMap.AddAction(ACTION_TURN_LEFT);
                InputMap.ActionAddEvent(ACTION_TURN_LEFT, new InputEventKey { Keycode = Key.A });
                InputMap.ActionAddEvent(ACTION_TURN_LEFT, new InputEventKey { Keycode = Key.Left });

                InputMap.AddAction(ACTION_TURN_RIGHT);
                InputMap.ActionAddEvent(ACTION_TURN_RIGHT, new InputEventKey { Keycode = Key.D });
                InputMap.ActionAddEvent(ACTION_TURN_RIGHT, new InputEventKey { Keycode = Key.Right });

                InputMap.AddAction(ACTION_SWITCH_MODE);
                InputMap.ActionAddEvent(ACTION_SWITCH_MODE, new InputEventKey { Keycode = Key.Tab });
            }
        }

        private void HandleInput()
        {
            if (Input.IsActionJustPressed(ACTION_SWITCH_MODE))
            {
                OnModeSwitchRequested?.Invoke();
                return;
            }

            if (Input.IsActionJustPressed(ACTION_TURN_LEFT))
            {
                Turn(-1);
            }
            else if (Input.IsActionJustPressed(ACTION_TURN_RIGHT))
            {
                Turn(1);
            }
            else if (Input.IsActionJustPressed(ACTION_MOVE_FORWARD))
            {
                MoveForward();
            }
            else if (Input.IsActionJustPressed(ACTION_MOVE_BACKWARD))
            {
                MoveBackward();
            }
        }

        public void Initialize(DungeonGrid dungeonGrid, Vector3I startPosition, Direction startFacing = Direction.North)
        {
            _dungeonGrid = dungeonGrid;
            _gridPosition = startPosition;
            _facing = startFacing;

            // Sync world position
            SyncWorldPosition();
            SyncRotation();
        }

        public void MoveForward()
        {
            if (_isMoving || _isTurning || _dungeonGrid == null) return;

            var targetPos = GetTileInDirection(_gridPosition, _facing);
            if (!CanMoveTo(targetPos)) return;

            _isMoving = true;
            var startWorldPos = GlobalPosition;
            var targetWorldPos = GridToWorld(targetPos);

            _currentTween = CreateTween();
            _currentTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
            _currentTween.TweenProperty(this, "global_position", targetWorldPos, MoveDuration);
            _currentTween.TweenCallback(Callable.From(() => OnMoveComplete(targetPos)));
            _currentTween.Play();
        }

        public void MoveBackward()
        {
            if (_isMoving || _isTurning || _dungeonGrid == null) return;

            var backDir = (Direction)(((int)_facing + 2) % 4);
            var targetPos = GetTileInDirection(_gridPosition, backDir);
            if (!CanMoveTo(targetPos)) return;

            _isMoving = true;
            var targetWorldPos = GridToWorld(targetPos);

            _currentTween = CreateTween();
            _currentTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
            _currentTween.TweenProperty(this, "global_position", targetWorldPos, MoveDuration);
            _currentTween.TweenCallback(Callable.From(() => OnMoveComplete(targetPos)));
            _currentTween.Play();
        }

        public void Turn(int direction) // -1 = left, 1 = right
        {
            if (_isMoving || _isTurning) return;

            _isTurning = true;
            int newFacingInt = ((int)_facing + direction + 4) % 4;
            Direction newFacing = (Direction)newFacingInt;
            float targetRotationY = newFacingInt * Mathf.Pi / 2.0f;

            _currentTween = CreateTween();
            _currentTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
            _currentTween.TweenProperty(this, "rotation", new Vector3(0, targetRotationY, 0), TurnDuration);
            _currentTween.TweenCallback(Callable.From(() => OnTurnComplete(newFacing)));
            _currentTween.Play();
        }

        private void OnMoveComplete(Vector3I targetPos)
        {
            _isMoving = false;
            _gridPosition = targetPos;
            OnPositionChanged?.Invoke(_gridPosition, _facing);
            GD.Print($"Moved to grid position: {_gridPosition}, facing: {_facing}");
        }

        private void OnTurnComplete(Direction newFacing)
        {
            _isTurning = false;
            _facing = newFacing;
            OnPositionChanged?.Invoke(_gridPosition, _facing);
            GD.Print($"Turned to face: {_facing}");
        }

        private Vector3I GetTileInDirection(Vector3I from, Direction dir)
        {
            return dir switch
            {
                Direction.North => new Vector3I(from.X, from.Y - 1, from.Z),
                Direction.East => new Vector3I(from.X + 1, from.Y, from.Z),
                Direction.South => new Vector3I(from.X, from.Y + 1, from.Z),
                Direction.West => new Vector3I(from.X - 1, from.Y, from.Z),
                _ => from
            };
        }

        private bool CanMoveTo(Vector3I pos)
        {
            if (_dungeonGrid == null) return false;
            var tile = _dungeonGrid.GetTile(pos.X, pos.Y, pos.Z);
            return tile != null && tile.Type != DungeonGrid.TileType.Empty;
        }

        private Vector3 GridToWorld(Vector3I gridPos)
        {
            return new Vector3(
                gridPos.X * TileSize,
                CameraHeight,
                gridPos.Y * TileSize
            );
        }

        private void SyncWorldPosition()
        {
            GlobalPosition = GridToWorld(_gridPosition);
        }

        private void SyncRotation()
        {
            float targetRotationY = ((int)_facing) * Mathf.Pi / 2.0f;
            Rotation = new Vector3(0, targetRotationY, 0);
        }

        // Called when entering crawl mode from builder mode
        public void EnterCrawlMode(Vector3I position, Direction facing)
        {
            _gridPosition = position;
            _facing = facing;
            SyncWorldPosition();
            SyncRotation();
            ProcessMode = ProcessModeEnum.Inherit;
        }

        // Called when exiting crawl mode to builder mode
        public void ExitCrawlMode()
        {
            ProcessMode = ProcessModeEnum.Disabled;
        }

        public Vector3 GetWorldPosition() => GlobalPosition;
        public Vector3 GetLookDirection()
        {
            return _facing switch
            {
                Direction.North => Vector3.Back,
                Direction.East => Vector3.Right,
                Direction.South => Vector3.Forward,
                Direction.West => Vector3.Left,
                _ => Vector3.Back
            };
        }
    }
}