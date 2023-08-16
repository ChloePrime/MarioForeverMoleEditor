using Godot;

namespace ChloePrime.MarioForever.Util.Animation;

public enum Pivot
{
	/// <summary>
	/// 交给引擎默认逻辑判断
	/// </summary>
	Default,
	TopLeft,
	Top,
	TopRight,
	Left,
	Center,
	Right,
	BottomLeft,
	Bottom,
	BottomRight,
}

public static class PivotEx
{
	public static Vector2 GetRelativePos(this Pivot pivot) => PivotPrivates.RelativePosTable[(int)pivot];

	public static Pivot Or(this Pivot self, Pivot other)
	{
		return self == Pivot.Default ? other : self;
	}
}

[GlobalClass]
public partial class AnimatedSpriteWithPivot2D : AnimatedSprite2D
{
	[Export] public Pivot Pivot { get; private set; }

	public void SnapToPivot(StringName animation, int frame, Pivot pivot)
	{
		if (pivot == Pivot.Default)
		{
			return;
		}
		var frameTex = SpriteFrames?.GetFrameTexture(animation, frame);
		if (frameTex == null)
		{
			return;
		}
		var spriteSize = frameTex.GetSize();
		Position = -spriteSize * (Centered ? pivot.GetRelativePos() - CenterPivotRp : pivot.GetRelativePos());
	}
	
	public override void _Ready()
	{
		base._Ready();
		if (GetType() == typeof(AnimatedSpriteWithPivot2D))
		{
			AnimationChanged += () => OnFrameChanged(true);
			FrameChanged += () => OnFrameChanged(false);
		}
	}

	private void OnFrameChanged(bool animChanged)
	{
		SnapToPivot(Animation, animChanged ? 0 : Frame, Pivot);
	}

	private static readonly Vector2 CenterPivotRp = Pivot.Center.GetRelativePos();
}

file static class PivotPrivates
{
	internal static readonly Vector2[] RelativePosTable = 
	{
		default,
		new(0.0F, 0.0F),
		new(0.5F, 0.0F),
		new(1.0F, 0.0F),
		new(0.0F, 0.5F),
		new(0.5F, 0.5F),
		new(1.0F, 0.5F),
		new(0.0F, 1.0F),
		new(0.5F, 1.0F),
		new(1.0F, 1.0F),
	};
}