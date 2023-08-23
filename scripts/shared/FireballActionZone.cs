using ChloePrime.MarioForever.Enemy;
using Godot;

namespace ChloePrime.MarioForever.Shared;

public partial class FireballActionZone : Area2D
{
    public Fireball Fireball { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Fireball = GetParent<Fireball>();
    }
}