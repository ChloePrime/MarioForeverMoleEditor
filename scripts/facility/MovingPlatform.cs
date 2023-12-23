using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Facility;

[GlobalClass]
public partial class MovingPlatform : AnimatableBody2D, IMarioStandable
{
    [Export] public Vector2 Velocity { get; set; }
    [Export] public Vector2 Gravity { get; set; } = new(0, Units.Acceleration.CtfToGd(0.2F));

    public float XSpeed
    {
        get => Velocity.X;
        set => Velocity = Velocity with { X = value };
    }
    
    public float YSpeed
    {
        get => Velocity.Y;
        set => Velocity = Velocity with { Y = value };
    }

    public CollisionShape2D CollisionShape => _collisionShape ??= GetNode<CollisionShape2D>(NpCollisionShape);

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_everGotStand)
        {
            Velocity += Gravity * (float)delta;
        }
        Translate(Velocity * (float)delta);
    }
    
    public void ProcessMarioStandOn(Node2D mario)
    {
        _everGotStand = true;
    }

    private static readonly NodePath NpCollisionShape = "Collision Shape";
    private CollisionShape2D _collisionShape;
    private bool _everGotStand;
}