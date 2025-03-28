using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Player;
using Godot;

namespace ChloePrime.MarioForever.Bonus;

[GlobalClass]
public partial class PickableBonus : WalkableObjectBase
{
    [Export]
    public PackedScene Score { get; set; } = GD.Load<PackedScene>("res://engine/objects/ui/O_score_1000.tscn");

    [Export]
    public AudioStream PickSound { get; set; } = GD.Load<AudioStream>("res://engine/resources/mario/SE_powerup.wav");

    public virtual void _OnMarioGotMe(Mario mario)
    {
        CreateScore();
        PickSound?.Play();
        QueueFree();
    }

    public override void _ReallyReady()
    {
        base._ReallyReady();
        if (TargetSpeed > 0 && !IsVelocityExplicitSet)
        {
            TryFaceTowardsMario();
        }
    }

    private void CreateScore()
    {
        if (!this.TryGetParent(out Node parent) || Score is not {} prefab) return;
        prefab.Instantiate(out Node2D score);
        parent.AddChild(score);
        score.GlobalPosition = GlobalPosition;
    }
}