using Godot;

namespace ChloePrime.MarioForever.Facility;

public partial class MovingPlatformMark : Area2D
{
    [Export] public bool OneTimeUse { get; set; }

    protected void OnMarkUsed()
    {
        if (OneTimeUse)
        {
            QueueFree();
        }
    }
}