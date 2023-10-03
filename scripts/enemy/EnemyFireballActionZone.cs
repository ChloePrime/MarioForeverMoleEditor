using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Shared;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class EnemyFireballActionZone : FireballActionZone
{
    [Export] public float DamageLo { get; set; } = 1;
    [Export] public float DamageHi { get; set; } = 10;
    
    public override void _Ready()
    {
        base._Ready();
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is MarioHurtZone mhz)
        {
            mhz.Root.Hurt(new DamageEvent
            {
                DamageLo = DamageLo,
                DamageHi = DamageHi,
                AttackVector = Fireball.VelocityVector,
                DirectSource = Fireball,
                TrueSource = Fireball.Shooter,
            });
            Fireball.Explode();
        }
    }
}