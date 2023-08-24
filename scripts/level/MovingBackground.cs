using Godot;

namespace ChloePrime.MarioForever.Level;

[GlobalClass]
public partial class MovingBackground : LevelBackground
{
    [Export] public Vector2 Speed { get; set; }
    [Export] public Vector2 Acceleration { get; set; }
    [Export] public Vector2 MaxSpeed { get; set; } = Vector2.Inf;

    public override void _Process(double delta)
    {
        PositionOffset += Speed * (float)delta;
        base._Process(delta);
    }

    public override void _PhysicsProcess(double deltaD)
    {
        base._PhysicsProcess(deltaD);
        var delta = (float)deltaD;
        Speed += Acceleration * delta;
        Speed = Speed.Clamp(-MaxSpeed, MaxSpeed);
    }
}