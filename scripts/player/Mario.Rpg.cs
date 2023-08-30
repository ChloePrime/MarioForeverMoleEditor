using System;
using ChloePrime.MarioForever.Level;
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

    public void Hurt()
    {
        if (IsInvulnerable() || _currentStatus == null)
        {
            return;
        }
        if (_currentStatus.HurtResult == null)
        {
            Kill();
            return;
        }
        GlobalData.Status = _currentStatus.HurtResult;
        SetInvulnerable(InvulnerableTimeOnHurt);
        _hurtSound.Play();
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
        _invulnerableTimer.Start();
    }
    
    public void Kill()
    {
        if (_killed)
        {
            return;
        }
        _killed = true;
        
        var corpse = Constants.CorpsePrefab.Instantiate<MarioCorpse>();
        GetParent()?.AddChild(corpse);
        corpse.GlobalPosition = GlobalPosition;
        corpse.SetFastRetry(FastRetry);
        PostDeath();
        QueueFree();
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
        
        _hurtZone.BodyEntered += _ => _hurtStack++;
        _hurtZone.BodyExited += _ => _hurtStack--;
        _deathZone.BodyEntered += _ => Kill();
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
    private Timer _invulnerableTimer;
}