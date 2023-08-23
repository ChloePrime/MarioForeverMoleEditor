using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Util.Animation;

[GlobalClass]
public partial class BetterAnimatedSprite2D : AnimatedSpriteWithPivot2D
{
    [Export] public Vector2 GlobalOffset { get; set; } = Vector2.Zero;
    [Export] public Dictionary<StringName, AnimationData> AnimationData { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        AnimationChanged += OnAnimChanged;
        AnimationLooped += OnAnimLooped;
        FrameChanged += () => OnFrameChanged(false);
    }

    private void OnAnimLooped()
    {
        _loopCount++;
        if (!AnimationData.TryGetValue(Animation, out var data))
        {
            return;
        }
        if (data.LoopStarts > 0)
        {
            Frame = data.LoopStarts;
        }
        if (data.LoopCount > 0 && _loopCount >= data.LoopCount)
        {
            Stop();
            EmitSignal(AnimatedSprite2D.SignalName.AnimationFinished);
        }
    }

    private void OnAnimChanged()
    {
        _loopCount = 0;
        OnFrameChanged(true);
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
            Translate(GlobalOffset);
        }
    }

    private int _loopCount;
}