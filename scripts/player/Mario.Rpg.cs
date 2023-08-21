using ChloePrime.MarioForever.level;
using Godot;

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
        if (time < (_invulnerableTimer?.TimeLeft ?? 0))
        {
            return;
        }
        _invulnerable = true;
        _invulnerableFlashPhase = 0;
        _invulnerableTimer?.Free();
        _invulnerableTimer = GetTree().CreateTimer(time);
        _invulnerableTimer.Timeout += () => _invulnerable = false;
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

    private int _hurtStack;
    private bool _killed;
    private bool _invulnerable;
    private float _invulnerableFlashPhase;
    private SceneTreeTimer _invulnerableTimer;
}