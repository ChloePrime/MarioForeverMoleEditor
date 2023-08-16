using System;

namespace ChloePrime.MarioForever.Util;

/// <summary>
/// 仅作辅助阅读用途，
/// 用于提示某个 StringName 对应 RE 引擎里该物体的某个动画
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class CtfAnimationAttribute : Attribute
{
    public CtfAnimationAttribute(int id)
    {
    }

    public CtfAnimationAttribute(string name)
    {
    }
}