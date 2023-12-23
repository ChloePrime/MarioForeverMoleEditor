using System.Collections.Generic;
using ChloePrime.MarioForever.Level;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Util;
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
        if (PipeState != MarioPipeState.NotInPipe) return;
        if ((!e.BypassInvulnerable && IsInvulnerable()) || _currentStatus == null)
        {
            return;
        }
        var rule = GameRule;
        float invulnerableTime;
        var useHp = false; 
        if (!rule.HitPointEnabled || (rule.HitPoint <= 0 && !rule.KillPlayerWhenHitPointReachesZero))
        {
            if (_currentStatus.HurtResult == null)
            {
                Kill(e, true);
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
                useHp = true;
            }
            if (rule.HitPoint <= 0 && rule.KillPlayerWhenHitPointReachesZero)
            {
                Kill(e, true);
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

        if (useHp && GetHurtVoice(e) is { } voice)
        {
            voice.Play();
        }
        else
        {
            _hurtSound.Play();
        }
    }

    private void DropStatus()
    {
        GlobalData.Status = _currentStatus.HurtResult ?? MarioStatus.Small;
    }

    public bool IsInvulnerable()
    {
        return _invulnerable || _completedLevel;
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
        Kill(e, false);
    }
    
    private void Kill(DamageEvent e, bool killedByHurt)
    {
        if (!killedByHurt)
        {
            if (GameRule.HitPointEnabled &&
                GameRule.HitPointProtectsDeath &&
                GameRule.HitPoint >= GameRule.HitPointProtectsDeathCost)
            {
                var event2 = e with
                {
                    DamageLo = GameRule.HitPointProtectsDeathCostLo,
                    DamageHi = GameRule.HitPointProtectsDeathCostHi,
                    EventFlags = e.EventFlags | DamageEvent.Flags.DeathProtection,
                };
                Hurt(event2);
                if (e.DirectSource == _slipperyGas && _posQueue.TryPeek(out var safePos))
                {
                    GlobalPosition = safePos;
                }
                return;
            }
            if (GameRule.HitPointEnabled &&
                GameRule.HitPointProtectsDeath &&
                !e.BypassInvulnerable &&
                IsInvulnerable())
            {
                return;
            }
        }

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

    private void ProcessPositionAutoSave()
    {
        if (_isInAir || _crouching)
        {
            return;
        }
        var collision = new KinematicCollision2D();
        bool safe;
        if (TestMove(GlobalTransform, GroundTestVec, collision))
        {
            safe = collision.GetCollider() is TileMap or StaticBody2D;
        }
        else
        {
            safe = false;
        }
        if (!safe)
        {
            return;
        }
        
        while (_posQueue.Count >= PosSaveRecordCount)
        {
            _posQueue.Dequeue();
        }
        _posQueue.Enqueue(GlobalPosition);
    }

    private readonly Queue<Vector2> _posQueue = new(32);
    private const int PosSaveRecordCount = 30;
    private static readonly NodePath NpCorpseDeathSound = "The Funny Sound";

    public void MakeSlippery(float slipperiness)
    {
        Slipperiness = slipperiness;
        _slipperyGas.Visible = slipperiness is not 0;
    }

    private void PostDeath()
    {
        if (GetViewport().GetCamera2D() is { } camera && camera.GetParent() == this)
        {
            camera.Reparent(this.GetLevel() ?? this.GetPreferredRoot());
        }
        GameRule.ReloadStatus();
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
        _deathZone.BodyEntered += _ => _deathStack++;
        _deathZone.BodyExited += _ => _deathStack--;
        _invulnerableTimer.Timeout += () => _invulnerable = false;
    }

    private void ProcessFlashing(float delta)
    {
        // 无敌
        if (_invulnerable)
        {
            _invulnerableFlashPhase = (_invulnerableFlashPhase + InvulnerabilityFlashSpeed * delta) % 1;
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
    private int _deathStack;
    private bool _killed;
    
    /// <summary>
    /// 该无敌变量不考虑通关过程
    /// </summary>
    /// <see cref="IsInvulnerable"/>
    private bool _invulnerable;
    
    private float _flashTime;
    private float _flashDuration;
    private float _invulnerableFlashPhase;
    private MarioCollisionBySize _hurtZone;
    private MarioCollisionBySize _deathZone;
    private Timer _invulnerableTimer;
    private AnimatedSprite2D _slipperyGas;
}