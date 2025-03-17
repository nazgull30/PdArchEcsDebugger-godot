namespace ArchEntityDebugger;

using System.Collections;
using System.Collections.Generic;
using Arch.Core.Utils;
using Godot;

public class ArchetypeData : IEnumerable<ChunkData>
{
    public List<string> Types = [];
    public List<ChunkData> Chunks = [];
    public int EntityCount;

    public IEnumerator<ChunkData> GetEnumerator()
    {
        return Chunks.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
