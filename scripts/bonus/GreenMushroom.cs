using ChloePrime.MarioForever.Player;

namespace ChloePrime.MarioForever.Bonus;

public partial class GreenMushroom : PickableBonus
{
    public override void _OnMarioGotMe(Mario mario)
    {
        mario.MakeSlippery(0);
        base._OnMarioGotMe(mario);
    }
}