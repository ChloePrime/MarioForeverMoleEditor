using System.Linq;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;
using Array = Godot.Collections.Array;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
[Icon("res://engine/resources/enemies/AT_bullet_launcher_up.tres")]
public partial class BulletLauncher : BulletLauncherBase
{
    [Export] public double FirstShotDelay { get; set; } = 0.5F;
    [Export] public double MinDelay { get; set; } = 1.5F;
    [Export] public double MaxDelay { get; set; } = 3;
    [Export] public float ShyDistance { get; set; } = 80;
    [Export] public PackedScene BulletPrefab { get; set; } = GD.Load<PackedScene>("res://engine/objects/enemies/O_bullet_bill.tscn");
    [Export] public PackedScene MuzzleFlash { get; set; } = GD.Load<PackedScene>("res://engine/objects/effect/O_explosion_s.tscn");
    [Export] public AudioStreamGroup ShootSound { get; set; }

    public virtual void ShootBullet()
    {
        // 炮弹
        var root = this.GetPreferredRoot()!;
        var bullet = BulletPrefab.Instantiate();
        root.AddChildAt(bullet, GlobalPosition);
        // 音效
        ShootSound?.Play();
        // 烟花
        if (MuzzleFlash?.Instantiate() is { } flash)
        {
            if (flash is Node2D flash2D)
            {
                var player = GetTree().GetPlayer();
                var xDir = player is Node2D mario ? Mathf.Sign(ToLocal(mario.GlobalPosition).X) : 0;
                flash2D.ZIndex = ZIndex;
                root.AddChildAt(flash, GlobalPosition + new Vector2(16 * xDir, 0));
            }
            else
            {
                root.AddChild(flash);
            }
        }
    }

    public bool TryShootBullet()
    {
        if (!_vose.IsOnScreen()) return false;
        
        var players = GetTree().GetAllPlayers();
        using var _ = (Array)players;
        
        var shy = players
            .OfType<Node2D>()
            .Select(mario => ToLocal(mario.GlobalPosition))
            .Any(rp => Mathf.Abs(rp.X) < ShyDistance);

        if (shy) return false;
        
        ShootBullet();
        return true;
    }

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _timer, NpTimer);
        this.GetNode(out _vose, NpVose);
        _timer.Timeout += OnTimerTimeout;
        _timer.WaitTime = FirstShotDelay;
        _timer.Start();
        if (!this.GetRule().BulletLauncherBreakable)
        {
            this.GetNode(out EnemyHurtDetector ehd, NpEhd);
            ehd.Monitorable = ehd.Monitoring = false;
            ehd.ProcessMode = ProcessModeEnum.Disabled;
        }
    }

    private void OnTimerTimeout()
    {
        TryShootBullet();
        ScheduleNextShot();
    }

    private void ScheduleNextShot()
    {
        _timer.WaitTime = GD.RandRange(MinDelay, MaxDelay);
        _timer.Start();
    }

    private static readonly NodePath NpTimer = "Timer";
    private static readonly NodePath NpVose = "VisibleOnScreenEnabler";
    private static readonly NodePath NpEhd = "Enemy Core/Hurt Detector";
    private Timer _timer;
    private VisibleOnScreenEnabler2D _vose;
}