using ChloePrime.MarioForever.Player;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Bonus;

public partial class PowerUpBonus : PickableBonus
{
    [Export] public float HitPointNutritionLo { get; set; } = 3;
    [Export] public float HitPointNutritionHi { get; set; } = 100;
    
    [Export] public MarioStatus TargetStatus { get; private set; }

    public override void OnMarioGotMe(Mario mario)
    {
        base.OnMarioGotMe(mario);
        if (TargetStatus is not { } target)
        {
            this.LogWarn("Invalid PowerUp: TargetStatus is not set");
            return;
        }
        bool addHp = mario.GameRule.HitPointProtectsYourPowerup;
        if (target == MarioStatus.Big)
        {
            if (GlobalData.Status == MarioStatus.Small)
            {
                GlobalData.Status = MarioStatus.Big;
                mario.OnPowerUp();
            }
            else
            {
                addHp = true;
            }
        }
        else
        {
            if (mario.GameRule.HitPointEnabled &&
                (GlobalData.Status != MarioStatus.Small && GlobalData.Status != MarioStatus.Big))
            {
                addHp = true;
            }
            if (GlobalData.Status != target)
            {
                GlobalData.Status = target;
                mario.OnPowerUp();
            }
        }
        if (addHp && mario.GameRule.HitPointEnabled)
        {
            mario.GameRule.AlterHitPoint(HitPointNutritionLo, HitPointNutritionHi);
        }
    }
}