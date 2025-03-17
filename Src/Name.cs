namespace PdArchEcsDebugger.Src;

using PdArchEcsCore.Entities;

[Component]
public struct Name
{
    public string Value;

    public override string ToString() => $"{Value}";
}
