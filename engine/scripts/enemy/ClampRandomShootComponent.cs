using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class ClampRandomShootComponent : ClampShootComponentBase
{
    [Export] public float MaxBulletXSpeed { get; set; } = Units.Speed.CtfToGd(5);
    [Export] public float MinBulletYSpeed { get; set; } = Units.Speed.CtfToGd(3);
    [Export] public float MaxBulletYSpeed { get; set; } = Units.Speed.CtfToGd(11);
    
    public override void _Ready()
    {
        base._Ready();
        _rule = this.GetRule();
    }

    public override void ShootBullet()
    {
        base.ShootBullet();
        if (!Flower.VisibleOnScreenNotifier.IsOnScreen())
        {
            return;
        }
        
        var fireball = BulletPrefab.Instantiate<Node2D>();
        if (fireball is GravityObjectBase gob)
        {
            gob.XSpeed = gob.TargetSpeed = (2 * GD.Randf() - 1) * MaxBulletXSpeed;
            gob.YSpeed = -(float)GD.RandRange(MinBulletYSpeed, MaxBulletYSpeed);
        }
        if (fireball is ClampFireball cf)
        {
            cf.GameRule = _rule;
        }
        this.GetPreferredRoot().AddChild(fireball);
        fireball.GlobalPosition = Muzzle.GlobalPosition;
    }

    private GameRule _rule;
}