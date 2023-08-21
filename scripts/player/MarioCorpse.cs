using System;
using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Player;

[GlobalClass]
[Icon("res://resources/mario/T_mario_corpse.png")]
public partial class MarioCorpse : Sprite2D
{
	[Export] public float JumpStrength { get; set; } = Units.Speed.CtfToGd(10);
	[Export] public float Gravity { get; set; } = Units.Acceleration.CtfToGd(0.4F);
	[Export] public float MaxSpeed { get; set; } = Units.Speed.CtfToGd(10);

	public override void _Ready()
	{
		this.GetNode(out _startMoveTimer, NpStartMoveTimer);
		this.GetNode(out _restartLevelTimer, NpRestartLevelTimer);
		_startMoveTimer.Timeout += StartMoving;
		_restartLevelTimer.Timeout += () => GetTree().ReloadCurrentScene();
	}

	public void SetFastRetry(bool fastRetry)
	{
		CallDeferred(MethodName.SetFastRetry0, fastRetry);
	}

	private void SetFastRetry0(bool fastRetry)
	{
		if (fastRetry)
		{
			_restartLevelTimer.Start(_restartLevelTimer.WaitTime / 10);
		}
	}

	private void StartMoving()
	{
		_moving = true;
		_ySpeed = -JumpStrength;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_moving)
		{
			Translate(new Vector2(0, _ySpeed*(float)delta));
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		_ySpeed = Math.Min(MaxSpeed, _ySpeed + Gravity * (float)delta);
	}

	private bool _moving;
	private float _ySpeed;
	private bool _restarting;
	private Timer _startMoveTimer;
	private Timer _restartLevelTimer;

	private static readonly NodePath NpStartMoveTimer = "Start Move Timer";
	private static readonly NodePath NpRestartLevelTimer = "Restart Level Timer";
}
