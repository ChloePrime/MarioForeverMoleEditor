using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Util.Animation;

[GlobalClass]
public partial class AnimationData : Resource
{
    [Export] public int LoopStarts { get; set; }
    [Export] public Array<Pivot> FramePivots { get; private set; }

    /// <summary>
    /// 尚未实现
    /// </summary>
    [Export]
    public Array<Vector2> FramePivotVectors { get; private set; }
}