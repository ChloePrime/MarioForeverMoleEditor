using Godot;

namespace ChloePrime.MarioForever.Player;

[GlobalClass]
public partial class MaFoCamera : Camera2D
{
	public bool BorderInitialized { get; set; }
	
	[Export]
	public Rect2 Border
	{
		get => _border;
		set => SetBorder(value);
	}

	private void SetBorder(Rect2 rect)
	{
		_border = rect;
		LimitLeft = (int)rect.Position.X;
		LimitTop = (int)rect.Position.Y;
		LimitRight = (int)rect.End.X;
		LimitBottom = (int)rect.End.Y;
		BorderInitialized = true;
	}
	
	private Rect2 _border = new(new Vector2(320, 240), new Vector2(640, 480));
}
