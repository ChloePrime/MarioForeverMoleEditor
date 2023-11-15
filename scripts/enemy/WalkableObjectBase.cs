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
    
    /// <summary>
    /// 悬崖边自动回头 / 红乌龟看路
    /// </summary>
    [Export] public bool TurnAtCliff { get; set; }
	
    [ExportSubgroup("Advanced")]
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

    protected override void _ProcessCollision(float delta)
    {
        base._ProcessCollision(delta);
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
        if (GetTree().GetPlayer() is not Node2D mario)
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
        if (!CanMove)
        {
            base._PhysicsProcess(deltaD);
            return;
        }

        if (TurnAtCliff && IsOnFloor() && TestCliff())
        {
            XDirection *= -1;
            base._PhysicsProcess(deltaD);
            return;
        }

        if (!Mathf.IsZeroApprox(ControlAcceleration))
        {
            XSpeed = Mathf.MoveToward(XSpeed, TargetSpeed, ControlAcceleration * (float)deltaD);
        }
        else
        {
            XSpeed = TargetSpeed;
        }
        base._PhysicsProcess(deltaD);
    }

    private bool TestCliff()
    {
        var testPos = GlobalTransform.Translated(new Vector2(Size.X * XDirection, 0));
        var depth = FloorSnapLength + 2 * SafeMargin;
        return !TestMove(testPos, new Vector2(0, depth));
    }
}