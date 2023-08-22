using ChloePrime.MarioForever.RPG;
using Godot;
using MarioForeverMoleEditor.scripts.util;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class GoombaHurtDetector : EnemyHurtDetector
{
    [Export]
    public AudioStream StompedSound { get; set; } = GD.Load<AudioStream>("res://resources/enemies/SE_stomp.wav");

    [Export]
    public PackedScene StompedCorpse { get; set; } = GD.Load<PackedScene>("res://resources/enemies/goomba_photo.tscn");

    public override void PlayDeathSound(DamageEvent e)
    {
        if (e.DamageTypes == DamageType.Stomp)
        {
            this.PlaySound(StompedSound);
        }
        else
        {
            base.PlayDeathSound(e);
        }
    }

    public override Node2D CreateCorpse(DamageEvent e)
    {
        return e.DamageTypes == DamageType.Stomp ? StompedCorpse.Instantiate<Node2D>() : base.CreateCorpse(e);
    }
}