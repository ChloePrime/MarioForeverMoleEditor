using ChloePrime.MarioForever.Shared;
using ChloePrime.MarioForever.Util;
using ChloePrime.MarioForever.Util.HelperNodes;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

/// <summary>
/// 普通的尸体
/// </summary>
public partial class GenericCorpse : SimpleNoClipGravityObject, ICorpse
{
    public AnimatedSprite2D Sprite => _sprite ??= GetNode<AnimatedSprite2D>(NpSprite);
    public Rotator2D Rotator => _rotator ??= GetNode<Rotator2D>(NpRotator);

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (GlobalPosition.Y >= this.GetFrame().End.Y + Sprite.GetSpriteSize().Y)
        {
            QueueFree();
        }
    }

    private static readonly NodePath NpSprite = "Sprite";
    private static readonly NodePath NpRotator = "Rotator";
    private AnimatedSprite2D _sprite;
    private Rotator2D _rotator;
}