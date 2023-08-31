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
}