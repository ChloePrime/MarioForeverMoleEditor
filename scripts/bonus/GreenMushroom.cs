using ChloePrime.MarioForever.Player;
using Godot;

namespace ChloePrime.MarioForever.Bonus;

[GlobalClass]
public partial class GreenMushroom : PickableBonus
{
    public override void _OnMarioGotMe(Mario mario)
    {
        mario.MakeSlippery(0);
        base._OnMarioGotMe(mario);
    }
}