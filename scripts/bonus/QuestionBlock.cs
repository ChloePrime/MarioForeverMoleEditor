using System;
using ChloePrime.MarioForever.Enemy;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Bonus;

public partial class QuestionBlock : BumpableBlock
{
    [Export] public PackedScene Content { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _sprite, NpSprite);
        this.GetNode(out _edytor, NpEdytor);
        _sprite.Visible = true;
        _edytor.QueueFree();
    }

    protected override void _OnBumpedBy(Node2D bumper)
    {
        base._OnBumpedBy(bumper);
        if (OneTimeUse)
        {
            _sprite.Animation = AnimUsed;
        }
        if (_watched is { } reference && reference.TryGetTarget(out var watched))
        {
            if (!IsInstanceValid(watched))
            {
                _watched = null;
            }
            else
            {
                var delta = ToLocal(watched.GlobalPosition);
                if (Math.Abs(delta.X) < 1 && delta.Y < 0)
                {
                    return;
                }
            }
        }

        if (GetParent() is not { } parent)
        {
            return;
        }
        if (Content.TryInstantiate(out Node2D content, out var fallback))
        {
            parent.AddChild(content);
            
            var offset = new Vector2(0, -Shape.Shape.GetRect().Size.Y);
            content.GlobalPosition = GlobalTransform.TranslatedLocal(offset).Origin;
            
            if (content is GravityObjectBase gob)
            {
                gob.AppearFrom(-GlobalTransform.Y);
            }

            if (!OneTimeUse)
            {
                _watched = new WeakReference<Node2D>(content);
            }
        }
        else
        {
            parent.AddChild(fallback);
        }
    }

    protected override void _Disable()
    {
        base._Disable();
        _sprite.Stop();
    }

    private static readonly NodePath NpSprite = "Sprite";
    private static readonly NodePath NpEdytor = "Editor Display";
    private static readonly StringName AnimUsed = "used";
    private AnimatedSprite2D _sprite;
    private WeakReference<Node2D> _watched;
    private Sprite2D _edytor;
}