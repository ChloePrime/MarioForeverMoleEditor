using System;

namespace ChloePrime.MarioForever;

public partial class GameRule
{

    public bool HitPointEnabled => HitPointPolicy != HitPointPolicyType.Disabled;
    
    public HitPointMagnitudeType HitPointMagnitude => HitPointPolicy switch
    {
        HitPointPolicyType.Disabled => HitPointMagnitudeType.Disabled,
        HitPointPolicyType.Mario3D or HitPointPolicyType.MegaMan => HitPointMagnitudeType.Low,
        HitPointPolicyType.Metroid or HitPointPolicyType.JRPG or _ => HitPointMagnitudeType.High,
    };

    public enum HitPointPolicyType
    {
        Disabled,
        /// <summary>
        /// 低绝对值血量系统，最多 6~8 点
        /// </summary>
        Mario3D,
        /// <summary>
        /// 低绝对值血量系统，最多 40 点左右
        /// </summary>
        MegaMan,
        /// <summary>
        /// 将生命值中多于 99 的部分按格子显示
        /// </summary>
        Metroid,
        /// <summary>
        /// 将生命值直接显示，不管是否大于 99
        /// </summary>
        JRPG,
    }
    
    public enum HitPointMagnitudeType
    {
        Disabled,
        Low,
        High,
    }

    public float HitPoint
    {
        get => SelectHitPoint(GlobalData.HitPointLo, GlobalData.HitPointHi);
        set => SetHitPoint(value);
    }

    public float MaxHitPoint => SelectHitPoint(GlobalData.MaxHitPointLo, GlobalData.MaxHitPointHi, 1);

    public float HitPointProtectsDeathCost => SelectHitPoint(HitPointProtectsDeathCostLo, HitPointProtectsDeathCostHi);

    public float SelectHitPoint(float low, float high, float fallback = 0) => HitPointMagnitude switch
    {
        HitPointMagnitudeType.Disabled => 0,
        HitPointMagnitudeType.Low => low,
        HitPointMagnitudeType.High or _ => high,
    };
    
    public void SetHitPoint(float value) => _ = HitPointMagnitude switch
    {
        HitPointMagnitudeType.Disabled => 0,
        HitPointMagnitudeType.Low => GlobalData.HitPointLo = value,
        HitPointMagnitudeType.High or _ => GlobalData.HitPointHi = value,
    };

    public void AlterHitPoint(float low, float high) => _ = HitPointMagnitude switch
    {
        HitPointMagnitudeType.Disabled => 0,
        HitPointMagnitudeType.Low => GlobalData.HitPointLo = Math.Clamp(GlobalData.HitPointLo + low, 0, GlobalData.MaxHitPointLo),
        HitPointMagnitudeType.High or _ => GlobalData.HitPointHi = Math.Clamp(GlobalData.HitPointHi + high, 0, GlobalData.MaxHitPointHi),
    };

    public void ResetHitPoint()
    {
        var hpMust = KillPlayerWhenHitPointReachesZero;
        GlobalData.HitPointLo = hpMust ? Math.Max(1, DefaultHitPointLo) : DefaultHitPointLo;
        GlobalData.HitPointHi = hpMust ? Math.Max(1, DefaultHitPointHi) : DefaultHitPointHi;
        GlobalData.MaxHitPointLo = MaxHitPointLo;
        GlobalData.MaxHitPointHi = MaxHitPointHi;
    }
}