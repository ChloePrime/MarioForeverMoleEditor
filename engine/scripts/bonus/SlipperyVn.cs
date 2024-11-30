using ChloePrime.MarioForever.Player;
using Godot;

namespace ChloePrime.MarioForever.Bonus;

[GlobalClass]
[Icon("res://engine/resources/bonus/vn_icon.tres")]
public partial class SlipperyVn : PickableBonus
{
    [Export] public float Power { get; set; } = 7F;
    
    public override void _OnMarioGotMe(Mario mario)
    {
        mario.MakeSlippery(Power);
        base._OnMarioGotMe(mario);
    }
}