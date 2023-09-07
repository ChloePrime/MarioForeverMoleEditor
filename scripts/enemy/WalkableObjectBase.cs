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
    [Export] public float JumpStrength { get; set; }

    [ExportGroup($"{nameof(WalkableObjectBase)} (Advanced)")] 
    [Export] public float ControlAcceleration;

    private VisibleOnScreenNotifier2D _enterScreenDetector;

    public override void _Ready()
    {
        base._Ready();
        this.GetNodeOrNull(out _enterScreenDetector, NpEnterScreenNotifier);
        
        // 进入屏幕后激活
        if (_enterScreenDetector is { } detector)
        {
            detector.ScreenEntered += EnableOnce;
        }
    }

    private static readonly NodePath NpEnterScreenNotifier = "Enter Screen Notifier";
    private bool _activated;

    protected override void _ProcessCollision()
    {
        if (IsOnWall())
        {
            XDirection *= -1;
            XSpeed = TargetSpeed;
        }
        if (IsOnFloor() && JumpStrength != 0)
        {
            YSpeed = -JumpStrength;
        }
    }

    private void EnableOnce()
    {
        if (_activated) return;
        _activated = true;
        
        Enabled = true;
        XSpeed = TargetSpeed;
        if (!Appearing)
        {
            TryFaceTowardsMario();
        }
    }

    protected void TryFaceTowardsMario()
    {
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

    public override void _PhysicsProcess(double deltaD)
    {
        if (Enabled)
        {
            if (!Mathf.IsZeroApprox(ControlAcceleration))
            {
                XSpeed = Mathf.MoveToward(XSpeed, TargetSpeed, ControlAcceleration * (float)deltaD);
            }
            else
            {
                XSpeed = TargetSpeed;
            }
        }
        base._PhysicsProcess(deltaD);
    }
}