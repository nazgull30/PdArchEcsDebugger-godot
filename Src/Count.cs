namespace PdArchEcsDebugger.Src;

using PdArchEcsCore.Entities;

[Component]
public struct Count
{
    public int Value;

    public override readonly string ToString() => $"{Value}";
}
