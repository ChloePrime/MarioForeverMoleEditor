using Godot;

namespace ChloePrime.MarioForever.Level.Darkness;

public partial class Light : Sprite2D
{
	[Export] public Node2D BoundObject { get; set; }
	[Export] public bool Animated { get; set; }
	
	public DarknessManager DarknessManager { get; internal set; }

	public void OnLightGC()
	{
		if (!IsInstanceValid(BoundObject))
		{
			QueueFree();
		}
	}

	private bool _destroying;

	public override void _Process(double delta)
	{
		if (_destroying || BoundObject is not { } bound) return;
		
		if (!IsInstanceValid(bound))
		{
			Destroy();
			return;
		}
		
		if (bound.IsInsideTree())
		{
			Visible = true;
			GlobalPosition = bound.GlobalPosition;
		}
		else
		{
			Visible = false;
		}
	}

	private void Destroy()
	{
		if (_destroying || !Animated)
		{
			QueueFree();
			return;
		}

		var tween = CreateTween();
		const float animTime = 0.5F;
		tween.TweenProperty(this, (string)Node2D.PropertyName.Scale, Vector2.Zero, animTime)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
		tween.TweenCallback(Callable.From(QueueFree)).SetDelay(animTime);
	}
}
