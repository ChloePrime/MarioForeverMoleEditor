using System;
using System.Globalization;
using JetBrains.Annotations;

namespace ChloePrime.MarioForever.Util;

/// <summary>
/// 用于标记某个类是否经过重构 / 经过第几次重构
/// </summary>
[MajorRevision(1)]
[AttributeUsage(AttributeTargets.Class)]
public class MajorRevisionAttribute : Attribute {
    public string Version { [UsedImplicitly] get; }

    public MajorRevisionAttribute(double version) {
        Version = version.ToString(CultureInfo.InvariantCulture);
    }

    public MajorRevisionAttribute(string version) {
        Version = version;
    }
}