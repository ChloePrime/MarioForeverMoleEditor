using ChloePrime.Godot.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class ClampShootComponentBase : Node
{
    [Export] public int BulletCount { get; set; } = 3;
    [Export] public float ShootDelay { get; set; } = 0.2F;
    [Export] public PackedScene BulletPrefab { get; set; }
    [Export] public AudioStream ShootSound { get; set; } = GD.Load<AudioStream>("res://resources/shared/SE_fireball.wav");

    public ClampFlower Flower => _clamp ??= GetParent<ClampFlower>();
    public Node2D Muzzle { get; private set; }

    public virtual void ShootBullet()
    {
        ShootSound?.Play();
    }

    protected virtual void OnClampMovedToTop()
    {
        if (Flower.VisibleOnScreenNotifier.IsOnScreen())
        {
            _shootTimer.WaitTime = ShootDelay;
            _shootTimer.Start();
        }
    }

    public override void _Ready()
    {
        base._Ready();
        Flower.ClampMovedToTop += OnClampMovedToTop;
        Muzzle = Flower.GetNode<EnemyCore>(NpClampEnemyCore);

        AddChild(_shootTimer = new Timer { Autostart = false, OneShot = false });
        _shootTimer.Timeout += OnShootTimerTimeout;
    }

    private void OnShootTimerTimeout()
    {
        _bulletsShot++;
        if (_bulletsShot >= BulletCount)
        {
            _shootTimer.Stop();
            _bulletsShot = 0;
        }
        ShootBullet();
    }

    private static readonly NodePath NpClampEnemyCore = "Enemy Core";
    private ClampFlower _clamp;
    private Timer _shootTimer;
    private int _bulletsShot;
}