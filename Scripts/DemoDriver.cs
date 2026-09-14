using System.Threading.Tasks;
using Godot;

namespace DungeonLord.Scripts
{
    /// <summary>
    /// Auto-drives the vertical-slice loop for the README GIF and end-to-end verification:
    /// build a room + trap top-down, Tab into first-person Crawl, spawn at the built room, walk.
    /// Attached to GameRootDemo.tscn only.
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

            _gameRoot = GetParent() as GameRoot;
            _builder = _gameRoot.GetNode<BuilderController>("BuilderMode/BuilderController");
            _crawler = _gameRoot.GetNode<CrawlController>("CrawlMode/CrawlController");
            _builderCamera = _gameRoot.GetNode<Camera3D>("BuilderMode/BuilderCamera");
            _Run();
        }

        private async void _Run()
        {
            await Frames(10); // let GameRoot._Ready finish

            // Top-down building: a small walkable hall + a trap, spending Essence.
            _gameRoot.Essence.AddEssence(400); // bump demo budget so we can afford the walkable hall
            await PressKey(Key.Key2); // select Room tool
            await ClickWorld(new Vector3(2 * 2f + 1, 0, 2 * 2f + 1)); // room tile (2,2)
            await ClickWorld(new Vector3(3 * 2f + 1, 0, 2 * 2f + 1)); // room tile (3,2)
            await PressKey(Key.Key3); // select Trap tool
            await ClickWorld(new Vector3(3 * 2f + 1, 0, 2 * 2f + 1)); // trap on tile (3,2)
            await Frames(3);

            GD.Print($"DEMO build: (2,2)={_gameRoot.Grid.GetTile(2, 2, 0).Type} (3,2)={_gameRoot.Grid.GetTile(3, 2, 0).Type} essence={_gameRoot.Essence.CurrentEssence}");

            // Tab into Crawl: Lord spawns at the most recently built room tile (2,2).
            await PressKey(Key.Tab);
            await Frames(5);
            GD.Print($"DEMO crawl spawn: pos={_crawler.GridPosition} facing={_crawler.Facing}");

            // Turn to face East, then walk tile-by-tile onto the trap tile.
            await PressKey(Key.D); // turn right (90 deg)
            await PressKey(Key.W); // move forward onto (3,2)
            await Frames(10);
            GD.Print($"DEMO walk: pos={_crawler.GridPosition} facing={_crawler.Facing}");

            if (_gameRoot.Grid.GetTile(2, 2, 0).Type == TileType.Room &&
                _gameRoot.Grid.GetTile(3, 2, 0).Type == TileType.Trap &&
                _crawler.GridPosition == new Vector3I(3, 2, 0) &&
                _gameRoot.Essence.CurrentEssence == 350)
            {
                GD.Print("DEMO RESULT: PASS");
            }
            else
            {
                GD.Print("DEMO RESULT: FAIL");
            }
        }

        private async Task ClickWorld(Vector3 worldPos)
        {
            Vector2 screen = _builderCamera.UnprojectPosition(worldPos);
            Input.ParseInputEvent(new InputEventMouseMotion { Position = screen });
            await Frames(2);
            Input.ParseInputEvent(new InputEventMouseButton { Position = screen, ButtonIndex = MouseButton.Left, Pressed = true });
            await Frames(2);
            Input.ParseInputEvent(new InputEventMouseButton { Position = screen, ButtonIndex = MouseButton.Left, Pressed = false });
            await Frames(4);
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