using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Facility;

public partial class Spring : Area2D, IStompable
{
    [Export] public float JumpStrengthLo { get; set; } = Units.Speed.CtfToGd(10);
    [Export] public float JumpStrengthHi { get; set; } = Units.Speed.CtfToGd(19);
    [Export] public AudioStream JumpSound { get; set; } = GD.Load<AudioStream>("res://resources/facility/SE_spring.wav");
    
    public Vector2 StompCenter => GlobalPosition;
    public void StompBy(Node2D stomper)
    {
        switch (stomper)
        {
            case Mario mario:
                mario.Jump(Input.IsActionPressed(Mario.Constants.ActionJump) ? JumpStrengthHi : JumpStrengthLo);
                break;
            case GravityObjectBase gob:
                gob.YSpeed = -JumpStrengthLo;
                break;
            default:
                return;
        }
        JumpSound?.Play();
        _sprite.Play(AnimDefault);
    }

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _sprite, NpSprite);
    }

    private static readonly NodePath NpSprite = "Sprite";
    private static readonly StringName AnimDefault = "default";
    private AnimatedSprite2D _sprite;
}