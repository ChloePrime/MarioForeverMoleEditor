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

    public virtual void OnMarioTeleportedToHere(Mario mario)
    {
        ApplyTransitionType(mario);
        mario.TransitionCompleted += () =>
        {
            BeginExitingProcedure(mario);
            EmitSignal(SignalName.MarioArrived, mario);
        };
    }

    protected void ApplyTransitionType(Mario mario)
    {
        mario.WarpTransitionPrefab = TransitionOverride ?? mario.GameRule.DefaultTransitionPrefab;
    }

    protected virtual void BeginExitingProcedure(Mario mario)
    {
        if (GetType() == typeof(WarpObject))
        {
            mario.PipeState = MarioPipeState.NotInPipe;
        }
    }

    protected void MarioExitWarp(Mario mario)
    {
        mario.PipeState = MarioPipeState.NotInPipe;
        mario.XSpeed = mario.YSpeed = 0;
        mario.PipeForceAnimation = null;
        mario.ZIndex = mario.ZIndexBeforePipe;
        // 防止因为误差导致马里奥传送后被判定为卡在地面里
        mario.GlobalPosition -= new Vector2(0, 1.5F * mario.SafeMargin);
    }
}