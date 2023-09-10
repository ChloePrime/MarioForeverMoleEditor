﻿using ChloePrime.MarioForever.Enemy;
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
        ehd.HurtBy(new DamageEvent
        {
            DamageTypes = DamageType.Fireball,
            DamageToEnemy = this.GetRule().FireballPower,
            DirectSource = Fireball,
            TrueSource = Fireball.Shooter
        });
        Fireball.Explode();
    }
}