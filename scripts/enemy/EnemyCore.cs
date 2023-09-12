using System.Diagnostics.CodeAnalysis;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using Godot;
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
    public Node2D Root => _root ??= (RootOverride ?? (GetParent() as WalkableNpc)) ?? this;
    public MarioForeverNpcData NpcData => Root == this ? MyNpcData : (Root as IMarioForeverNpc)?.NpcData ?? MyNpcData ?? MarioForeverNpcData.SafeFallback;
    public IMarioForeverNpc AsNpc => this;

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
            gob.HitEnemyWhenThrown += (it, isKiss) =>
            {
                if (it.HurtDetector is not { } itsHurtDetector) return;
                // 伤害对方
                var e = new DamageEvent
                {
                    DamageToEnemy = 100,
                    DamageTypes = DamageType.KickShell,
                    DirectSource = this,
                };
                // 亲嘴必须能够杀死对方时才会触发
                if (isKiss && !itsHurtDetector.CanBeOneHitKilledBy(e))
                {
                    return;
                }
                itsHurtDetector.HurtBy(e);
                
                // 自己暴毙
                if (!isKiss && Root is not IGrabbable { IsGrabbed: true })
                {
                    if (!DieWhenThrownAndHitOther) return;
                }
                myHurtDetector.HurtBy(new DamageEvent
                {
                    DamageTypes = DamageType.KickShell,
                    DamageToEnemy = 100,
                    DirectSource = it.Root,
                    TrueSource = it,
                    EventFlags = DamageEvent.Flags.Silent,
                });
            };
        }
    }

    private Node2D _root;
    private static readonly NodePath NpSprite = "Sprite";
    private static readonly NodePath NpAnimation = "Animation Player";
}