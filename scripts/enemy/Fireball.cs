using System;
using ChloePrime.Godot.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
[Icon("res://resources/shared/T_fireball.png")]
public partial class Fireball : WalkableObjectBase
{
    [Export] public AudioStream DefaultExplodeSound { get; set; } = GD.Load<AudioStream>("res://resources/shared/SE_fireball_hit.wav");
    [Export] public PackedScene ExplodeResult { get; set; } = GD.Load<PackedScene>("res://objects/effect/O_explosion_s.tscn");
    
    public Node2D Shooter { get; set; }
    
    [Flags]
    public enum ExplodeFlags
    {
        None = 0,
        WithDefaultSound = 1,
    }

    public void Explode() => Explode(ExplodeFlags.None);
    
    public virtual void Explode(ExplodeFlags flags)
    {
        if ((flags & ExplodeFlags.WithDefaultSound) != 0)
        {
            DefaultExplodeSound?.Play();
        }

        if (ExplodeResult is { } explosionPrefab&& GetParent() is {} parent)
        {
            var explosion = explosionPrefab.Instantiate();
            parent.AddChild(explosion);
            if (explosion is Node2D explosion2D)
            {
                explosion2D.GlobalPosition = GlobalPosition;
            }
        }
        
        QueueFree();
    }
    
    protected override void _ProcessCollision(float delta)
    {
        if (IsOnWall() || IsOnCeiling())
        {
            Explode(ExplodeFlags.WithDefaultSound);
            return;
        }
        base._ProcessCollision(delta);
    }
}