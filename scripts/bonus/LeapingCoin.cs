using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Bonus;

public partial class LeapingCoin : Node2D
{
    [Export] public PackedScene Score { get; set; } = GD.Load<PackedScene>("res://objects/ui/O_score_200.tscn"); 
    
    public override void _Ready()
    {
        GlobalData.Coins++;
        this.GetNode(out _sprite, NpSprite);
        _sprite.AnimationFinished += OnSpriteAnimationFinished;
    }

    private void OnSpriteAnimationFinished()
    {
        TrySummonScore();
        QueueFree();
    }

    private void TrySummonScore()
    {
        if (GetParent() is not { } parent || Score is not { } scorePrefab)
        {
            return;
        }
        var is2d = scorePrefab.TryInstantiate(out Node2D score2D, out var score);
        parent.AddChild(score);
        if (is2d)
        {
            score2D.GlobalPosition = _sprite.GlobalPosition;
        }
    }

    private static readonly NodePath NpSprite = "Sprite";
    private AnimatedSprite2D _sprite;
}