using Godot;

namespace ChloePrime.Godot.Util;

public static class MathUtil
{
    public static float MoveToward(this ref float from, float to, float delta)
    {
        var before = from;
        from = Mathf.MoveToward(from, to, delta);
        return from - before;
    }
}