using System;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

[CtfGroup("1")]
[GlobalClass]
public partial class WalkableObjectBase : GravityObjectBase
{
    [Export] public float JumpStrength { get; set; }
	
    [ExportGroup($"{nameof(GravityObjectBase)} (Advanced)")] 
    [Export] public float ControlAcceleration { get; set; }

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
        base._ProcessCollision();
        if (!WasThrown)
        {
            if (IsOnWall())
            {
                XSpeed = TargetSpeed;
            }
            if (IsOnFloor() && JumpStrength != 0)
            {
                YSpeed = -JumpStrength;
            }
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
        if (CanMove)
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