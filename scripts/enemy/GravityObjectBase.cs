using System;
using System.Linq;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Shared;
using Godot;
using ChloePrime.MarioForever.Util;
using DotNext.Collections.Generic;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class GravityObjectBase : CharacterBody2D, IGrabbable
{
	/// <see cref="ReallyEnabled"/> Enabled 并且不在出水管过程中
	[Export]
	public bool Enabled { get; set; }

	/// <summary>
	/// X 速度，永远为正
	/// </summary>
	/// <see cref="XDirection"/>
	[Export]
	public float XSpeed { get; set; }
	
	[Export] public float TargetSpeed { get; set; }

	/// <summary>
	/// Y 速度，可能为负值
	/// </summary>
	[ExportGroup($"{nameof(GravityObjectBase)}")] 
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

	[ExportGroup("Appearing")] [Export] public float AppearSpeed { get; set; } = Units.Speed.CtfToGd(1);

	[Signal]
	public delegate void HitEnemyWhenThrownEventHandler(EnemyCore hit, bool isKiss);

	public CollisionShape2D Shape => _shape ??= this.Children().OfType<CollisionShape2D>().First();
	public Vector2 Size => _size ??= Shape.Shape.GetRect().Size;
	public Vector2 VelocityVector => new(XSpeed * XDirection, YSpeed);
	public bool Appearing { get; private set; }
	public bool ReallyEnabled => Enabled && !Appearing;
	public virtual bool CanMove => ReallyEnabled && !(this as IGrabbable).IsGrabbed;
	public virtual bool AutoDestroy => false;
	public virtual float AnimationDirection => XDirection;

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
	public bool WasThrown { get; private set; }
	public bool WasShot { get; private set; }
	public virtual bool WillHurtOthers => WasThrown || WasShot || IGrabbable.IsGrabbedByPlayer(this);

	public float LastXSpeed { get; private set; }
	public float LastYSpeed { get; private set; }

	public override void _Ready()
	{
		base._Ready();
		Grabbed += OnGrabbed;
		GrabReleased += OnGrabReleased;
		Velocity = new Vector2(XSpeed * XDirection, YSpeed);

		if (!Appearing)
		{
			CallDeferred(MethodName._ReallyReady);
		}
	}

	protected virtual void _ProcessCollision(float delta)
	{
		var hitSomething = false;
		
		if (IsOnWall())
		{
			XDirection *= -1;
			hitSomething = true;
		}

		if (!WasThrown) return;

		if (!IsOnFloor())
		{
			foreach (var collision in this.GetSlideCollisions())
			{
				if (collision.GetCollider() is IBumpable bumpable)
				{
					bumpable.OnBumpBy(this);
					hitSomething = true;
				}
			}
		}
		if (IsOnFloor() && LastYSpeed > 50)
		{
			YSpeed = -LastYSpeed / 4;
			hitSomething = true;
		}

		if (hitSomething)
		{
			SetDeferred(PropertyName.WasThrown, false);
		}
	}

	private void TryHitOverlappedEnemyWhenThrown(bool isKiss)
	{
		Shape.IntersectTyped(new PhysicsShapeQueryParameters2D
		{
			CollideWithAreas = true,
			CollideWithBodies = false,
			CollisionMask = MaFo.CollisionMask.Enemy,
		}).Select(result => result.Collider)
			.OfType<EnemyHurtDetector>()
			.Where(ehd => ehd.Core.Root != this)
			.ForEach(ehd =>
			{
				EmitSignal(SignalName.HitEnemyWhenThrown, ehd.Core, isKiss);
			});

		if (WasShot && !WasThrown)
		{
			var isSpeedUsual = (Mathf.IsZeroApprox(TargetSpeed) && Mathf.IsZeroApprox(XSpeed)) ||
			                   (XSpeed - TargetSpeed <= TargetSpeed);
			if (isSpeedUsual)
			{
				SetDeferred(PropertyName.WasShot, false);
				OnShotEnd();
			}
		}
	}

	private void TryHitOverlappedHiddenBonusWhenThrown()
	{
		Shape.IntersectTyped(new PhysicsShapeQueryParameters2D
			{
				CollideWithAreas = false,
				CollideWithBodies = true,
				CollisionMask = MaFo.CollisionMask.HiddenBonus,
			}).Select(result => result.Collider)
			.OfType<IBumpable>()
			.ForEach(b => b.OnBumpBy(this));
	}
	
	/// <summary>
	/// 在该物件完全从水管中钻出后触发。
	/// </summary>
	public virtual void _ReallyReady()
	{
	}

	private int _zIndexBefore;
	private uint _collLayerBeforeGrab;
	private uint _collMaskBeforeGrab;
	private bool? _collWithOthersBefore;
	private Vector2 _appearNormal;
	private float _appearDistance;
	private Vector2? _size;
	private CollisionShape2D _shape;

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

	private void OnGrabbed(Mario.GrabEvent _)
	{
		_collWithOthersBefore = CollideWithOthers;
		CollideWithOthers = false;
		
		_collLayerBeforeGrab = CollisionLayer;
		_collMaskBeforeGrab = CollisionMask;
		CollisionLayer = 0;
		CollisionMask = 0;
	}

	private void OnGrabReleased(Mario.GrabReleaseEvent e)
	{
		if (CollisionMask == 0 && _collMaskBeforeGrab != 0)
		{
			CollisionMask = _collMaskBeforeGrab;
			_collMaskBeforeGrab = 0;
		}
		if (e.Flags == Mario.GrabReleaseFlags.Gently)
		{
			OnShotEnd();
		}
		else
		{
			WasThrown = true;
			WasShot = true;
		}
	}

	private void OnShotEnd()
	{
		if (CollisionLayer == 0 && _collLayerBeforeGrab != 0)
		{
			CollisionLayer = _collLayerBeforeGrab;
			_collLayerBeforeGrab = 0;
		}
		if (_collWithOthersBefore is { } collWithOthersBefore)
		{
			CollideWithOthers = collWithOthersBefore;
			_collWithOthersBefore = null;
		}
	}

	private void ProcessAppearing(float delta)
	{
		var wouldFinish = Mathf.IsZeroApprox(_appearDistance);

		var offset = wouldFinish
			? AppearSpeed * delta
			: -_appearDistance.MoveToward(0, AppearSpeed * delta);
		Translate(_appearNormal * offset);

		if (Mathf.IsZeroApprox(_appearDistance) && !TestMove(GlobalTransform, DeMargin))
		{
			Appearing = false;
			ZIndex = _zIndexBefore;
			_ReallyReady();
		}
	}

	private static readonly Vector2 DeMargin = new(0, 0.08F);

	public override void _PhysicsProcess(double deltaD)
	{
		var willHurtOthers = WillHurtOthers;
		if (ReallyEnabled && willHurtOthers)
		{
			TryHitOverlappedEnemyWhenThrown((this as IGrabbable).IsGrabbed);
		}
		
		var grabbed = (this as IGrabbable).IsGrabbed;
		if (!Enabled || Appearing || grabbed)
		{
			return;
		}
		
		if (!CanMove) return;

		var delta = (float)deltaD;
		if (!IsOnFloor() || YSpeed < 0)
		{
			YSpeed += Gravity * delta;
		}

		Velocity = new Vector2(XSpeed * XDirection, YSpeed);
		var collided = MoveAndSlide();

		TryAutoDestroy();
		if (IsQueuedForDeletion()) return;

		HasHitWall = Math.Abs(Velocity.X) < XSpeed;
		LastXSpeed = XSpeed;
		LastYSpeed = YSpeed;
		XSpeed = Math.Abs(Velocity.X);
		YSpeed = IsOnFloor() ? 0 : Velocity.Y;

		if (willHurtOthers)
		{
			TryHitOverlappedEnemyWhenThrown(false);
			TryHitOverlappedHiddenBonusWhenThrown();
		}
		
		if (collided)
		{
			_ProcessCollision(delta);
		}

		if (Sprite is { } sprite)
		{
			sprite.Scale = XDirection > 0 ? Mario.Constants.DoNotFlipX : Mario.Constants.FlipX;
		}
	}

	private void TryAutoDestroy()
	{
		if (!AutoDestroy) return;
		if (GetViewport().GetCamera2D() is not { } camera) return;
		if (Position.Y > camera.LimitBottom + 64)
		{
			QueueFree();
		}
	}

	// IGrabbable

	public Node2D Grabber { get; set; }
	public event Action<Mario.GrabEvent> Grabbed;
	public event Action<Mario.GrabReleaseEvent> GrabReleased;

	public void GrabNotify(Mario.GrabEvent e, Mario.GrabReleaseEvent? flags)
	{
		IGrabbable.GrabNotifyImpl(Grabbed, GrabReleased, e, flags);
	}
}
