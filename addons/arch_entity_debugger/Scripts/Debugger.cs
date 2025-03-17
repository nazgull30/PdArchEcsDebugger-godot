namespace ArchEntityDebugger;

using System.Text.Json;
using CompactJson;
using Godot;
using Godot.NativeInterop;

[Tool]
public partial class Debugger : EditorDebuggerPlugin
{
    private EntityDebugger _entityDebugger;

    public override bool _HasCapture(string capture) => capture == "ecs_debugger";

    public override void _SetupSession(int sessionId)
    {
        var scene = GD.Load<PackedScene>("res://addons/arch_entity_debugger/Assets/Scenes/Entity_Debugger.tscn");
        _entityDebugger = scene.Instantiate<EntityDebugger>();
        _entityDebugger.Name = "Arch Entity Debugger";

        var session = GetSession(sessionId);
        session.Started += () => _entityDebugger.SetActive(true);
        session.Stopped += () =>
        {
            _entityDebugger.SetActive(false);
            _entityDebugger.ClearEntityListTree();
        };
        session.AddSessionTab(_entityDebugger);
    }

    public override bool _Capture(string message, Godot.Collections.Array data, int sessionId)
    {
        if (_entityDebugger == null)
            return false;

        if (message == "ecs_debugger:test")
        {
            GD.Print($"Debugger. Test: {data[0]}");
        }

        if (message == "ecs_debugger:update")
        {

            var worldDataStr = data[0].AsString();
            var worlds = Serializer.Parse<WorldData[]>(worldDataStr);
            _entityDebugger.Render(worlds);
            return true;
        }

        return false;
    }
}
