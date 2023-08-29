using System.Numerics;
using Godot;

namespace ChloePrime.MarioForever.Util;

public static class MathUtil
{
    public static void UnsignedSub<T>(this ref T i, T r) where T: struct, INumber<T>, IMinMaxValue<T>
    {
        i = T.Max(T.Zero, i - r);
    }

    public static float MoveToward(this ref float from, float to, float delta)
    {
        var before = from;
        from = Mathf.MoveToward(from, to, delta);
        return from - before;
    }
}