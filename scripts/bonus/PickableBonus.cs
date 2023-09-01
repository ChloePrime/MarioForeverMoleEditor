using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Player;
using Godot;
using MarioForeverMoleEditor.scripts.util;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Bonus;

public partial class PickableBonus : WalkableObjectBase
{
    [Export]
    public PackedScene Score { get; set; } = GD.Load<PackedScene>("res://objects/ui/O_score_1000.tscn");

    [Export]
    public AudioStream PickSound { get; set; } = GD.Load<AudioStream>("res://resources/mario/SE_powerup.wav");

    public virtual void OnMarioGotMe(Mario mario)
    {
        CreateScore();
        PickSound?.Play();
        QueueFree();
    }

    public override void _ReallyReady()
    {
        base._ReallyReady();
        if (TargetSpeed > 0)
        {
            TryFaceTowardsMario();
        }
    }

    private void CreateScore()
    {
        if (!this.TryGetParent(out Node parent)) return;
        Score.Instantiate(out Node2D score);
        parent.AddChild(score);
        score.GlobalPosition = GlobalPosition;
    }
}