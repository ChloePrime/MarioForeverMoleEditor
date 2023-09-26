using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class TurtleTrigonometricFlyMovementComponent : TurtleFlyMovementComponent
{
    [Export] public Vector2 AxisRadius { get; set; } = new(0, 50);
    [Export] public float AngularSpeed { get; set; } = 2;
    [Export] public float Phase { get; set; }
    [Export] public bool RandomizePhase { get; set; } = true;

    public override void _Ready()
    {
        base._Ready();
        if (RandomizePhase)
        {
            var phase = Phase = GD.Randf() * Mathf.Tau;
            var (sin, cos) = Mathf.SinCos(phase);
            Turtle.Translate(new Vector2(cos * AxisRadius.X, -sin * AxisRadius.Y));
        }
    }

    public override void _ProcessMovement(float delta)
    {
        var (sin0, cos0) = Mathf.SinCos(Phase);
        Phase += Units.AngularSpeed.CtfToGd(AngularSpeed) * delta;
        var (sin1, cos1) = Mathf.SinCos(Phase);
        Turtle.Translate(new Vector2((cos1 - cos0) * AxisRadius.X, -(sin1 - sin0) * AxisRadius.Y));
    }
}