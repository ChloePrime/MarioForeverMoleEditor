#nullable enable
using System.Numerics;
using ChloePrime.MarioForever.Player;

namespace ChloePrime.MarioForever;

/// <summary>
/// AutoLoad 类
/// </summary>
public static class GlobalData
{
    public static MarioStatus Status { get; set; } = MarioStatus.Big;
    public static BigInteger Score { get; set; }
    public static int Lives { get; set; }
}