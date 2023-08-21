using ChloePrime.MarioForever.Player;
using Godot;

namespace ChloePrime.MarioForever.level;

public partial class MusicChanger : Area2D
{
    [Export] public AudioStream TargetMusic { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D other)
    {
        if (other is Mario && TargetMusic is {} music)
        {
            BackgroundMusic.Music = music;
        }
    }
}   