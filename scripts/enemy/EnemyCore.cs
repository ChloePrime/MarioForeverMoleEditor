using System.Diagnostics.CodeAnalysis;
using ChloePrime.MarioForever.RPG;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class EnemyCore : Node2D, IMarioForeverNpc
{
    [Export] private Node2D RootOverride { get; set; }
    [Export, MaybeNull] public AnimatedSprite2D Sprite { get; private set; }
    
    /// <summary>
    /// 优先使用 <see cref="Root"/> 的 NPC 数据，随后才会用这里的
    /// </summary>
    [Export, MaybeNull] public MarioForeverNpcData MyNpcData { get; private set; }

    public Node2D Root { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Root = (RootOverride ?? (GetParent() as WalkableNpc)) ?? this;
    }
    
    public MarioForeverNpcData NpcData => Root == this ? MyNpcData : (Root as IMarioForeverNpc)?.NpcData ?? MyNpcData ?? MarioForeverNpcData.SafeFallback;
}