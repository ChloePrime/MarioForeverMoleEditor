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
    
    [Export]
    public SubViewport GameViewport { get; private set; }

    public void ReloadLevel()
    {
        foreach (var child in GameViewport.GetChildren())
        {
            child.QueueFree();
        }
        GameViewport.AddChild(_level.Instantiate());
    }

    public override void _Ready()
    {
        base._Ready();
        ReloadLevel();
        _ready = true;
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
}