#nullable enable
using System.Numerics;
using ChloePrime.MarioForever.Player;

namespace ChloePrime.MarioForever;

/// <summary>
/// AutoLoad 类
/// </summary>
public static class GlobalData
{
    public static MarioStatus Status { get; set; } = null!;
    public static BigInteger Score { get; set; }
    public static int Coins { get; set; }
    public static int Lives { get; set; }
    
    public static void Reset()
    {
        Score = Coins = 0;
        Status = MarioStatus.Small;
        Lives = 4;
    }

    static GlobalData()
    {
        Reset();
    }
}