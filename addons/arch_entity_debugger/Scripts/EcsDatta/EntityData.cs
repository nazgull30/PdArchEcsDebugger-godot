namespace ArchEntityDebugger;

using System;
using System.Collections.Generic;

public class EntityData
{
    public int Id;
    public int Version;

    public object[] Types = [];


    public override bool Equals(object obj)
    {
        if (obj is not EntityData other)
            return false;
        return Id == other.Id && Version == other.Version;
    }
    // public List<objeect> GetComponents()
    // {
    //     var res = new List<Type>();
    //     foreach (var type in TypeNames)
    //     {
    //         res.Add(Type.GetType(type));
    //     }
    //     return res;
    // }
}
