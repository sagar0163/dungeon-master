using System.Threading.Tasks;
using Godot;

namespace DungeonLord.Scripts
{
    /// <summary>
    /// Auto-drives the vertical-slice loop for the README GIF and end-to-end verification:
    /// build a room + trap top-down (spending Essence), Tab into first-person Crawl,
    /// spawn at the room just built, 90-degree turn and walk tile-by-tile.
    /// Attached to GameRootDemo.tscn only; activated with DUNGEON_DEMO=1.
    /// </summary>
    [GlobalClass]
    public partial class DemoDriver : Node3D
    {
        private GameRoot _gameRoot;
        private BuilderController _builder;
        private CrawlController _crawler;
        private Camera3D _builderCamera;

        public override void _Ready()
        {
            if (OS.GetEnvironment("DUNGEON_DEMO") != "1")
            {
                QueueFree();
                return;
            }

            _gameRoot = GetParent().GetNode<GameRoot>("GameRoot");
            _builder = _gameRoot.GetNode<BuilderController>("BuilderMode/BuilderController");
            _crawler = _gameRoot.GetNode<CrawlController>("CrawlMode/CrawlController");
            _builderCamera = _gameRoot.GetNode<Camera3D>("BuilderMode/BuilderCamera");
            GD.Print("DEMO driver started");
            _Run();
        }

        private async void _Run()
        {
            await Frames(30); // let GameRoot._Ready finish + show empty grid

            // Top-down building, spending Essence. Order matters: the Lord spawns
            // at the most recently built room, and we want a room one tile East
            // of it so the walk lands on a built tile.
            _gameRoot.Essence.AddEssence(400); // bump demo budget so the walkable hall is affordable
            _builder.SetSelectedRoomType("lair");
            _builder.SetSelectedTrapType("spikes");
            _builder.SetTool(BuilderController.BuildTool.Room);
            _builder.PlaceRoom(new Vector3I(2, 2, 0)); // becomes the trap below
            await Frames(8);
            _builder.PlaceRoom(new Vector3I(2, 3, 0));
            await Frames(8);
            _builder.PlaceRoom(new Vector3I(4, 3, 0));
            await Frames(8);
            _builder.PlaceRoom(new Vector3I(3, 3, 0)); // most recently built room -> spawn
            await Frames(8);
            _builder.SetTool(BuilderController.BuildTool.Trap);
            _builder.PlaceTrap(new Vector3I(2, 2, 0));
            await Frames(15);

            GD.Print($"DEMO build: (3,3)={_gameRoot.Grid.GetTile(3, 3, 0).Type} (4,3)={_gameRoot.Grid.GetTile(4, 3, 0).Type} (2,2)={_gameRoot.Grid.GetTile(2, 2, 0).Type} essence={_gameRoot.Essence.CurrentEssence}");

            // Tab into Crawl: the Lord spawns at the room just built (3,3).
            await PressKey(Key.Tab);
            await Frames(15);
            GD.Print($"DEMO crawl spawn: pos={_crawler.GridPosition} facing={_crawler.Facing}");

            // 90-degree turn, then tile-by-tile walk East onto the room at (4,3).
            await PressKey(Key.D); // turn right
            await WaitWhileBusy();
            await Frames(10);
            GD.Print($"DEMO turn: facing={_crawler.Facing}");

            await PressKey(Key.W); // move forward one tile
            await WaitWhileBusy();
            await Frames(40); // hold on the final first-person view
            GD.Print($"DEMO walk: pos={_crawler.GridPosition} facing={_crawler.Facing}");

            bool pass =
                _gameRoot.Grid.GetTile(3, 3, 0).Type == TileType.Room &&
                _gameRoot.Grid.GetTile(4, 3, 0).Type == TileType.Room &&
                _gameRoot.Grid.GetTile(2, 2, 0).Type == TileType.Trap &&
                _crawler.GridPosition == new Vector3I(4, 3, 0) &&
                _crawler.Facing == CrawlController.Direction.East &&
                _gameRoot.Essence.CurrentEssence == 150;
            GD.Print($"DEMO RESULT: {(pass ? "PASS" : "FAIL")}");
        }

        private async Task WaitWhileBusy()
        {
            while (_crawler.IsBusy)
                await Frames(1);
            await Frames(2); // settle after the tween callback
        }

        private async Task PressKey(Key key)
        {
            Input.ParseInputEvent(new InputEventKey { Keycode = key, Pressed = true });
            await Frames(2);
            Input.ParseInputEvent(new InputEventKey { Keycode = key, Pressed = false });
            await Frames(2);
        }

        private async Task Frames(int count)
        {
            for (int i = 0; i < count; i++)
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }
}