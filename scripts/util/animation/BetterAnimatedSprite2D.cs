using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Util.Animation;

[GlobalClass]
public partial class BetterAnimatedSprite2D : AnimatedSpriteWithPivot2D
{
    [Export] public Dictionary<StringName, AnimationData> AnimationData { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        AnimationChanged += () => OnFrameChanged(true);
        AnimationLooped += OnAnimLooped;
        FrameChanged += () => OnFrameChanged(false);
    }

    private void OnAnimLooped()
    {
        if (AnimationData.TryGetValue(Animation, out var data) && data.LoopStarts > 0)
        {
            Frame = data.LoopStarts;
        }
    }
    
    private void OnFrameChanged(bool newAnimation)
    {
        var frame = newAnimation ? 0 : Frame;
        Pivot pivot;
        if (!AnimationData.TryGetValue(Animation, out var data))
        {
            pivot = Pivot;
        }
        else
        {
            pivot = frame >= data.FramePivots.Count ? Pivot : data.FramePivots[frame].Or(Pivot);
        }
        if (pivot != Pivot.Default)
        {
            SnapToPivot(Animation, frame, pivot);
        }
    }
}