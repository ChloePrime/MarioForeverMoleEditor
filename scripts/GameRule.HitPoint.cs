﻿using System;

namespace ChloePrime.MarioForever;

public partial class GameRule
{

    public bool HitPointEnabled => HitPointPolicy != HitPointPolicyType.Disabled;
    
    public HitPointMagnitudeType HitPointMagnitude => HitPointPolicy switch
    {
        HitPointPolicyType.Disabled => HitPointMagnitudeType.Disabled,
        HitPointPolicyType.Mario3D => HitPointMagnitudeType.Low,
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
        GlobalData.HitPointLo = GlobalData.MaxHitPointLo = MaxHitPointLo;
        GlobalData.HitPointHi = GlobalData.MaxHitPointHi = MaxHitPointHi;
    }
}