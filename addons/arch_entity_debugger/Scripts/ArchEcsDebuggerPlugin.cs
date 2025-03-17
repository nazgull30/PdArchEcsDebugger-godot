namespace ArchEntityDebugger;

using Godot;

[Tool]
public partial class ArchEcsDebuggerPlugin : EditorPlugin
{
    private Debugger _debugger;

    public override void _EnterTree()
    {
        _debugger = new Debugger();
        AddDebuggerPlugin(_debugger);
    }

    public override void _ExitTree()
    {
        RemoveDebuggerPlugin(_debugger);
    }

}
