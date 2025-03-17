namespace ArchEntityDebugger;

using System.Collections;
using System.Collections.Generic;

public class ChunkData : IEnumerable<EntityData>
{
    public List<EntityData> Entities = [];

    public IEnumerator<EntityData> GetEnumerator()
    {
        return Entities.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
