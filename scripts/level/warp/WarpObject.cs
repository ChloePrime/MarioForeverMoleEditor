using ChloePrime.MarioForever.Player;
using Godot;

namespace ChloePrime.MarioForever.Level.Warp;

[GlobalClass]
public partial class WarpObject : Node2D
{
    [Export] public WarpObject Target { get; set; }

    public void PrepareMarioExit(Mario mario)
    {
        mario.TransitionCompleted += () => _OnMarioArrived(mario);
    }

    protected virtual void _OnMarioArrived(Mario mario)
    {
        if (GetType() == typeof(WarpObject))
        {
            mario.PipeState = MarioPipeState.NotInPipe;
        }
    }
}