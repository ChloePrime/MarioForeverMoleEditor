using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class TurtleMarioWorkerFlyMovementComponent : TurtleFlyMovementComponent
{
    public override void _Ready()
    {
        base._Ready();
        Turtle.TurtleStateChanged += OnTurtleStateChanged;
        OnTurtleStateChanged();
    }

    public override void _ProcessMovement(float delta)
    {
        Turtle.Velocity = new Vector2(Turtle.WalkSpeed * Turtle.XDirection, 0);
        Turtle.MoveAndSlide();
        if (Turtle.IsOnWall())
        {
            Turtle.XDirection *= -1;
        }
    }

    private void OnTurtleStateChanged()
    {
        Turtle.MotionMode = Turtle.State is Turtle.TurtleState.Flying
            ? CharacterBody2D.MotionModeEnum.Floating
            : CharacterBody2D.MotionModeEnum.Grounded;
    }
}