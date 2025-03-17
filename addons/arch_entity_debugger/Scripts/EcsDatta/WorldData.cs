namespace ArchEntityDebugger;

using System.Collections;
using System.Collections.Generic;
using Arch.Core;
using Godot;
using Godot.Collections;

public class WorldData : IEnumerable<ArchetypeData>
{
    public List<ArchetypeData> Archetypes = [];
    public int TotalEntities;

    public IEnumerator<ArchetypeData> GetEnumerator()
    {
        return Archetypes.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
