using ChloePrime.MarioForever.Util.Animation;
using Godot;

namespace ChloePrime.MarioForever.Util.HelperNodes;

[GlobalClass]
public partial class SelfDestroyingBetterAnimatedEffect : BetterAnimatedSprite2D
{
    [ExportCategory("Effect")] [Export] public Vector2 Velocity;
    
    public override void _Ready()
    {
        base._Ready();
        AnimationFinished += QueueFree;
    }

    public override void _Process(double delta)
    {
        Translate(Velocity * (float)delta);
        base._Process(delta);
    }
}