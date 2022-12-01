using UnityEngine;

public static class Extensions
{
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
    public static void Wrap(this ref int value, int max)
    {
        value %= max;
        if (value < 0)
            value = max - 1;
    }
    public static Vector2Int ToVec2IntPosition(this Vector2 value)
    {
        return new Vector2Int((int)value.x, (int)value.y);
    }
    public static Vector2Int ToVec2IntPosition(this Vector3 value)
    {
        return new Vector2Int((int)value.x, (int)value.y);
    }
}