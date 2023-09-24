using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class TurtleHurtDetector : EnemyHurtDetector
{
    [Export] public AudioStream KickSound { get; set; } = GD.Load<AudioStream>("res://resources/shared/SE_kick.wav");
    
    public override void _Ready()
    {
        base._Ready();
        _core = (TurtleEnemyCore)Core;
    }

    public override void StompBy(Node2D stomper)
    {
        // 让静止的龟壳可以被抱起来
        if (_core.Turtle.State is Turtle.TurtleState.StaticShell && Input.IsActionPressed(Mario.Constants.ActionRun))
        {
            return;
        }
        base.StompBy(stomper);
    }

    public override bool Kill(DamageEvent e)
    {
        if (!e.DamageTypes.HasFlag(DamageType.Stomp))
        {
            return base.Kill(e);
        }
        
        StompedSound.Play();

        _core.NpcData.HitPoint = _core.NpcData.MaxHitPoint;
        _core.Turtle.State = _core.Turtle.State switch
        {
            Turtle.TurtleState.Flying or Turtle.TurtleState.Jumping => Turtle.TurtleState.Walking,
            Turtle.TurtleState.Walking => Turtle.TurtleState.StaticShell,
            Turtle.TurtleState.StaticShell => Turtle.TurtleState.MovingShell,
            Turtle.TurtleState.MovingShell => Turtle.TurtleState.StaticShell,
            _ => Turtle.TurtleState.StaticShell,
        };
        
        if (_core.Turtle.State is Turtle.TurtleState.MovingShell)
        {
            if ((e.DirectSource ?? e.TrueSource) is { } kicker)
            {
                _core.Turtle.KickBy(kicker);
            }
        }
        
        return false;
    }

    private TurtleEnemyCore _core;
}