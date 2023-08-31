using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Shared;
using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Bonus;

public partial class BumpableBlock : StaticBody2D, IBumpable
{
    [Export]
    public new bool Hidden
    {
        get => _hidden;
        set
        {
            _hidden = value;
            SetCollisionLayerValue(MaFo.CollisionLayers.HiddenBonus, value);
            SetCollisionLayerValue(MaFo.CollisionLayers.Solid, !value);
            if (!Bumped)
            {
                Visible = !value;
            }
        }
    }
    
    protected bool Bumped { get; set; }

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _bumpAnimation, NpBumpAnimation);
        this.GetNode(out _shape, NpCollisionShape);
    }

    public virtual void OnBumpBy(Node2D bumper)
    {
        _bumpAnimation.Play(AnimBumped);
        if (Hidden)
        {
            Hidden = false;
        }
        Bumped = true;
        KillMobsAbove(bumper);
    }

    private void KillMobsAbove(Node2D bumper)
    {
        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = _shape.Shape,
            Transform = _shape.GlobalTransform.TranslatedLocal(BumpTestOffset),
            CollisionMask = MaFo.CollisionMask.Enemy,
            CollideWithAreas = true,
            CollideWithBodies = false,
        };
        foreach (var result in GetWorld2D().DirectSpaceState.IntersectShapeTyped(query))
        {
            if (result.Collider is EnemyHurtDetector ehd)
            {
                ehd.HurtBy(new DamageEvent
                {
                    DamageTypes = DamageType.Bump,
                    TrueSource = bumper,
                    DirectSource = this,
                });
            }
        }
    }

    private static readonly NodePath NpBumpAnimation = "Bump Animation";
    private static readonly NodePath NpCollisionShape = "Collision Shape";
    private static readonly StringName AnimBumped = "bumped";
    private static readonly Vector2 BumpTestOffset = new(0, -4); 
    private AnimationPlayer _bumpAnimation;
    private CollisionShape2D _shape;
    private bool _hidden;
}