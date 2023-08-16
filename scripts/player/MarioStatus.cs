using System;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Player;

public abstract partial class MarioStatus : Resource, IEquatable<MarioStatus>
{
    public static MarioStatus Small => Mario.Constants.SmallStatus;
    public static MarioStatus Big => Mario.Constants.BigStatus;
    
    public abstract StringName GetId();
    public virtual MarioSize GetSize() => MarioSize.Big;
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
