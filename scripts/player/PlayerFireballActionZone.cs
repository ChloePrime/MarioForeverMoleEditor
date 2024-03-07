using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Shared;
using Godot;

namespace ChloePrime.MarioForever.Player;

public partial class PlayerFireballActionZone : FireballActionZone
{
    public override void _Ready()
    {
        base._Ready();
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D other)
    {
        if (other is not EnemyHurtDetector ehd)
        {
            return;
        }
        var de = new DamageEvent
        {
            DamageTypes = DamageType.Fireball,
            DamageToEnemy = this.GetRule().FireballPower,
            AttackVector = Fireball.VelocityVector,
            DirectSource = Fireball,
            TrueSource = Fireball.Shooter,
        };
        if (ehd.IgnoreUnsupportedDamageTypes && !ehd.CanBeHurtBy(de))
        {
            return;
        }
        ehd.HurtBy(de);
        Fireball.Explode();
    }
}