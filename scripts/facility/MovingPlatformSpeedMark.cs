using Godot;

namespace ChloePrime.MarioForever.Facility;

public partial class MovingPlatformSpeedMark : MovingPlatformMark
{
    [Export] public Vector2 TargetVelocity { get; set; }
    [Export] public bool ModifyXVelocity { get; set; } = true;
    [Export] public bool ModifyYVelocity { get; set; } = true;

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not MovingPlatform platform) return;
        if (ModifyXVelocity && ModifyYVelocity)
        {
            platform.Velocity = TargetVelocity;
        }
        else
        {
            var originalVelocity = platform.Velocity;
            platform.Velocity = new Vector2(
                ModifyXVelocity ? TargetVelocity.X : originalVelocity.X,
                ModifyYVelocity ? TargetVelocity.Y : originalVelocity.Y
            );
        }
        OnMarkUsed();
    }
}