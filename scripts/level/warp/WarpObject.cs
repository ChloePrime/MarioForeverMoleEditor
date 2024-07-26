#nullable enable
using ChloePrime.MarioForever.Player;
using Godot;

namespace ChloePrime.MarioForever.Level.Warp;

[GlobalClass]
public partial class WarpObject : Node2D
{
    [Export] public WarpObject? Target { get; set; }
    [Export] public PackedScene? TransitionOverride { get; set; }
    [Signal] public delegate void MarioArrivedEventHandler(Mario mario);

    public void PrepareMarioExit(Mario mario)
    {
        mario.TransitionCompleted += () =>
        {
            ApplyTransitionType(mario);
            _OnMarioArrived(mario);
            EmitSignal(SignalName.MarioArrived, mario);
        };
    }

    protected void ApplyTransitionType(Mario mario)
    {
        mario.WarpTransitionPrefab = TransitionOverride ?? mario.GameRule.DefaultTransitionPrefab;
    }

    protected virtual void _OnMarioArrived(Mario mario)
    {
        if (GetType() == typeof(WarpObject))
        {
            mario.PipeState = MarioPipeState.NotInPipe;
        }
    }
}