using Godot;

namespace ChloePrime.MarioForever.Level.Warp;

public partial class WarpTransition : CanvasLayer
{
    [Signal]
    public delegate void TransitionCompletedEventHandler();

    protected void CompleteTransition()
    {
        EmitSignal(SignalName.TransitionCompleted);
    }

    public void BeginTransition(WarpTransitionType type)
    {
        _BeginTransition(type);
    }

    public void ProcessTransition(float delta)
    {
        _ProcessTransition(delta);
    }
    
    public void EndTransition()
    {
        _EndTransition();
    }

    protected virtual void _BeginTransition(WarpTransitionType type)
    {
    }

    protected virtual void _ProcessTransition(float delta)
    {
    }

    protected virtual void _EndTransition()
    {
    }
}