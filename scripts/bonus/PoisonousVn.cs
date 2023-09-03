using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using Godot;

namespace ChloePrime.MarioForever.Bonus;

[GlobalClass]
[Icon("res://resources/bonus/AT_vn_icon.tres")]
public partial class PoisonousVn: PickableBonus
{
    public override void OnMarioGotMe(Mario mario)
    {
        mario.Kill(new DamageEvent
        {
            DamageTypes = DamageType.Poison,
            DirectSource = this,
            TrueSource = this,
        });
        base.OnMarioGotMe(mario);
    }
}