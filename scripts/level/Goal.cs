using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ChloePrime.Godot.Util;
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
    [Export] public PackedScene ExitTarget { get; set; }
    [Export] public AudioStream ClearJingle { get; set; } = GD.Load<AudioStream>("res://resources/level/ME_level_completed.ogg");
    
    [ExportSubgroup("Advanced")]
    [Export]
    public GoalType Type { get; set; } = GoalType.Main;
    
    private static readonly PackedScene Score100 = GD.Load<PackedScene>("res://objects/ui/O_score_100.tscn");
    private static readonly AudioStream TimeSettlingSound = GD.Load<AudioStream>("res://resources/level/SE_time_settling.wav");
    private WeakRef _capturedPlayer;

    public async void CompleteLevel()
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

        if (this.GetLevel() is { } level)
        {
            level.Completed = true;
        }
        else
        {
            return;
        }

        await this.DelayAsync(8, false);
        if (!IsInstanceValid(this) || !IsInstanceValid(level)) return;
        if (level.GetLevelManager() is not {} manager) return;

        double timeUnit;
        switch (manager.GameRule.TimePolicy)
        {
            case GameRule.TimePolicyType.Classic:
                timeUnit = manager.GameRule.ClassicTimeUnitSize;
                break;
            case GameRule.TimePolicyType.Countdown:
                timeUnit = 1;
                break;
            default:
                return;
        }
        
        PlayTimeSettlingSound();

        while (GlobalData.Time > 0)
        {
            await this.DelayAsync(0.02F, false);
            for (var i = 0; i < 8; i++)
            {
                if (GlobalData.Time > 0)
                {
                    GlobalData.Time = Math.Max(0, GlobalData.Time - timeUnit);
                    GlobalData.Score += 10;
                }
            }
        }

        await this.DelayAsync(1);
        SwitchToNextLevel();
    }

    public void SwitchToNextLevel()
    {
        Checkpoint.ClearSaved();
        if (this.GetLevelManager() is not { } manager) return;

        manager.Level = ExitTarget ?? GD.Load<PackedScene>("res://resources/level/limbo.tscn");
    }

    private async void PlayTimeSettlingSound()
    {
        while (GlobalData.Time > 0)
        {
            TimeSettlingSound.Play();
            await this.DelayAsync(0.1F, false);
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