using System;

namespace ChloePrime.MarioForever.Util;

/// <summary>
/// 仅作辅助阅读用途，
/// 用于提示某个类对应 RE 引擎里的某个分组
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CtfGroupAttribute : Attribute
{
    public CtfGroupAttribute(string groupName)
    {
    }
}