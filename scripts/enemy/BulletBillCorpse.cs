using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Shared;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class BulletBillCorpse : SimpleNoClipGravityObject
{
    [Export] public PackedScene ExplodeEffect = GD.Load<PackedScene>("res://objects/effect/O_explosion_m_megaman.tscn");
    [Export] public AudioStreamGroup ExplodeSound;
    
    public AnimatedSprite2D Sprite => _sprite ??= GetNode<AnimatedSprite2D>(NpSprite);

    public void Explode()
    {
        var gfx = this.GetPreferredRoot().SpawnChild(ExplodeEffect);
        if (gfx is Node2D gfx2d)
        {
            gfx2d.GlobalPosition = GlobalPosition;
        }
        ExplodeSound.Play();
        QueueFree();
    }

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _explodeHitbox, NpExplodeHitbox);
        _explodeHitbox.BodyEntered += OnExplodeHitboxBodyEntered;
        _explodeHitbox.BodyExited += OnExplodeHitboxBodyExited;

        if (Sprite.FlipH)
        {
            _explodeHitbox.Rotation = Mathf.Pi;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (GlobalPosition.Y >= this.GetFrame().End.Y + Sprite.GetSpriteSize().Y)
        {
            QueueFree();
            return;
        }
        var atan2 = Mathf.Atan2(YSpeed, XSpeed);
        Rotation = XDirection >= 0 ? atan2 : -atan2;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        ++_lifeTicks;
    }

    private void OnExplodeHitboxBodyEntered(Node2D body)
    {
        if (_lifeTicks >= 1 && _hitStack <= 0)
        {
            Explode();
            return;
        }
        ++_hitStack;
    }
    
    private void OnExplodeHitboxBodyExited(Node2D body)
    {
        --_hitStack;
    }

    private static readonly NodePath NpSprite = "Sprite";
    private static readonly NodePath NpExplodeHitbox = "Explode Hitbox";
    private AnimatedSprite2D _sprite;
    private Area2D _explodeHitbox;
    private long _lifeTicks;
    private int _hitStack;
}