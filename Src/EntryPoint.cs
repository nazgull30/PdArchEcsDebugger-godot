namespace PdArchEcsDebugger.Src;

using Arch.Core;
using Godot;

public partial class EntryPoint : Node
{
    public override void _Ready()
    {
        var world = World.Create();
        var person = world.Create(new Name
        {
            Value = "Mark"
        }, new Count
        {
            Value = 2
        });

    }
}
