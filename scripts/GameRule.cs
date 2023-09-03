﻿using System.Linq;
using ChloePrime.MarioForever.Level;
using ChloePrime.MarioForever.Util;
using DotNext;
using Godot;
using Godot.Collections;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever;

public partial class GameRule : Resource
{
    public static GameRule Default => _default ??= GD.Load<GameRule>("res://R_game_rule.tres");
    public static GameRule Get() => GetManager(Engine.GetMainLoop() as SceneTree)?.GameRule ?? Default;
    
    [ExportGroup("Vanilla")]
    [Export] public bool DisableLives { get; set; }
    [Export] public bool DisableScore { get; set; }
    
    [ExportSubgroup("Coins")]
    [Export] public bool DisableCoin { get; set; }
    [Export] public bool CoinAutoExchangeLife { get; set; } = true;
    [Export] public int CostOf1Life { get; set; } = 100;

    [Export]
    public Array<PackedScene> AddLifeMethod { get; set; } = new()
    {
        GD.Load<PackedScene>("res://objects/ui/O_1up.tscn")
    };
    
    [ExportGroup("Simple QoL")]
    [ExportSubgroup("Direction Calculation")]
    [Export] public MarioDirectionPolicy CharacterDirectionPolicy { get; set; } = MarioDirectionPolicy.FollowXSpeed;
    
    public enum MarioDirectionPolicy
    {
        FollowXSpeed,
        FollowControlDirection,
    }
    
    [ExportSubgroup("Toss Fireball Upward")]
    [Export] public bool EnableTossFireballUpward { get; set; } = true;
    [Export] public float TossFireballUpwardStrength { get; set; } = 400;

    [ExportGroup("Advanced")] 
    [Export] public bool EnableMarioBursting { get; set; } = true;

    [ExportSubgroup("Jump Height Bonus")]
    [Export] public float XSpeedBonus { get; set; } = Units.Speed.CtfToGd(1F);
    [Export] public float SprintingBonus { get; set; } = 0;

    [ExportSubgroup("Hit Point")]
    [Export] public HitPointPolicyType HitPointPolicy { get; set; } = HitPointPolicyType.Metroid;

    [Export] public bool HideHitPointAtZero { get; set; } = true;
    
    /// <summary>
    /// 如果为 false，那么玩家在 HP 归零后再次受伤会掉状态。
    /// 如果为 true，那么玩家在 HP 归零时会立刻死亡。
    /// </summary>
    [Export] public bool KillPlayerWhenHitPointReachesZero { get; set; }

    [Export] public bool HitPointProtectsYourPowerup { get; set; } = true;
    [Export] public bool CoinGivesHitPoint { get; set; } = true;
    [Export] public float MaxHitPointLo { get; set; } = 8;
    [Export] public float MaxHitPointHi { get; set; } = 400;
    [Export] public float DefaultTerrainDamageLo { get; set; } = 1;
    [Export] public float DefaultTerrainDamageHi { get; set; } = 16;

    public void ResetGlobalData()
    {
        GlobalData.ResetRuleNeutralValues();
        ResetHitPoint();
    }

    private static Optional<LevelManager> _manager;
    private static GameRule _default;

    private static LevelManager GetManager(SceneTree tree)
    {
        if (_manager.IsUndefined)
        {
            _manager = tree?.Root.Walk().OfType<LevelManager>().First();
        }
        return _manager.OrDefault();
    }
}

public static class GameRuleEx
{
    public static GameRule GetRule(this Node node) => node.GetLevelManager()?.GameRule ?? GameRule.Get();
}