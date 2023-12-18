using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

public partial class Lakitu : Node2D
{
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _sprite, NpSprite);
    }

    private void OnAnimationUpdateTimerTimeout()
    {
        if (_sprite.Animation != Anim00Default)
        {
            return;
        }

        var rng = GD.Randi() % 20;
        _sprite.Animation = rng switch
        {
            12 => Anim03BlinkUp,
            13 => Anim04BlinkDown,
            _ => Anim00Default,
        };
        _sprite.Play();
    }

    private void OnSpriteAnimationFinished()
    {
        _sprite.Animation = Anim00Default;
        _sprite.Play();
    }

    private static readonly NodePath NpSprite = "Enemy Core/Sprite";
    private static readonly StringName Anim00Default = "[00] default";
    private static readonly StringName Anim01AttackIn = "[01] attack_in";
    private static readonly StringName Anim02AttackOut = "[02] attack_out";
    private static readonly StringName Anim03BlinkUp = "[03] blink_up";
    private static readonly StringName Anim04BlinkDown = "[04] blink_down";

    private AnimatedSprite2D _sprite;
}