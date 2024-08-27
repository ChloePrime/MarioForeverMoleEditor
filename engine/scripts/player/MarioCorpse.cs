using System;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Player;

[GlobalClass]
[Icon("res://engine/resources/mario/AT_mario_corpse.tres")]
public partial class MarioCorpse : Sprite2D
{
	[Export] public float JumpStrength { get; set; } = Units.Speed.CtfToGd(10);
	[Export] public float Gravity { get; set; } = Units.Acceleration.CtfToGd(0.4F);
	[Export] public float MaxSpeed { get; set; } = Units.Speed.CtfToGd(10);
	[Export] public PackedScene GameOverScene { get; set; }

	public override void _Ready()
	{
		this.GetNode(out _startMoveTimer, NpStartMoveTimer);
		this.GetNode(out _restartLevelTimer, NpRestartLevelTimer);
		_startMoveTimer.Timeout += StartMoving;
		_restartLevelTimer.Timeout += OnDeathAnimOver;
	}

	public void SetFastRetry(bool fastRetry)
	{
		_fastRetry = fastRetry;
		Callable.From<bool>(SetFastRetry0).CallDeferred(fastRetry);
	}

	private void SetFastRetry0(bool fastRetry)
	{
		if (fastRetry)
		{
			_restartLevelTimer.Start(_restartLevelTimer.WaitTime / 10);
		}
	}

	private void OnDeathAnimOver()
	{
		var rule = this.GetRule();
		if (!rule.DisableLives)
		{
			switch (GlobalData.Lives)
			{
				case <= 0:
					GameOver();
					return;
				case > 0:
					GlobalData.Lives--;
					break;
			}
		}

		rule.ResetHitPoint();
		if (this.GetLevelManager() is { } levelManager)
		{
			levelManager.RetryLevel();
		}
		else
		{
			GetTree().ReloadCurrentScene();
		}
	}

	private void GameOver()
	{
		if (this.GetLevelManager() is not { } manager)
		{
			this.GetRule().ResetHitPoint();
			GetTree().ReloadCurrentScene();
			return;
		}
		if (_fastRetry)
		{
			manager.RestartGame();
		}
		else
		{
			manager.GameOver();
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

	private bool _fastRetry;
	private bool _moving;
	private float _ySpeed;
	private bool _restarting;
	private Timer _startMoveTimer;
	private Timer _restartLevelTimer;

	private static readonly NodePath NpStartMoveTimer = "Start Move Timer";
	private static readonly NodePath NpRestartLevelTimer = "Restart Level Timer";
}
