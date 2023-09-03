#nullable enable
using System;
using ChloePrime.MarioForever.Player;

namespace ChloePrime.MarioForever;

/// <summary>
/// AutoLoad 类
/// </summary>
public static class GlobalData
{
    public static MarioStatus Status { get; set; } = null!;
    public static Int128 Score { get; set; }
    public static int Coins { get; set; }
    public static int Lives { get; set; }
    public static float HitPointLo { get; set; }
    public static float HitPointHi { get; set; }
    public static float MaxHitPointLo { get; set; }
    public static float MaxHitPointHi { get; set; }
    
    /// <summary>
    /// 请不要用这个，用 <see cref="GameRule.ResetGlobalData"/>
    /// </summary>
    internal static void ResetRuleNeutralValues()
    {
        Score = Coins = 0;
        Status = MarioStatus.Small;
        Lives = 4;
    }

    static GlobalData()
    {
        ResetRuleNeutralValues();
    }
}