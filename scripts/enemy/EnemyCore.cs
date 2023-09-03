using ChloePrime.MarioForever.RPG;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class EnemyCore : Area2D
{
    [Export] private Node2D RootOverride { get; set; }
    [Export] public AnimatedSprite2D Sprite { get; private set; }

    public Node2D Root { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Root = RootOverride ?? (GetParent() as MarioForeverNpc);
    }

    public bool IsFriendly()
    {
        return Root is not MarioForeverNpc npc || npc.Friendly;
    }
}