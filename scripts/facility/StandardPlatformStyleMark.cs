using Godot;

namespace ChloePrime.MarioForever.Facility;

public partial class StandardPlatformStyleMark : MovingPlatformMark
{
    [Export] public Texture2D StaticSprite { get; set; }
    [Export] public SpriteFrames AnimatedSprite { get; set; }
    [Export] public Vector2 AnimatedSpriteSize { get; set; } = new(96, 16);

    protected override void OnPlatformEntered(MovingPlatform body)
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
        OnMarkUsed();
    }
}