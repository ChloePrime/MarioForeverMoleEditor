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
    [Export] public SubViewport GameViewport { get; private set; }
    [Export] public LevelHud Hud { get; private set; }
    
    [Export] public GameRule GameRule { get; private set; }
    [Export] public AudioStream GameOverJingle { get; set; }
    [Export] public PackedScene SceneAfterGameOver { get; set; }

    public void ReloadLevel()
    {
        foreach (var child in GameViewport.GetChildren())
        {
            child.QueueFree();
        }
        GameViewport.AddChild(_level.Instantiate());
    }
    
    public void RestartGame()
    {
        Hud.GameOverLabel.Visible = false;
        GlobalData.Reset();
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
        ReloadLevel();
#if TOOLS
        if (TestLevel is { } level)
        {
            _level = level;
        }
#endif
        GameRule ??= GameRule.Default;
        SceneAfterGameOver ??= Level;
        Hud.GameOverLabel.Visible = false;
        _ready = true;
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

    private void SetLevel(PackedScene level)
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