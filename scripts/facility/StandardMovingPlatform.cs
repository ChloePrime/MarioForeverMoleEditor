using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Facility;

[GlobalClass]
public partial class StandardMovingPlatform : MovingPlatform
{
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _staticSprite, NpStaticSprite);
        this.GetNode(out _animatedSprite, NpAnimatedSprite);
    }

    public void SetSprite(Texture2D sprite)
    {
        var size = sprite.GetSize();
        _staticSprite.Visible = true;
        _staticSprite.ProcessMode = ProcessModeEnum.Inherit;
        _staticSprite.Texture = sprite;
        _staticSprite.Position = new Vector2(0, size.Y / 2);
        _animatedSprite.Visible = false;
        _animatedSprite.ProcessMode = ProcessModeEnum.Disabled;
        Callable.From<Vector2>(UpdateCollisionSize).CallDeferred(size);
    }

    public void SetSprite(SpriteFrames frames, Vector2 size)
    {
        _animatedSprite.Visible = true;
        _animatedSprite.ProcessMode = ProcessModeEnum.Inherit;
        _animatedSprite.SpriteFrames = frames;
        _animatedSprite.Play(frames.HasAnimation(AnimDefault) ? AnimDefault : frames.GetAnimationNames()[0]);
        _animatedSprite.Position = new Vector2(0, size.Y / 2);
        _staticSprite.Visible = false;
        _staticSprite.ProcessMode = ProcessModeEnum.Disabled;
        Callable.From<Vector2>(UpdateCollisionSize).CallDeferred(size);
    }

    private void UpdateCollisionSize(Vector2 size)
    {
        if (CollisionShape.Shape is not RectangleShape2D shape || !Mathf.IsEqualApprox(shape.Size.X, size.X))
        {
            CollisionShape.Shape = new RectangleShape2D()
            {
                Size = size
            };
            CollisionShape.Position = new Vector2(0, size.Y / 2);
        }
    }

    private static readonly NodePath NpStaticSprite = "Static Sprite";
    private static readonly NodePath NpAnimatedSprite = "Animated Sprite";
    private static readonly StringName AnimDefault = "default";

    private Sprite2D _staticSprite;
    private AnimatedSprite2D _animatedSprite;
}