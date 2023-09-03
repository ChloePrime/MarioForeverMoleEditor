using Godot;

namespace ChloePrime.MarioForever.Bonus;

public partial class LeapingCoin : Node2D
{
    public override void _Ready()
    {
        GlobalData.Coins++;
    }
}