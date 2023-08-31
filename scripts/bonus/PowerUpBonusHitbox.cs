using ChloePrime.MarioForever.Player;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Bonus;

public partial class PowerUpBonusHitbox : Area2D
{
    public override void _Ready()
    {
        base._Ready();
        this.GetParent(out _root);
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D other)
    {
        // if (!_root.ReallyEnabled) return;
        if (other is not Mario mario) return;
        _root.OnMarioGotMe(mario);
    }

    private PickableBonus _root;
}