using System;
using Godot;

namespace DungeonLord.Scripts
{
    /// <summary>
    /// Vertical slice entry point: owns the single shared DungeonGrid + EssenceManager
    /// and toggles between top-down Builder mode and first-person Crawl mode (Tab).
    /// The same grid data drives both modes.
    /// </summary>
    [GlobalClass]
    public partial class GameRoot : Node3D
    {
        private const int GridWidth = 8;
        private const int GridHeight = 8;

        private DungeonGrid _grid;
        private EssenceManager _essence;
        private BuilderController _builder;
        private CrawlController _crawler;
        private GridVisual _gridVisual;
        private Label _modeLabel;
        private Label _essenceLabel;
        private Label _hintLabel;
        private Vector3I _lastRoomPos = new Vector3I(1, 1, 0);
        private bool _isCrawlMode = false;

        public override void _Ready()
        {
            _builder = GetNode<BuilderController>("BuilderMode/BuilderController");
            _crawler = GetNode<CrawlController>("CrawlMode/CrawlController");
            _gridVisual = GetNode<GridVisual>("World/GridVisual");
            var hud = GetNode<Control>("HUD");
            _modeLabel = hud.GetNode<Label>("ModeLabel");
            _essenceLabel = hud.GetNode<Label>("EssenceLabel");
            _hintLabel = hud.GetNode<Label>("HintLabel");

            // Single shared data model (FR-2.2): one grid, one essence pool for both modes.
            _grid = new DungeonGrid(GridWidth, GridHeight, 1);
            _essence = new EssenceManager(200);

            _builder.Initialize(_grid, _essence);
            _builder.GridWidth = GridWidth;
            _builder.GridHeight = GridHeight;
            _builder.SetSelectedRoomType("lair");
            _builder.SetSelectedTrapType("spikes");
            _builder.OnModeSwitchRequested += ToggleMode;
            _builder.OnRoomPlaced += OnRoomPlaced;
            _builder.OnTrapPlaced += OnTrapPlaced;

            _crawler.Initialize(_grid, _lastRoomPos, CrawlController.Direction.North);
            _crawler.OnModeSwitchRequested += ToggleMode;
            _crawler.OnPositionChanged += OnCrawlPositionChanged;

            _essence.OnEssenceChanged += OnEssenceChanged;

            _gridVisual.Initialize(_grid, 2.0f);
            _gridVisual.Rebuild();

            _hintLabel.Text = "Builder: [1] select  [2] room  [3] trap, left-click to place. Tab switches to Crawl. W/S move, A/D turn.";
            EnterBuilderMode();
            OnEssenceChanged(_essence.CurrentEssence);
        }

        public void ToggleMode()
        {
            if (_isCrawlMode) EnterBuilderMode();
            else EnterCrawlMode();
        }

        public void EnterBuilderMode()
        {
            _crawler.ExitCrawlMode();
            _lastRoomPos = _crawler.GridPosition;
            _builder.EnterBuilderMode();
            _isCrawlMode = false;
            _modeLabel.Text = "Mode: Builder (Tab -> Crawl)";
        }

        public void EnterCrawlMode()
        {
            _builder.ExitBuilderMode();
            // Lord spawns at the most recently built room (FR: spawn at room just built).
            _crawler.EnterCrawlMode(_lastRoomPos, _crawler.Facing);
            _isCrawlMode = true;
            _modeLabel.Text = "Mode: Crawl (Tab -> Builder)";
        }

        private void OnRoomPlaced(string roomType, Vector3I position)
        {
            _lastRoomPos = position;
            _gridVisual.Rebuild();
            GD.Print($"GameRoot: room '{roomType}' placed at {position}");
        }

        private void OnTrapPlaced(string trapType, Vector3I position)
        {
            _gridVisual.Rebuild();
            GD.Print($"GameRoot: trap '{trapType}' placed at {position}");
        }

        private void OnCrawlPositionChanged(Vector3I position, CrawlController.Direction facing)
        {
            GD.Print($"GameRoot: lord at {position} facing {facing}");
        }

        private void OnEssenceChanged(long essence)
        {
            _essenceLabel.Text = $"Essence: {essence}";
        }
    }
}