using System;
using System.Numerics;

namespace ChloePrime.MarioForever.Util;

public static class MathUtil
{
    public static void UnsignedSub<T>(ref this T i, T r) where T: struct, INumber<T>, IMinMaxValue<T>
    {
        i = T.Max(T.Zero, i - r);
    }
}