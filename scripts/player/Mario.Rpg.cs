using System;
using ChloePrime.MarioForever.Level;
using ChloePrime.MarioForever.RPG;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Player;

public partial class Mario
{
    public void OnPowerUp()
    {
        Flash(DefaultRainbowFlashTime);
    }

    public void Flash(float duration)
    {
        _flashTime = _flashDuration = duration;
    }

    public void Hurt(DamageEvent e)
    {
        if (IsInvulnerable() || _currentStatus == null)
        {
            return;
        }
        var rule = GameRule;
        float invulnerableTime;
        if (!rule.HitPointEnabled || (rule.HitPoint <= 0 && !rule.KillPlayerWhenHitPointReachesZero))
        {
            if (_currentStatus.HurtResult == null)
            {
                Kill(e);
                return;
            }
            DropStatus();
            invulnerableTime = InvulnerableTimeOnHurt;
        }
        else
        {
            if (rule.HitPointProtectsYourPowerup || _currentStatus == MarioStatus.Small)
            {
                rule.AlterHitPoint(-e.DamageLo, -e.DamageHi);
            }
            if (rule.HitPoint <= 0 && rule.KillPlayerWhenHitPointReachesZero)
            {
                Kill(e);
                return;
            }
            if (!rule.HitPointProtectsYourPowerup)
            {
                DropStatus();
            }
            invulnerableTime = rule.HitPointMagnitude switch
            {
                GameRule.HitPointMagnitudeType.High => e.DamageHi <= SmallHpThreshold + 1e-4
                    ? InvulnerableTimeOnSmallHpHurt
                    : InvulnerableTimeOnHurt,
                GameRule.HitPointMagnitudeType.Low or _ => InvulnerableTimeOnHurt,
            };
        }
        SetInvulnerable(invulnerableTime);
        _hurtSound.Play();
    }

    private void DropStatus()
    {
        GlobalData.Status = _currentStatus.HurtResult ?? MarioStatus.Small;
    }

    public bool IsInvulnerable()
    {
        return _invulnerable;
    }

    public void SetInvulnerable(double time)
    {
        if (time < _invulnerableTimer.TimeLeft)
        {
            return;
        }
        _invulnerable = true;
        _invulnerableFlashPhase = 0;
        _invulnerableTimer.WaitTime = time;
        _invulnerableTimer.Start();
    }
    
    public void Kill(DamageEvent e)
    {
        if (_killed)
        {
            return;
        }
        _killed = true;
        
        var corpse = Constants.CorpsePrefab.Instantiate<MarioCorpse>();
        GetParent()?.AddChild(corpse);
        corpse.GlobalPosition = GlobalPosition;
        if (_slipperyGas.Visible &&
            e.DirectSource == _slipperyGas &&
            corpse.TryGetNode(out AudioStreamPlayer funnySound, NpCorpseDeathSound))
        {
            funnySound.Stream = GD.Load<AudioStream>("res://resources/mario/ME_slippery_man.ogg");
            funnySound.Play();
            FastRetry = false;
        }
        corpse.SetFastRetry(FastRetry);
        PostDeath();
        QueueFree();
    }

    private static readonly NodePath NpCorpseDeathSound = "The Funny Sound";

    public void MakeSlippery(bool enabled, float maxSpeedScale, float accScale, bool accModifyTurnOnly = true)
    {
        _speedBackups ??= new float?[6];
        if (enabled)
        {
            _speedBackups[0] ??= MaxSpeedWhenWalking;
            _speedBackups[1] ??= MaxSpeedWhenRunning;
            _speedBackups[2] ??= MaxSpeedWhenSprinting;
            _speedBackups[3] ??= AccelerationWhenWalking;
            _speedBackups[4] ??= AccelerationWhenRunning;
            _speedBackups[5] ??= AccelerationWhenTurning;
            MaxSpeedWhenWalking *= maxSpeedScale;
            MaxSpeedWhenRunning *= maxSpeedScale;
            MaxSpeedWhenSprinting *= maxSpeedScale;
            if (!accModifyTurnOnly)
            {
                AccelerationWhenWalking *= accScale;
                AccelerationWhenRunning *= accScale;
            }
            AccelerationWhenTurning *= accScale;
        }
        else
        {
            MaxSpeedWhenWalking = _speedBackups[0] ?? MaxSpeedWhenWalking;
            MaxSpeedWhenRunning = _speedBackups[1] ?? MaxSpeedWhenRunning;
            MaxSpeedWhenSprinting = _speedBackups[2] ?? MaxSpeedWhenSprinting;
            AccelerationWhenWalking = _speedBackups[3] ?? AccelerationWhenWalking;
            AccelerationWhenRunning = _speedBackups[4] ?? AccelerationWhenRunning;
            AccelerationWhenTurning = _speedBackups[5] ?? AccelerationWhenTurning;
            Array.Fill(_speedBackups, null);
        }
        _slipperyGas.Visible = enabled;
    }

    private void PostDeath()
    {
        GlobalData.Status = MarioStatus.Small;
        if (!FastRetry)
        {
            BackgroundMusic.Stop();
        }
    }

    private void RpgReady()
    {
        this.GetNode(out _hurtZone, Constants.NpHurtZone);
        this.GetNode(out _deathZone, Constants.NpDeathZone);
        this.GetNode(out _invulnerableTimer, Constants.NpInvTimer);
        this.GetNode(out _slipperyGas, Constants.NpSlipperyGas);
        
        _hurtZone.BodyEntered += _ => _hurtStack++;
        _hurtZone.BodyExited += _ => _hurtStack--;
        _deathZone.BodyEntered += body => Kill(new DamageEvent
        {
            DamageTypes = DamageType.Environment,
            DirectSource = body,
            TrueSource = body,
        });
        _invulnerableTimer.Timeout += () => _invulnerable = false;
    }

    private void ProcessFlashing(float delta)
    {
        // 无敌
        if (_invulnerable)
        {
            _invulnerableFlashPhase = (_invulnerableFlashPhase + InvulnerabilityFlashSpeed * (float)delta) % 1;
            var alpha = Mathf.Cos(2 * Mathf.Pi * _invulnerableFlashPhase);
            _spriteRoot.Modulate = new Color(Colors.White, alpha);
        }
        else if (_invulnerableFlashPhase != 0)
        {
            _invulnerableFlashPhase = 0;
            _spriteRoot.Modulate = Colors.White;
        }
        // 强化状态 / 彩虹
        if (_spriteRoot.Material is ShaderMaterial sm)
        {
            float alpha;
            const float blendTime = 0.2F;
            switch (_flashTime)
            {
                case <= 0:
                    return;
                case <= blendTime:
                    alpha = _flashTime / blendTime;
                    break;
                default:
                    alpha = 1;
                    break;
            }

            _flashTime -= delta;
            if (_flashTime <= 0)
            {
                alpha = 0;
            }
            
            sm.SetShaderParameter(Constants.ShaderParamAlpha, alpha);
        }
    }

    private int _hurtStack;
    private bool _killed;
    private bool _invulnerable;
    private float _flashTime;
    private float _flashDuration;
    private float _invulnerableFlashPhase;
    private float?[] _speedBackups;
    private MarioCollisionBySize _hurtZone;
    private MarioCollisionBySize _deathZone;
    private Timer _invulnerableTimer;
    private AnimatedSprite2D _slipperyGas;
}