using Godot;

namespace ChloePrime.MarioForever.Facility;

public abstract partial class MovingPlatformMark : Area2D
{
    [Export] public bool OneTimeUse { get; set; }
    
    
    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
    }

    protected abstract void OnPlatformEntered(MovingPlatform platform);

    private void OnBodyEntered(Node2D body)
    {
        if (body is MovingPlatform platform)
        {
            OnPlatformEntered(platform);
        }
    }

    protected void OnMarkUsed()
    {
        if (OneTimeUse)
        {
            QueueFree();
        }
    }
}