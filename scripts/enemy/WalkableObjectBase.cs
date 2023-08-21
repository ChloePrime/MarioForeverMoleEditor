using System;
using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

[CtfGroup("1")]
[GlobalClass]
public partial class WalkableObjectBase : GravityObjectBase
{
    [Export] public float TargetSpeed { get; set; } = Units.Speed.CtfToGd(1);
    
    [Export]
    public bool CollideWithOthers
    {
        get => GetCollisionMaskValue(MaFo.CollisionLayers.Enemy);
        set => SetCollisionMaskValue(MaFo.CollisionLayers.Enemy, value);
    }
    
    [ExportGroup($"{nameof(WalkableObjectBase)} (Advanced)")] 
    [Export] public float ControlAcceleration = Units.Acceleration.CtfMovementToGd(1);

    private VisibleOnScreenNotifier2D _enterScreenDetector;

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _enterScreenDetector, NpEnterScreenNotifier);
        
        // 进入屏幕后激活
        if (_enterScreenDetector is { } detector)
        {
            detector.ScreenEntered += EnableOnce;
        }
        
        if (GetTree().GetFirstNodeInGroup(MaFo.Groups.Player) is not Node2D mario)
        {
            return;
        }
        var relPos = ToLocal(mario.GlobalPosition).X;
        if (relPos != 0)
        {
            XDirection = Math.Sign(relPos);
        }
    }

    private static readonly NodePath NpEnterScreenNotifier = "Enter Screen Notifier";
    private bool _collideWithOthers = true;
    private bool _activated;

    protected override void _ProcessCollision()
    {
        if (IsOnWall())
        {
            XDirection *= -1;
            XSpeed = TargetSpeed;
        }
    }

    private void EnableOnce()
    {
        if (_activated) return;
        _activated = true;
        
        Enabled = true;
        XSpeed = TargetSpeed;
    }

    public override void _PhysicsProcess(double deltaD)
    {
        if (Enabled)
        {
            XSpeed = Mathf.MoveToward(XSpeed, TargetSpeed, ControlAcceleration * (float)deltaD);
        }
        base._PhysicsProcess(deltaD);
    }

    // private void OnCollideWithOthers(Node2D other)
    // {
    //     if (!CollideWithOthers || other == this || other is not WalkableObjectBase { CollideWithOthers: true } otherEnemy)
    //     {
    //         return;
    //     }
    //     var relativeX = otherEnemy.Position.X - Position.X;
    //     SetDirectionOnCollide(relativeX);
    //     otherEnemy.SetDirectionOnCollide(-relativeX);
    // }
    //
    // private void SetDirectionOnCollide(float relativeX)
    // {
    //     if (relativeX != 0)
    //     {
    //         XDirection = Math.Sign(relativeX);
    //     }
    // }
}