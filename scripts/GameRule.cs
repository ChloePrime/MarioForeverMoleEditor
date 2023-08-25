using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever;

public partial class GameRule : Resource
{
    public static GameRule Default => _default ??= GD.Load<GameRule>("res://R_game_rule.tres");
    
    [Export] public bool DisableLives { get; set; }
    [Export] public bool DisableScore { get; set; }

    private static GameRule _default;
}

public static class GameRuleEx
{
    public static GameRule GetRule(this Node node) => node.GetLevelManager()?.GameRule ?? GameRule.Default;
}