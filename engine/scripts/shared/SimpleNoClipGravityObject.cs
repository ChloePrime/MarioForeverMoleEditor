using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Shared;

public partial class SimpleNoClipGravityObject : Node2D
{
    [Export] public float XSpeed { get; set; }
    [Export] public float YSpeed { get; set; }
    [Export] public float XDirection { get; set; } = 1;
    [Export] public float Gravity { get; set; } = Units.Acceleration.CtfToGd(0.2F);

    public override void _Process(double delta)
    {
        base._Process(delta);
        Translate(new Vector2(XSpeed * XDirection, YSpeed) * (float)delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        YSpeed += Gravity * (float)delta;
    }
}