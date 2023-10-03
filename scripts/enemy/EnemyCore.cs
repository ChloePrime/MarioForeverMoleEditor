using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Shared;
using ChloePrime.MarioForever.Util;
using Godot;
using Godot.Collections;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class EnemyCore : Node2D, IMarioForeverNpc
{
    /// <summary>
    /// 优先使用 <see cref="Root"/> 的 NPC 数据，随后才会用这里的
    /// </summary>
    [Export, MaybeNull] public MarioForeverNpcData MyNpcData { get; private set; }
    [Export] public bool DieWhenThrownAndHitOther { get; set; } = true;
    [Export] private Node2D RootOverride { get; set; }
    
    [Signal]
    public delegate void HurtEventHandler(float amount);

    [Signal]
    public delegate void DiedEventHandler();

    [MaybeNull] public EnemyHurtDetector HurtDetector { get; private set; }
    [MaybeNull] public EnemyDamageSource DamageSource { get; private set; }
    public AnimatedSprite2D Sprite { get; private set; }
    public AnimationPlayer Animation { get; private set; }
    public Node2D Root => _root ??= (RootOverride ?? (GetParent() as Node2D)) ?? this;
    public MarioForeverNpcData NpcData => Root == this ? MyNpcData : (Root as IMarioForeverNpc)?.NpcData ?? MyNpcData ?? MarioForeverNpcData.SafeFallback;
    public IMarioForeverNpc AsNpc => this;

    [return: MaybeNull]
    public virtual ComboTracker GetComboTracker() => null;

    public (float, float) GetDamage()
    {
        var npcData = (this as IMarioForeverNpc).NpcData;
        return (npcData.DamageLo, npcData.DamageHi);
    }

    public override void _Ready()
    {
        base._Ready();
        _ = Root;
        Sprite = GetNode<AnimatedSprite2D>(NpSprite);
        Animation = GetNode<AnimationPlayer>(NpAnimation);
        HurtDetector = GetNodeOrNull<EnemyHurtDetector>("Hurt Detector");
        DamageSource = GetNodeOrNull<EnemyDamageSource>("Damage Source");

        if (Sprite.Material is { ResourceLocalToScene: false } material)
        {
            Sprite.Material = material.Clone();
        }

        if (Root is GravityObjectBase gob && HurtDetector is { } myHurtDetector)
        {
            gob.GrabReleased += e => OnGrabReleased(e, gob, myHurtDetector);
            gob.HitEnemyWhenThrown += (c, isKiss) => OnHitOthers(c, isKiss, myHurtDetector);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Sprite is { } sprite && Root is GravityObjectBase gob)
        {
            var shouldFlipH = IGrabbable.IsGrabbedByPlayer(gob) ? false : gob.AnimationDirection < 0;
            if (sprite.FlipH != shouldFlipH)
            {
                sprite.FlipH = shouldFlipH;
                sprite.Position *= new Vector2(-1, 1);
            }
            if (gob is IDynamicAnimationSpeedEnemy das)
            {
                sprite.SpeedScale = das.AnimationSpeedScale;
            }
        }
    }

    private static void OnGrabReleased(Mario.GrabReleaseEvent e, GravityObjectBase myGob, EnemyHurtDetector myEhd)
    {
        if (e.Flags.HasFlag(Mario.GrabReleaseFlags.Gently))
        {
            return;
        }

        if (TestIsInWall(myGob))
        {
            myEhd.Kill(new DamageEvent
            {
                DamageToEnemy = 100,
                DamageTypes = DamageType.KickShell,
                AttackVector = new Vector2(myGob.Grabber.GlobalPosition.X - myGob.GlobalPosition.X, 0),
                DirectSource = myGob.Grabber,
                TrueSource = myGob.Grabber,
            });
        }
    }

    private static bool TestIsInWall(GravityObjectBase myGob)
    {
        var state = myGob.GetWorld2D().DirectSpaceState;
        var param = new PhysicsShapeQueryParameters2D
        {
            Shape = myGob.Shape.Shape,
            CollideWithAreas = false,
            CollideWithBodies = true,
            CollisionMask = MaFo.CollisionMask.Solid | MaFo.CollisionMask.SolidEnemyOnly,
            Exclude = new Array<Rid> { myGob.GetRid() },
        };
        return TestInWallOffsetList.All(offset =>
        {
            param.Transform = myGob.GlobalTransform.TranslatedLocal(offset);
            return state.IntersectShapeTyped(param).Any();
        });
    }

    private static readonly Vector2[] TestInWallOffsetList =
    {
        8 * Vector2.Right,
        8 * Vector2.Up,
        8 * Vector2.Left,
        8 * Vector2.Down,
        8 * Vector2.Right.Rotated(Mathf.Pi / 4),
        8 * Vector2.Up.Rotated(Mathf.Pi / 4),
        8 * Vector2.Left.Rotated(Mathf.Pi / 4),
        8 * Vector2.Down.Rotated(Mathf.Pi / 4),
    };

    private void OnHitOthers(EnemyCore it, bool isKiss, EnemyHurtDetector myHurtDetector)
    {
        if (it.HurtDetector is not { } itsHurtDetector) return;
        var trueSource = (Root as IGrabbable)?.Grabber ?? this;
        // 伤害对方
        var e = new DamageEvent
        {
            DamageToEnemy = 100,
            DamageTypes = DamageType.KickShell,
            DirectSource = Root,
            TrueSource = trueSource,
            ComboTracker = GetComboTracker(),
        };
        // 亲嘴必须能够杀死对方时才会触发
        if (isKiss && !itsHurtDetector.CanBeOneHitKilledBy(e))
        {
            return;
        }

        itsHurtDetector.HurtBy(e);

        static bool IsShell(EnemyCore enemy)
        {
            return enemy.Root is GravityObjectBase { WillHurtOthers: true } && !enemy.DieWhenThrownAndHitOther;
        }

        // 自己暴毙
        if (!isKiss && Root is not IGrabbable { IsGrabbed: true })
        {
            if (!DieWhenThrownAndHitOther) return;
        }
        myHurtDetector.Kill(e with
        {
            DirectSource = it.Root,
            EventFlags = DamageEvent.Flags.Silent,
        });
    }

    private Node2D _root;
    private static readonly NodePath NpSprite = "Sprite";
    private static readonly NodePath NpAnimation = "Animation Player";
}