using System.Collections.Generic;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class LakituMovementComponent : Node
{
    [Export] public float SpeedHi { get; set; } = Units.Speed.CtfToGd(9);
    [Export] public float SpeedLo { get; set; } = Units.Speed.CtfToGd(2);
    [Export] public float AccRangeHi { get; set; } = 100;
    [Export] public float AccRangeLo { get; set; } = 50;
    [Export] public float AccelerationHi { get; set; } = Units.Acceleration.CtfToGd(1);
    [Export] public float AccelerationLo { get; set; } = Units.Acceleration.CtfToGd(0.2F);

    public float XSpeed => Mathf.Abs(_speed);
    public float XDirection => Mathf.Sign(_speed);

    public override void _Ready()
    {
        base._Ready();
        _lakitu = GetParent<Node2D>();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        var mario = _mario ??= GetTree().GetMario();
        if (!IsInstanceValid(mario))
        {
            _mario = null;
            return;
        }
        var rel = _lakitu.ToLocal(mario.GlobalPosition).X;
        if (rel > +AccRangeLo && _speed < +SpeedHi)
        {
            _speed.MoveToward(+SpeedHi, AccelerationLo * (float)delta);
        }
        if (rel < -AccRangeLo && _speed > -SpeedHi)
        {
            _speed.MoveToward(-SpeedHi, AccelerationLo * (float)delta);
        }
        if (rel >= -AccRangeHi && rel <= AccRangeHi)
        {
            var marioDir = mario is Mario m ? m.XDirection : 0;
            switch (marioDir)
            {
                case > 0 when _speed < -SpeedLo:
                    _speed.MoveToward(-SpeedLo, AccelerationHi * (float)delta);
                    break;
                case < 0 when _speed > +SpeedLo:
                    _speed.MoveToward(+SpeedLo, AccelerationHi * (float)delta);
                    break;
            }
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _lakitu.Translate(new Vector2(_speed * (float)delta, 0));
    }

    private float _speed;
    private Node2D _mario;
    private Node2D _lakitu;
}