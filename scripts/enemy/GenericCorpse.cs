using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

/// <summary>
/// 普通的尸体
/// </summary>
public partial class GenericCorpse : AnimatedSprite2D, ICorpse
{
    [Export] public float YSpeed { get; set; }
    [Export] public float Gravity { get; set; } = Units.Acceleration.CtfToGd(0.2F);

    public override void _Process(double delta)
    {
        base._Process(delta);
        Translate(new Vector2(0, YSpeed * (float)delta));

        if (GlobalPosition.Y >= this.GetFrame().End.Y + this.GetSpriteSize().Y)
        {
            QueueFree();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        YSpeed += Gravity * (float)delta;
    }
}