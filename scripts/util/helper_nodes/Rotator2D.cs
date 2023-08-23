using System;
using Godot;

namespace ChloePrime.MarioForever.Util.HelperNodes;

[GlobalClass]
[Icon("res://resources/T_xfx.png")]
public partial class Rotator2D : Node
{
	[Export] public float Cycle { get; set; } = 0.64F;
	
	private Node2D _parent;
	private bool _valid;
	public override void _Ready()
	{
		_parent = GetParent() as Node2D;
		_valid = _parent != null;
	}

	public override void _Process(double delta)
	{
		if (!_valid) return;
		_parent.Rotate(Math.Sign(_parent.Scale.X) * (float)delta * 2 * Mathf.Pi / Cycle);
	}
}
