using Godot;

namespace DungeonLord.Scripts
{
    [GlobalClass]
    public partial class Probe : Node3D
    {
        public override void _Ready()
        {
            GD.Print("PROBE _Ready fired");
        }
    }
}