using System.Numerics;
using Godot;

namespace ChloePrime.MarioForever.Util;

public static class MathUtil
{
    public static void UnsignedSub<T>(this ref T i, T r) where T: struct, INumber<T>, IMinMaxValue<T>
    {
        i = T.Max(T.Zero, i - r);
    }

    public static void MoveForward(this ref float from, float to, float delta)
    {
        from = Mathf.MoveToward(from, to, delta);
    }
}