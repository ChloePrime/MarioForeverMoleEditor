using System;
using System.Linq;
using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
[Icon("res://resources/enemies/AT_clamp_icon.tres")]
public partial class ClampFlower : Node2D
{
    [Export] public Vector2 MoveDirection { get; private set; } = Vector2.Down;
    [Export] public float MoveDistance { get; private set; } = 56;
    [Export] public float MoveSpeed { get; set; } = Units.Speed.CtfToGd(2);
    [Export] public float WaitTimeUp { get; set; } = 0.5F;
    [Export] public float WaitTimeDown { get; set; } = 1.4F;
    [Export] public float ShyDetectDistance { get; set; } = 80;

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _waitTimer, NpWaitTimer);
        _waitTimer.Timeout += OnWaitTimerTimeout;
    }

    public override void _Process(double deltaD)
    {
        base._Process(deltaD);
        var delta = (float)deltaD;
        if (_moving)
        {
            var end = _rev ? 0 : MoveDistance;
            var offset = _moved.MoveToward(end, MoveSpeed * delta);
            Translate(new Vector2(0, offset));
            if (Mathf.IsEqualApprox(_moved, end))
            {
                _waitTimer.WaitTime = _rev ? WaitTimeUp : WaitTimeDown;
                _waitTimer.Start();
                _moving = false;
            }
        }
        else if (_waitingForMarioToLeave)
        {
            var isGrowBlocked = GetTree().GetNodesInGroup(MaFo.Groups.Player)
                .OfType<Node2D>()
                .Select(mario => ToLocal(mario.GlobalPosition))
                .Any(rp => Mathf.Abs(rp.X) < ShyDetectDistance);
            if (!isGrowBlocked)
            {
                SwapMovement();
            }
        }
    }

    private void OnWaitTimerTimeout()
    {
        if (_rev || ShyDetectDistance == 0)
        {
            SwapMovement();
        }
        else
        {
            _waitingForMarioToLeave = true;
        }
    }

    private void SwapMovement()
    {
        _rev = !_rev;
        _moving = true;
        _waitingForMarioToLeave = false;
    }

    private static readonly NodePath NpWaitTimer = "Wait Timer";
    private Timer _waitTimer;
    private bool _rev;
    private bool _moving = true;
    private float _moved;
    private bool _waitingForMarioToLeave;
}