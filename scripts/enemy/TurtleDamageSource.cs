using System;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class TurtleDamageSource : EnemyDamageSource
{
    public override void _Ready()
    {
        base._Ready();
        _core = (TurtleEnemyCore)Core;
        _core.Turtle.TurtleStateChanged += OnTurtleStateChanged;
    }

    private void OnTurtleStateChanged()
    {
        if (_core.Turtle.State == Turtle.TurtleState.MovingShell)
        {
            HaltFor(TimeSpan.FromSeconds(0.25));
        }
    }

    public override void HurtMario(Mario mario)
    {
        if (_core.Turtle.State == Turtle.TurtleState.StaticShell)
        {
            CallDeferred(MethodName.KickOrGrab, mario);
        }
        else
        {
            base.HurtMario(mario);
        }
    }

    private void KickOrGrab(Mario mario)
    {
        if (Input.IsActionPressed(Mario.Constants.ActionRun) && mario.GameRule.EnableActiveGrabbing)
        {
            mario.Grab(_core.Turtle);
        }
        else
        {
            (_core.HurtDetector as TurtleHurtDetector)?.KickSound.Play();
            _core.Turtle.State = Turtle.TurtleState.MovingShell;
            _core.Turtle.KickBy(mario);
        }
    }
    
    private TurtleEnemyCore _core;
}