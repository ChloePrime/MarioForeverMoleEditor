using System;

namespace ChloePrime.MarioForever.Util;

/// <summary>
/// 仅作辅助阅读用途，
/// 用于提示某个字段对应 RE 引擎里该物体的某个 flag
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class CtfFlagAttribute : Attribute
{
    public CtfFlagAttribute(int id)
    {
    }
}