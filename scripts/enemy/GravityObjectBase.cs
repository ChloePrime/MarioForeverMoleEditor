using System;
using ChloePrime.MarioForever.Player;
using Godot;
using ChloePrime.MarioForever.Util;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class GravityObjectBase : CharacterBody2D
{ 
	[Export] public bool Enabled { get; set; }
	
	/// <summary>
	/// X 速度，永远为正
	/// </summary>
	/// <see cref="XDirection"/>
	[Export]
	public float XSpeed { get; set; }
	
	/// <summary>
	/// Y 速度，可能为负值
	/// </summary>
	[Export]
	public float YSpeed { get; set; }

	/// <summary>
	/// X 方向的运动方向，为 1 或 -1
	/// </summary>
	[Export(PropertyHint.Enum, "Left:-1,Right:1")]
	public float XDirection { get; set; } = -1;
	
	[Export] public float MaxYSpeed { get; set; } = Units.Speed.CtfToGd(10);
	[Export] public float Gravity { get; set; } = Units.Acceleration.CtfToGd(0.4F);
	[Export] public Node2D Sprite { get; set; }
	
	public bool HasHitWall { get; private set; }
	public float LastXSpeed { get; private set; }
	public float LastYSpeed { get; private set; }

	public override void _Ready()
	{
		base._Ready();
		Velocity = new Vector2(XSpeed * XDirection, YSpeed);
	}

	public override void _PhysicsProcess(double deltaD)
	{
		if (!Enabled)
		{
			return;
		}

		var delta = (float)deltaD;
		if (!IsOnFloor() || YSpeed < 0)
		{
			YSpeed += Gravity * delta;
		}

		Velocity = new Vector2(XSpeed * XDirection, YSpeed);
		var collided = MoveAndSlide();

		HasHitWall = Math.Abs(Velocity.X) < XSpeed;
		LastXSpeed = XSpeed;
		LastYSpeed = YSpeed;
		XSpeed = Math.Abs(Velocity.X);
		YSpeed = IsOnFloor() ? 0 : Velocity.Y;

		if (collided)
		{
			_ProcessCollision();
		}

		if (Sprite is { } sprite)
		{
			sprite.Scale = XDirection > 0 ? Mario.Constants.DoNotFlipX : Mario.Constants.FlipX;
		}
	}

	protected virtual void _ProcessCollision()
	{
	}
}
