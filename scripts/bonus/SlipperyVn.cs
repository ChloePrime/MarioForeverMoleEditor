﻿using ChloePrime.MarioForever.Player;
using Godot;

namespace ChloePrime.MarioForever.Bonus;

[GlobalClass]
[Icon("res://resources/bonus/AT_vn_icon.tres")]
public partial class SlipperyVn : PickableBonus
{
    [Export] public float MaxSpeedScale { get; set; } = 1F;
    [Export] public float AccelerationScale { get; set; } = 1F / 4;
    
    public override void _OnMarioGotMe(Mario mario)
    {
        mario.MakeSlippery(false);
        mario.MakeSlippery(true, MaxSpeedScale, AccelerationScale);
        base._OnMarioGotMe(mario);
    }
}