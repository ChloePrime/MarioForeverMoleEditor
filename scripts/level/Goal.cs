using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using DotNext.Collections.Generic;
using Godot;

namespace ChloePrime.MarioForever.Level;

public enum GoalType
{
    /// <summary>
    /// 通关杠
    /// </summary>
    Main,

    /// <summary>
    /// 小通关器
    /// </summary>
    Small,
    
    Max,
}

public partial class Goal : Node2D
{
    [Export] public GoalType Type { get; set; } = GoalType.Main;
    [Export] public AudioStream ClearJingle { get; set; } = GD.Load<AudioStream>("res://resources/level/ME_level_completed.ogg");
    
    private static readonly PackedScene Score100 = GD.Load<PackedScene>("res://objects/ui/O_score_100.tscn");
    private WeakRef _capturedPlayer;

    public void CompleteLevel()
    {
        BackgroundMusic.Music = ClearJingle;

        foreach (var player in GetTree().GetAllPlayers())
        {
            SetLevelCompleted(player, true);
            if (Type is GoalType.Small && player is Node2D player2d)
            {
                var score100 = Score100.Instantiate<Node2D>();
                player.GetPreferredRoot().AddChild(score100);
                score100.GlobalPosition = player2d.ToGlobal(new Vector2(0, -32));
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (FetchPlayer() is not { } player )
        {
            return;
        }
        if (Type == GoalType.Small)
        {
            ProcessSmallGoal(player);
        }
    }

    private void ProcessSmallGoal(Node2D player)
    {
        if (player is Mario mario && (mario.ComboJumpAsked || !mario.IsOnFloor()))
        {
            return;
        }
        var relPos = ToLocal(player.GlobalPosition);
        if (relPos.X > 0 && Mathf.Abs(relPos.Y) <= 0.5)
        {
            TryCompleteLevel(player);
        }
    }

    [return:MaybeNull]
    private Node2D FetchPlayer()
    {
        if (_capturedPlayer?.GetRef() is { VariantType: Variant.Type.Object } refer &&
            refer.AsGodotObject() is Node2D cp)
        {
            return cp;
        }
        if (GetTree().GetAllPlayers().OfType<Node2D>().FirstOrDefault() is {} np)
        {
            _capturedPlayer = WeakRef(np);
            return np;
        }
        
        return null;
    }

    private void TryCompleteLevel(Node2D player)
    {
        if (HasCompletedLevel(player)) return;
        CompleteLevel();
        if (Type is GoalType.Small)
        {
            var score100 = Score100.Instantiate<Node2D>();
            player.GetPreferredRoot().AddChild(score100);
            score100.GlobalPosition = player.ToGlobal(new Vector2(0, -32));
        }
    }

    private static bool HasCompletedLevel(GodotObject player)
    {
        if (player is Mario mario)
        {
            return mario.HasCompletedLevel;
        }
        else
        {
            return player.GetMeta(LevelCompletedMetaName) is { VariantType: Variant.Type.Bool } variant && variant.AsBool();
        }
    }

    private static void SetLevelCompleted(GodotObject player, bool value)
    {
        if (player is Mario mario)
        {
            mario.HasCompletedLevel = value;
        }
        else
        {
            player.SetMeta(LevelCompletedMetaName, value);
        }
    }

    private static readonly StringName LevelCompletedMetaName = "mfme:LevelCompleted";
}