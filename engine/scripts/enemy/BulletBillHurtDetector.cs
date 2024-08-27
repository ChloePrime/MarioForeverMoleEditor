using ChloePrime.MarioForever.RPG;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class BulletBillHurtDetector : EnemyHurtDetector
{
    [Export]
    public PackedScene StompedCorpse { get; set; } = GD.Load<PackedScene>("res://engine/resources/enemies/bullet_bill_corpse.tscn");

    public override Node2D CreateCorpse(DamageEvent e)
    {
        return e.DamageTypes == DamageType.Stomp ? StompedCorpse.Instantiate<Node2D>() : base.CreateCorpse(e);
    }

    public override void CustomizeCorpse(DamageEvent e, Node2D corpse)
    {
        base.CustomizeCorpse(e, corpse);
        if (corpse is not BulletBillCorpse bbc) return;
        var bullet = ((BulletBill)Core.Root);
        bbc.XSpeed       = bullet.Speed;
        bbc.XDirection   = bullet.XDirection;
        bbc.Sprite.FlipH = bullet.XDirection < 0;
    }
}