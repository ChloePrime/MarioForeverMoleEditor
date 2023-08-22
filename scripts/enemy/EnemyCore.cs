using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class EnemyCore : Area2D
{
    [Export] public Node2D RootOverride { get; private set; }
    [Export] public AnimatedSprite2D Sprite { get; private set; }

    public Node2D Root { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Root = RootOverride ?? (GetParent() as Node2D);
    }
}