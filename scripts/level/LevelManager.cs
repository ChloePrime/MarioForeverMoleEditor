using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Level;

public partial class LevelManager : Control
{
    [Export]
    public PackedScene Level
    {
        get => _level;
        set => SetLevel(value);
    }

    public void ReloadLevel()
    {
        foreach (var child in _viewport.GetChildren())
        {
            child.QueueFree();
        }
        _viewport.AddChild(_level.Instantiate());
    }

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _viewport, NpViewport);
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

    private static readonly NodePath NpViewport = "Aspect Ratio Container/SubViewport Container/SubViewport";
    
    private PackedScene _level;
    private SubViewport _viewport;
    private bool _ready;
}