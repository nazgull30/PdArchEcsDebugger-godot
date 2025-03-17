namespace ArchEntityDebugger;

using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Arch.Core.Extensions;
using CompactJson;
using Godot;

public partial class ArchEntityDebuggerRuntime : Node
{
#if TOOLS
    public override void _Ready()
    {
        RegistryConverters.Register();
    }

    public override void _Process(double delta)
    {
        if (World.WorldSize == 0)
        {
            return;
        }

        var worlds = GetWorlds();


        var json = Serializer.ToString(worlds, false);
        EngineDebugger.SendMessage("ecs_debugger:update", [json]);
    }

    private static WorldData[] GetWorlds()
    {
        var res = new List<WorldData>();
        for (var i = 0; i < World.WorldSize; i++)
        {
            var world = World.Worlds[i];
            var worldData = new WorldData();
            res.Add(worldData);
            foreach (var archetype in world)
            {
                var archetypeData = new ArchetypeData
                {
                    Types = [.. archetype.Types.Select(t => t.Type.Name)]
                };
                worldData.Archetypes.Add(archetypeData);

                foreach (var chunk in archetype)
                {
                    var chunkData = new ChunkData();
                    archetypeData.Chunks.Add(chunkData);
                    foreach (var index in chunk)
                    {
                        archetypeData.EntityCount++;
                        var entity = chunk.Entities[index];
                        // var types = entity.GetComponentTypes().Select(c => c.Type.Name).ToList();
                        var entityData = new EntityData
                        {
                            Id = entity.Id,
                            Version = entity.Version(),
                            Types = entity.GetAllComponents()
                        };
                        worldData.TotalEntities++;
                        chunkData.Entities.Add(entityData);
                    }
                }
            }

        }


        return [.. res];
    }
#endif
}

