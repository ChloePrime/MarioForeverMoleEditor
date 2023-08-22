using System;
using Godot;

namespace ChloePrime.MarioForever.Player;

public abstract partial class MarioStatus : Resource, IEquatable<MarioStatus>
{
    public static MarioStatus Small => Mario.Constants.SmallStatus;
    public static MarioStatus Big => Mario.Constants.BigStatus;
    
    public abstract StringName GetId();
    [Export] public MarioSize Size { get; private set; } = MarioSize.Big;
    
    /// <summary>
    /// 指定此状态的马里奥受伤后变为什么状态。
    /// 该值为空时，该状态下的马里奥受伤后会直接挂掉，空值的典型应用场景为小个子状态。
    /// </summary>
    [Export] public MarioStatus HurtResult { get; private set; }
    [Export] public PackedScene AnimationNode { get; private set; }

    public virtual void Fire(Mario mario)
    {
    }

    public bool Equals(MarioStatus other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(GetId(), other.GetId());
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((MarioStatus)obj);
    }

    public override int GetHashCode()
    {
        return GetId()?.GetHashCode() ?? 0;
    }

    public static bool operator ==(MarioStatus left, MarioStatus right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(MarioStatus left, MarioStatus right)
    {
        return !Equals(left, right);
    }
}
