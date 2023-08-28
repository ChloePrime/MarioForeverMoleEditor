using ChloePrime.MarioForever.Level;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Player;

public partial class Mario
{
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

    private int _hurtStack;
    private bool _killed;
    private bool _invulnerable;
    private float _invulnerableFlashPhase;
    private Timer _invulnerableTimer;
}