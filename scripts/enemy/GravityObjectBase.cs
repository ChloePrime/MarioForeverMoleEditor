using System;
using System.Linq;
using ChloePrime.MarioForever.Player;
using Godot;
using ChloePrime.MarioForever.Util;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class GravityObjectBase : CharacterBody2D
{ 
	/// <see cref="ReallyEnabled"/> Enabled 并且不在出水管过程中
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
	
	[Export]
	public bool CollideWithOthers
	{
		get => GetCollisionLayerValue(MaFo.CollisionLayers.Enemy) || GetCollisionMaskValue(MaFo.CollisionLayers.Enemy);
		set
		{
			SetCollisionLayerValue(MaFo.CollisionLayers.Enemy, value);
			SetCollisionMaskValue(MaFo.CollisionLayers.Enemy, value);
		}
	}
	
	[Export] public Node2D Sprite { get; set; }

	[ExportGroup("Appearing")] 
	[Export] public float AppearSpeed { get; set; } = Units.Speed.CtfToGd(1);

	public Vector2 Size => _size ??= this.Children().OfType<CollisionShape2D>().First().Shape.GetRect().Size;
	public bool Appearing { get; private set; }
	public bool ReallyEnabled => Enabled && !Appearing;

	public void AppearFrom(Vector2 pipeNormal)
	{
		var distance = pipeNormal.Abs().MaxAxisIndex() == Vector2.Axis.X ? Size.X : Size.Y;
		Translate(-pipeNormal * distance);
		
		_zIndexBefore = ZIndex;
		ZIndex = -1;
		_appearNormal = pipeNormal;
		_appearDistance = distance;
		Appearing = true;
	}
	
	public bool HasHitWall { get; private set; }
	public float LastXSpeed { get; private set; }
	public float LastYSpeed { get; private set; }

	public override void _Ready()
	{
		base._Ready();
		Velocity = new Vector2(XSpeed * XDirection, YSpeed);
	}

	protected virtual void _ProcessCollision()
	{
	}

	private int _zIndexBefore;
	private Vector2 _appearNormal;
	private float _appearDistance;
	private Vector2? _size;

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (!Enabled)
		{
			return;
		}
		if (Appearing)
		{
			ProcessAppearing((float)delta);
		}
	}

	private void ProcessAppearing(float delta)
	{
		var offset = _appearDistance.MoveToward(0, AppearSpeed * delta);
		Translate(_appearNormal * offset);
		
		if (Mathf.IsZeroApprox(_appearDistance))
		{
			Appearing = false;
			ZIndex = _zIndexBefore;
		}
	}

	public override void _PhysicsProcess(double deltaD)
	{
		if (!Enabled || Appearing)
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
}
