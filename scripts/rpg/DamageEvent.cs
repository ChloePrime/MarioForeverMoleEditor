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
    public DamageEvent(DamageType types, Node2D source) : this(types, source, source)
    {
    }
}