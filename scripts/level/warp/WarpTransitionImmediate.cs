using Godot;

namespace ChloePrime.MarioForever.Level.Warp;

public partial class WarpTransitionImmediate : WarpTransition
{
    protected override void _BeginTransition(WarpTransitionType type)
    {
        base._BeginTransition(type);
        Callable.From(CompleteTransition).CallDeferred();
    }
}