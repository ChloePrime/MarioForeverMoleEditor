using Godot;

namespace ChloePrime.MarioForever.Level.Darkness;

public partial class Light : Sprite2D
{
	[Export] public Node2D BoundObject { get; set; }
	
	public DarknessManager DarknessManager { get; internal set; }

	public void OnLightGC()
	{
		if (!IsInstanceValid(BoundObject))
		{
			QueueFree();
		}
	}

	public override void _Process(double delta)
	{
		if (BoundObject is not { } bound) return;
		
		if (!IsInstanceValid(bound))
		{
			QueueFree();
			return;
		}
		
		if (bound.IsInsideTree())
		{
			Visible = true;
			UpdatePosition(bound);
		}
		else
		{
			Visible = false;
		}
	}

	private void UpdatePosition(Node2D bound)
	{
		if (DarknessManager.LevelManager.LevelInstance is { } level &&
		    level.GetViewport() is { } viewport &&
		    viewport.GetCamera2D() is { } camera)
		{
			GlobalPosition = bound.GlobalPosition;
		}
		else
		{
			GlobalPosition = bound.GlobalPosition;
		}
	}
}
