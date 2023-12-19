using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

public partial class Lakitu : Node2D
{
    [Export] public PackedScene Projectile { get; set; }
    [Export] public float MinAttackDelay { get; set; } = 4;
    [Export] public float MaxAttackDelay { get; set; } = 6;
    [Export] public float AttackCastTime { get; set; } = 0.8F;
    [Export] public float AttackRecoverTime { get; set; } = 0.2F;
    [Export] public float ThrowPower { get; set; } = Units.Speed.CtfToGd(3);
    [Export] public AudioStreamGroup AttackSound { get; set; }

    public const float StandardAttackCastTime = 0.8F;
    public const float StandardAttackRecoverTime = 0.2F;
    
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _sprite, NpSprite);
        this.GetNode(out _muzzle, NpMuzzle);
        this.GetNode(out _attackTimer, NpAttackTimer);
        this.GetNode(out _vosn, NpVosn);
        ResetAttackTimer();
    }

    public async void ThrowSpiny()
    {
        if (_sprite.Animation != Anim00Default &&
            _sprite.Animation != Anim03BlinkUp &&
            _sprite.Animation != Anim04BlinkDown)
        {
            return;
        }
        
        _sprite.Play(Anim01AttackIn, AttackCastTime / StandardAttackCastTime);
        await this.DelayAsync(AttackCastTime);
        if (_sprite.Animation != Anim01AttackIn)
        {
            return;
        }
        
        _sprite.Play(Anim02AttackOut, AttackRecoverTime / StandardAttackRecoverTime);
        await this.DelayAsync(AttackRecoverTime, true, true);
        if (_sprite.Animation != Anim02AttackOut)
        {
            return;
        }
        
        ThrowSpinyInstantly();
    }

    public void ThrowSpinyInstantly()
    {
        AttackSound.Play();
        _sprite.Play(Anim00Default);

        if (Projectile is not { } prefab)
        {
            return;
        }

        var projectile = prefab.Instantiate();
        if (projectile is GravityObjectBase gob)
        {
            gob.YSpeed = -ThrowPower;
        }
        this.GetPreferredRoot().AddChild(projectile);
        if (projectile is Node2D p2d)
        {
            p2d.GlobalPosition = _muzzle.GlobalPosition;
        }
    }

    private async void OnAttackTimerTimeout()
    {
        if (_vosn.IsOnScreen())
        {
            ThrowSpiny();
        }
        await this.DelayAsync(AttackCastTime + AttackRecoverTime);
        ResetAttackTimer();
    }

    private void ResetAttackTimer()
    {
        _attackTimer.WaitTime = Mathf.Lerp(MinAttackDelay, MaxAttackDelay, GD.Randf());
        _attackTimer.Start();
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
        if (_sprite.Animation != Anim00Default && 
            _sprite.Animation != Anim01AttackIn &&
            _sprite.Animation != Anim02AttackOut)
        {
            _sprite.Animation = Anim00Default;
            _sprite.Play();
        }
    }

    private static readonly NodePath NpSprite = "Enemy Core/Sprite";
    private static readonly NodePath NpMuzzle = "Muzzle";
    private static readonly NodePath NpAttackTimer = "Attack Timer";
    private static readonly NodePath NpVosn = "VisibleOnScreenNotifier";
    private static readonly StringName Anim00Default = "[00] default";
    private static readonly StringName Anim01AttackIn = "[01] attack_in";
    private static readonly StringName Anim02AttackOut = "[02] attack_out";
    private static readonly StringName Anim03BlinkUp = "[03] blink_up";
    private static readonly StringName Anim04BlinkDown = "[04] blink_down";

    private AnimatedSprite2D _sprite;
    private Node2D _muzzle;
    private VisibleOnScreenNotifier2D _vosn;
    private Timer _attackTimer;
}