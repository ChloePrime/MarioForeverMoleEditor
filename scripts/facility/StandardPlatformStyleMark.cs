using Godot;

namespace ChloePrime.MarioForever.Facility;

public partial class StandardPlatformStyleMark: Area2D
{
    [Export] public Texture2D StaticSprite { get; set; }
    [Export] public SpriteFrames AnimatedSprite { get; set; }
    [Export] public Vector2 AnimatedSpriteSize { get; set; } = new(96, 16);
    
    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not StandardMovingPlatform platform) return;
        if (StaticSprite is { } ss)
        {
            platform.SetSprite(ss);
        }
        else if (AnimatedSprite is { } @as)
        {
            platform.SetSprite(@as, AnimatedSpriteSize);
        }
        QueueFree();
    }
}