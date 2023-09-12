using Godot;

namespace ChloePrime.MarioForever.Player;

[GlobalClass]
public partial class MarioGrabMuzzle : Node2D
{
    public Node2D SyncTarget { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        ProcessPriority = 100;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (SyncTarget is { } synced)
        {
            synced.GlobalPosition = GlobalPosition;
        }
    }
}