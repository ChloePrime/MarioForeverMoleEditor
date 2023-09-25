using ChloePrime.MarioForever.Util;
using ChloePrime.MarioForever.Util.HelperNodes;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

/// <summary>
/// 普通的尸体
/// </summary>
public partial class GenericCorpse : Node2D, ICorpse
{
    [Export] public float XSpeed { get; set; }
    [Export] public float YSpeed { get; set; }
    [Export] public float XDirection { get; set; } = 1;
    [Export] public float Gravity { get; set; } = Units.Acceleration.CtfToGd(0.2F);
    public AnimatedSprite2D Sprite => _sprite ??= GetNode<AnimatedSprite2D>(NpSprite);
    public Rotator2D Rotator => _rotator ??= GetNode<Rotator2D>(NpRotator);

    public override void _Process(double delta)
    {
        base._Process(delta);
        Translate(new Vector2(XSpeed * XDirection, YSpeed) * (float)delta);

        if (GlobalPosition.Y >= this.GetFrame().End.Y + Sprite.GetSpriteSize().Y)
        {
            QueueFree();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        YSpeed += Gravity * (float)delta;
    }

    private static readonly NodePath NpSprite = "Sprite";
    private static readonly NodePath NpRotator = "Rotator";
    private AnimatedSprite2D _sprite;
    private Rotator2D _rotator;
}