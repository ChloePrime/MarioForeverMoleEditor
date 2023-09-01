using System.Linq;
using ChloePrime.MarioForever.Level;
using ChloePrime.MarioForever.Util;
using DotNext;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever;

public partial class GameRule : Resource
{
    public static GameRule Default => _default ??= GD.Load<GameRule>("res://R_game_rule.tres");
    public static GameRule Get() => GetManager(Engine.GetMainLoop() as SceneTree)?.GameRule ?? Default;
    
    [ExportGroup("Vanilla")]
    [Export] public bool DisableLives { get; set; }
    [Export] public bool DisableScore { get; set; }

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