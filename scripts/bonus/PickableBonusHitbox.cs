using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Bonus;

public partial class PickableBonusHitbox : Area2D
{
    public override void _Ready()
    {
        base._Ready();
        this.FindParentOfType(out _root);
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D other)
    {
        // if (!_root.ReallyEnabled) return;
        if (other is not Mario mario) return;
        _root._OnMarioGotMe(mario);
    }

    private PickableBonus _root;
}