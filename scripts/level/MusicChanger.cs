using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.level;

public partial class MusicChanger : Area2D
{
    [Export] public AudioStream TargetMusic { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        FindLevelFrom(this);
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D other)
    {
        if (other is Mario && _level is { } level && TargetMusic is {} music)
        {
            level.LevelMusic = music;
        }
    }

    private void FindLevelFrom(Node node)
    {
        _level = this.GetLevel();
    }

    private MaFoLevel _level;
}   