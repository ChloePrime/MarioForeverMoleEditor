using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Player;
using Godot;

namespace ChloePrime.MarioForever.Bonus;

[GlobalClass]
public partial class PowerUpBonus : PickableBonus
{
    [Export] public float HitPointNutritionLo { get; set; } = 3;
    [Export] public float HitPointNutritionHi { get; set; } = 100;
    
    [Export] public MarioStatus TargetStatus { get; private set; }

    public override void _OnMarioGotMe(Mario mario)
    {
        base._OnMarioGotMe(mario);
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
            else
            {
                AddHp(mario);
            }
        }
        else
        {
            if (mario.GameRule.HitPointEnabled &&
                (GlobalData.Status != MarioStatus.Small && GlobalData.Status != MarioStatus.Big))
            {
                AddHp(mario);
            }
            if (GlobalData.Status != target)
            {
                GlobalData.Status = target;
                mario.OnPowerUp();
            }
        }
    }

    private void AddHp(Mario mario)
    {
        if(mario.GameRule.HitPointEnabled)
        {
            mario.GameRule.AlterHitPoint(HitPointNutritionLo, HitPointNutritionHi);
        }
    }
}