using Godot;

namespace ChloePrime.MarioForever.RPG;

public readonly record struct DamageEvent(
    DamageType DamageTypes,
    Node2D TrueSource,
    Node2D DirectSource
)
{
    public float DamageLo { get; init; } = 0;
    public float DamageHi { get; init; } = 0;
    public bool IsDeathProtection { get; init; } = false;
    public bool BypassInvulnerable { get; init; } = false;
    public DamageEvent(DamageType types, Node2D source) : this(types, source, source)
    {
    }
}