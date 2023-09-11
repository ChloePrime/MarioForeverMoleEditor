using System;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.UI;

public partial class Score: Sprite2D
{
    [Export] public long Amount { get; private set; } = 100;
    [Export] public float MoveDistance { get; set; } = 32;
    [Export] public float MoveSpeed { get; set; } = Units.Speed.CtfMovementToGd(9);

    public override void _Ready()
    {
        base._Ready();
        TakeEffect();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_distanceMoved >= MoveDistance)
        {
            return;
        }
        var dp = Math.Min(MoveDistance - _distanceMoved, MoveSpeed * (float)delta);
        _distanceMoved += dp;
        Translate(new Vector2(0, -dp));
    }

    public virtual void TakeEffect()
    {
        if (!this.GetRule().DisableScore)
        {
            GlobalData.Score += Amount;
        }
        else
        {
            Visible = false;
            QueueFree();
        }
    }

    private float _distanceMoved;
}