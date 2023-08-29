using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Player;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Bonus;

public partial class PowerUpBonus : PickableBonus
{
    [Export] public MarioStatus TargetStatus { get; private set; }

    public override void OnMarioGotMe(Mario mario)
    {
        base.OnMarioGotMe(mario);
        if (TargetStatus is not { } target)
        {
            this.LogWarn("Invalid PowerUp: TargetStatus is not set");
            return;
        }
        if (target == MarioStatus.Big)
        {
            if (GlobalData.Status == MarioStatus.Small)
            {
                GlobalData.Status = MarioStatus.Big;
                mario.OnPowerUp();
            }
        }
        else
        {
            if (GlobalData.Status != target)
            {
                GlobalData.Status = target;
                mario.OnPowerUp();
            }
        }
    }
}