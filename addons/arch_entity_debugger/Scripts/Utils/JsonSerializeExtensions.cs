namespace ArchEntityDebugger;

using Godot;

public static class JsonSerializeExtensions
{
    public static string ToString(this Vector2 vector2)
    {
        return $"{vector2.X},{vector2.Y}";
    }

    public static Vector2 ParseVector2(string value)
    {
        var temp = value.Split(',');
        return new Vector2(float.Parse(temp[0]), float.Parse(temp[1]));
    }

    public static string ToString(this Vector3 vector3)
    {
        return $"{vector3.X},{vector3.Y}, {vector3.Z}";
    }

    public static Vector3 ParseVector3(string value)
    {
        var temp = value.Split(',');
        return new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
    }
}
