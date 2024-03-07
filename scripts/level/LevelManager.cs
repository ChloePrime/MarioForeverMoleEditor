using System;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Level.Darkness;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.UI;
using Godot;

namespace ChloePrime.MarioForever.Level;

public partial class LevelManager : Control
{
    [Export]
    public PackedScene Level
    {
        get => _level;
        set => SetLevel(value);
    }
    
    [Export] public PackedScene TestLevel { get; set; }
    [Export] public AudioStream GameOverJingle { get; set; }
    [Export] public PackedScene SceneAfterGameOver { get; set; }

    public Node LevelInstance { get; private set; }

    public bool DarknessEnabled
    {
        get => DarknessManager.Visible;
        set
        {
            DarknessManager.Visible = value;
            DarknessManager.ProcessMode = value ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        }
    }

    [ExportGroup("Advanced")]
    [Export] public DarknessManager DarknessManager { get; private set; }
    [Export] public SubViewport GameViewport { get; private set; }
    [Export] public LevelHud Hud { get; private set; }
    
    [Export] public GameRule GameRule { get; private set; }

    public void ReloadLevel()
    {
        foreach (var child in GameViewport.GetChildren())
        {
            child.QueueFree();
        }
        GameViewport.AddChild(LevelInstance = _level.Instantiate());
        if (LevelInstance is MaFoLevel mfl)
        {
            Hud.WorldName = mfl.LevelName;
            Hud.CurrentLevel = mfl;
            Hud.Visible = true;
            GlobalData.Time = GameRule.TimePolicy switch
            {
                GameRule.TimePolicyType.CountOnly => 0,
                _ => mfl.TimeLimit,
            };
        }
        else
        {
            Hud.CurrentLevel = null;
            Hud.Visible = false;
        }
        BackgroundMusic.Speed = 1;
        Hud.MegaManBossHpBar.Visible = false;
    }
    
    public void RestartGame()
    {
        Hud.GameOverLabel.Visible = false;
        GameRule.ResetGlobalData();
        if (Level == SceneAfterGameOver)
        {
            ReloadLevel();
        }
        else
        {
            Level = SceneAfterGameOver;
        }
    }

    public void GameOver()
    {
        Hud.GameOverLabel.Visible = true;
        BackgroundMusic.Music = GameOverJingle;
        using var timer = GetTree().CreateTimer(6, true, true);
        timer.Timeout += () => _waitingForRestart = true;
    }
    
    public override void _Ready()
    {
        base._Ready();
        GameRule.ResetGlobalData();
        ReloadLevel();
#if TOOLS
        if (TestLevel is { } level)
        {
            _level = level;
        }
#endif
        GameRule ??= GameRule.Default;
        GameRule.ResetHitPoint();
        ProcessPriority = -100;
        SceneAfterGameOver ??= Level;
        Hud.GameOverLabel.Visible = false;
        _ready = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (GameRule.DisableLives || GameRule.DisableCoin || !GameRule.CoinAutoExchangeLife)
        {
            return;
        }
        var cost = GameRule.CostOf1Life;
        var coins = GlobalData.Coins;
        if (coins < cost) return;
        
        AddLives(coins / cost);
        GlobalData.Coins = coins % cost;
    }

    private void AddLives(int count)
    {
        if (GameRule.AddLifeMethod is not { } scores ||
            GetTree()?.GetMario() is not { } mario ||
            mario.GetParent() is not { } level)
        {
            GlobalData.Lives += count;
            return;
        }

        var livesAddedByScore = Math.Min(count, scores.Count);
        var is2D = scores[livesAddedByScore - 1].TryInstantiate(out Node2D score2D, out var score);
        level.AddChild(score);
        if (is2D)
        {
            score2D.GlobalPosition = mario.ToGlobal(new Vector2(0, -32));
        }

        GlobalData.Lives += count - livesAddedByScore;
    }

    public override void _Input(InputEvent e)
    {
        base._Input(e);
        if (_waitingForRestart && IsAnyKeyPressed(e))
        {
            _waitingForRestart = false;
            RestartGame();
        }
    }

    private static bool IsAnyKeyPressed(InputEvent e)
    {
        return e is InputEventKey or InputEventJoypadButton && e.IsPressed();
    }

    protected void SetLevel(PackedScene level)
    {
        if (_level == level)
        {
            return;
        }
        _level = level;
        if (_ready)
        {
            ReloadLevel();
        }
    }

    private PackedScene _level;
    private bool _ready;
    private bool _waitingForRestart;
}