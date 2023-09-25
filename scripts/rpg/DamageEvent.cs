using System;
using ChloePrime.MarioForever.Shared;
using Godot;

namespace ChloePrime.MarioForever.RPG;

public readonly record struct DamageEvent(
    DamageType DamageTypes,
    Node2D TrueSource,
    Node2D DirectSource
)
{
    public DamageEvent(DamageType types, Node2D source) : this(types, source, source)
    {
    }
    
    public float DamageLo { get; init; } = 0;
    public float DamageHi { get; init; } = 0;
    public float DamageToEnemy { get => DamageLo; init => DamageLo = value; }
    public Vector2? AttackVector { get; init; } = null;
    public ComboTracker ComboTracker { get; init; }
    
    
    [Flags]
    public enum Flags
    {
        None               = 0,
        DeathProtection    = 1,
        BypassInvulnerable = 2,
        Silent             = 4,
    }

    public Flags EventFlags { get; init; } = Flags.None;
    public bool IsDeathProtection => EventFlags.HasFlag(Flags.DeathProtection);
    public bool BypassInvulnerable  => EventFlags.HasFlag(Flags.BypassInvulnerable);
    public bool IsSilent => EventFlags.HasFlag(Flags.Silent);
}