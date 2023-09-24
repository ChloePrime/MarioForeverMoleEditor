﻿using System;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class Turtle : WalkableNpc
{
    [Export] public float WalkSpeed { get; set; } = Units.Speed.CtfToGd(1);
    [Export] public float RollSpeed { get; set; } = Units.Speed.CtfToGd(5);

    public enum TurtleState
    {
        Flying,
        Jumping,
        Walking,
        StaticShell,
        MovingShell,
    }

    [Export]
    public TurtleState State
    {
        get => _state;
        set => SetState(value);
    }

    [Signal]
    public delegate void TurtleStateChangedEventHandler();

    public void KickBy(Node2D kicker)
    {
        XDirection = -Math.Sign(ToLocal(kicker.GlobalPosition).X);
    }
    
    public override bool WillHurtOthers => State is TurtleState.MovingShell || base.WillHurtOthers;

    public override void _Ready()
    {
        base._Ready();
        GrabReleased += OnGrabReleased;
    }

    protected override void _ProcessCollision(float delta)
    {
        base._ProcessCollision(delta);
        if (State is TurtleState.MovingShell && IsOnWall())
        {
            XSpeed = LastXSpeed;
        }
    }

    private void OnGrabReleased(Mario.GrabReleaseEvent e)
    {
        if (e.Flags.HasFlag(Mario.GrabReleaseFlags.TossHorizontally))
        {
            var xSpeed = XSpeed;
            State = TurtleState.MovingShell;
            XSpeed = xSpeed;
        }
    }

    private void SetState(TurtleState value)
    {
        if (_state == value) return;
        _state = value;
        
        XSpeed = TargetSpeed = value switch
        {
            TurtleState.Jumping or TurtleState.Walking => WalkSpeed,
            TurtleState.MovingShell => RollSpeed,
            TurtleState.StaticShell or _ => 0,
        };

        var disableOtherCollision = value == TurtleState.MovingShell;
        if (disableOtherCollision)
        {
            _collideWithOthersRecovery = CollideWithOthers;
            CollideWithOthers = false;
        }
        else if (_collideWithOthersRecovery is {} backup)
        {
            CollideWithOthers = backup;
            _collideWithOthersRecovery = null;
        }

        EmitSignal(SignalName.TurtleStateChanged);
    }

    private TurtleState _state = TurtleState.Walking;
    private bool? _collideWithOthersRecovery;
}