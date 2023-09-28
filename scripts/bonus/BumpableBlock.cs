using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Shared;
using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Bonus;

[GlobalClass]
public partial class BumpableBlock : StaticBody2D, IBumpable
{
    /// <summary>
    /// 设置该问号块是否为隐藏块。
    /// 在第一次被顶过以后设置就无效了
    /// </summary>
    [Export]
    public new bool Hidden
    {
        get => _hidden;
        set => SetHidden(value);
    }
    
    [Export] public bool OneTimeUse { get; set; }
    [Export] public AudioStream BumpSound { get; set; } = GD.Load<AudioStream>("res://resources/shared/SE_bump.wav");
    
    protected bool Bumped { get; set; }

    protected virtual void _Disable()
    {
        _bumpAnimation.ProcessMode = ProcessModeEnum.Disabled;
    }

    protected virtual void _OnBumpedBy(Node2D bumper)
    {
        BumpSound?.Play();
        KillMobsAbove(bumper);
    }

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _bumpAnimation, NpBumpAnimation);
        this.GetNode(out _shape, NpCollisionShape);

        _bumpAnimation.AnimationFinished += _ => FinishBumping();
    }

    public void OnBumpBy(Node2D bumper)
    {
        if (_bumping || (OneTimeUse && Bumped))
        {
            return;
        }
        
        _bumpAnimation.Play(AnimBumped);
        _OnBumpedBy(bumper);
        
        if (Hidden)
        {
            Hidden = false;
        }
        Bumped = _bumping = true;
    }

    private void SetHidden(bool value)
    {
        _hidden = value;
        if (Bumped)
        {
            return;
        }
        SetCollisionLayerValue(MaFo.CollisionLayers.HiddenBonus, value);
        SetCollisionLayerValue(MaFo.CollisionLayers.Solid, !value);
        Visible = !value;
    }

    private void FinishBumping()
    {
        _bumping = false;
        if (OneTimeUse)
        {
            _Disable();
        }
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
            if (result.Collider is not EnemyHurtDetector ehd) continue;
            if (ehd.Core.Root == bumper || ehd == bumper) continue;
            if (ehd.Core.Root is IGrabbable { IsGrabbed: true }) continue;
            if (ehd.Core.Root is not CharacterBody2D body || body.IsOnFloor())
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

    protected CollisionShape2D Shape => _shape;
    private static readonly NodePath NpBumpAnimation = "Bump Animation";
    private static readonly NodePath NpCollisionShape = "Collision Shape";
    private static readonly StringName AnimBumped = "bumped";
    private static readonly Vector2 BumpTestOffset = new(0, -4); 
    private AnimationPlayer _bumpAnimation;
    private CollisionShape2D _shape;
    private bool _bumping;
    private bool _hidden;
}