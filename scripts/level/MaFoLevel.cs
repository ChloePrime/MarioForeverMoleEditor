using System.Collections.Generic;
using System.Linq;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Level;

/// <summary>
/// 初始化关卡：
/// 1. 让作为本节点直接子节点的 Tilemap 的碰撞层（名字为 Collision 的层）变为透明
/// </summary>
[GlobalClass]
public partial class MaFoLevel : Node
{
	[Export(PropertyHint.MultilineText)] public string LevelName { get; set; } = "Mole Editor";
	[Export] public double TimeLimit { get; set; } = 400;
	[Export] public bool TimeFlows { get; set; } = true;
	[Export] public MaFoLevelArea DefaultArea { get; private set; }

	[ExportGroup("MaFoLevel: Advanced")]
	[Export] public ObjectTilePresetList TileLoadingPreset { get; private set; }
	
	[Signal]
	public delegate void TimeoutEventHandler();
	
	public bool Completed { get; set; }
	
	public MaFoCamera FindCamera()
	{
		return this.Children().OfType<MaFoCamera>().FirstOrDefault(cam => cam.Enabled) ??
		       GetViewport().GetCamera2D() as MaFoCamera;
	}

	public override void _Ready()
	{
		DefaultArea ??= this.Children().OfType<MaFoLevelArea>().First();
		TileLoadingPreset ??= GD.Load<ObjectTilePresetList>("res://tiles/R_object_tile_presets.tres");

		if ((_manager = this.GetLevelManager()) is null)
		{
			GetTree().Root.ContentScaleAspect = Window.ContentScaleAspectEnum.Keep;
		}

		Timeout += () =>
		{
			if (_manager is not { } manager) return;
			if (manager.GameRule.TimePolicy == GameRule.TimePolicyType.Classic && GetTree() is {} tree)
			{
				_timeoutKillList ??= tree.GetAllPlayers().OfType<Mario>().ToList();
			}
		};

		AreasReady();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		ProcessTime(delta);
		ProcessTimeoutKill();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (!IsQueuedForDeletion()) return;
		foreach (var area in Areas)
		{
			if (area.GetParent() != this)
			{
				area.QueueFree();
			}
		}
	}

	private void ProcessTime(double delta)
	{
		if (!TimeFlows || Completed || _manager is not { } manager) return;
		switch (manager.GameRule.TimePolicy)
		{
			case GameRule.TimePolicyType.Disable:
			case GameRule.TimePolicyType.Date:
				return;
			case GameRule.TimePolicyType.CountOnly:
				GlobalData.Time += delta;
				return;
			case GameRule.TimePolicyType.Classic:
				GlobalData.Time -= delta / manager.GameRule.ClassicTimeUnitSize;
				break;
			case GameRule.TimePolicyType.Countdown:
			default:
				GlobalData.Time -= delta;
				break;
		}
		if (manager.GameRule.TimePolicy == GameRule.TimePolicyType.Classic)
		{
			if (GlobalData.Time <= 100 && !_timeHintEmitted)
			{
				manager.GameRule.TimeoutHintSound?.Play();
				manager.Hud.HintTimeout();
				BackgroundMusic.Speed = 1.5F;
				_timeHintEmitted = true;
			}
		}
		if (GlobalData.Time <= 0 && !_timeoutEmitted)
		{
			EmitSignal(SignalName.Timeout);
			_timeoutEmitted = true;
		}
	}

	/// <summary>
	/// 倒计时结束后循环杀死马里奥，仅限 Classic 计时模式才会触发
	/// </summary>
	private void ProcessTimeoutKill()
	{
		if (_timeoutKillList is not {} list) return;
		foreach (var mario in list)
		{
			mario.Kill(new DamageEvent
			{
				DamageTypes = 0,
				DirectSource = null,
				TrueSource = null,
			});
		}
	}
	
	private LevelManager _manager;
	private List<Mario> _timeoutKillList;
	private bool _timeoutEmitted;
	private bool _timeHintEmitted;
}
