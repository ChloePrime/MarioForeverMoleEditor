using ChloePrime.MarioForever.Shared;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class BulletBillCorpse : SimpleNoClipGravityObject
{
    public AnimatedSprite2D Sprite => _sprite ??= GetNode<AnimatedSprite2D>(NpSprite);

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (GlobalPosition.Y >= this.GetFrame().End.Y + Sprite.GetSpriteSize().Y)
        {
            QueueFree();
            return;
        }
        var atan2 = Mathf.Atan2(YSpeed, XSpeed);
        Sprite.Rotation = XDirection >= 0 ? atan2 : -atan2;
    }

    private static readonly NodePath NpSprite = "Sprite";
    private AnimatedSprite2D _sprite;
}