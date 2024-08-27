using System;
using System.Collections.Generic;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Level;

/// <summary>
/// 自动无限循环的背景图片
/// </summary>
[GlobalClass]
public partial class LevelBackgroundManager : Node
{
    [Export] public bool WrapX { get; set; } = true;
    [Export] public bool WrapY { get; set; }
    [Export] public Vector2 ParallaxRatio { get; set; } = Vector2.One;
    [Export] public Vector2 PositionOffset { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        _adapter = MainNodeAdapter.Of(GetParent<Node2D>());
        _intitalOffset = MainNode.GlobalPosition - MainNode.GetFrame().Size / 2;
        InitSubSprites();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        var size = _adapter .Size * MainNode.GlobalScale.Abs();
        var viewport = MainNode.GetViewport();
        var camera = viewport.GetCamera2D();
        var screenSize = viewport.GetVisibleRect().Size;
        var screenCenter = MainNode.GetFrame().GetCenter();
        var start = new Vector2(camera?.LimitLeft ?? 0, camera?.LimitTop ?? 0);
        var pos = (MainNode.GetFrame().GetCenter() - start - size / 2) * (Vector2.One - ParallaxRatio) + start + size / 2 +
                  PositionOffset;
        
        if (WrapX)
        {
            {
                var xLeft = pos.X - size.X / 2;
                while (xLeft < (screenCenter.X - screenSize.X / 2) + 1e-5)
                {
                    xLeft += size.X;
                }
                pos.X = xLeft + size.X / 2;
            }
            {
                var xRight = pos.X + size.X / 2;
                while (xRight > (screenCenter.X + screenSize.X / 2) - 1e-5)
                {
                    xRight -= size.X;
                }
                pos.X = xRight - size.X / 2;
            }
        }
        else
        {
            pos.X = ParallaxRatio.X == 0 ? screenCenter.X + _intitalOffset.X : MainNode.GlobalPosition.X;
        }
        
        if (WrapY)
        {
            {
                var yTop = pos.Y - size.Y / 2;
                while (yTop < (screenCenter.Y - screenSize.Y / 2) + 1e-5)
                {
                    yTop += size.Y;
                }
                pos.Y = yTop + size.Y / 2;
            }
            {
                var yBottom = pos.Y + size.Y / 2;
                while (yBottom > (screenCenter.Y + screenSize.Y / 2) - 1e-5)
                {
                    yBottom -= size.Y;
                }
                pos.Y = yBottom - size.Y / 2;
            }
        }
        else
        {
            pos.Y = ParallaxRatio.Y == 0 ? screenCenter.Y + _intitalOffset.Y : MainNode.GlobalPosition.Y;
        }
        _adapter.Node.GlobalPosition = pos;
    }

    private void InitSubSprites()
    {
        var size = _adapter.Size * _adapter.Node.GlobalScale;
        var windowSize = MainNode.GetViewportRect().Size;
        var builder = new List<(Node2D, Vector2)>();
        var w = WrapX ? Math.Max(3, 2 * Mathf.CeilToInt(windowSize.X / size.X) + 2) : 1;
        var h = WrapY ? Math.Max(3, 2 * Mathf.CeilToInt(windowSize.Y / size.Y) + 2) : 1;
        for (var x = -(w / 2); x <= w / 2; x++)
        for (var y = -(h / 2); y <= h / 2; y++)
        {
            if (x == 0 && y == 0) continue;
            builder.Add((_adapter.CreateSub(), new Vector2(x, y)));
        }
        _subs = builder.ToArray();
        foreach (var (sub, _) in _subs)
        {
            Callable.From(() => MainNode.AddChild(sub)).CallDeferred();
        }
        OnTextureChanged();
    }

    private void OnTextureChanged()
    {
        var size = _adapter.Size;
        foreach (var (sub, relPos) in _subs)
        {
            _adapter.InitSub(sub);
            sub.Position = relPos * size;
        }
    }
    
    public abstract class MainNodeAdapter
    {
        public abstract Node2D Node { get; }
        public abstract Vector2 Size { get; }
        public abstract Node2D CreateSub();
        public abstract void InitSub(Node2D sub);

        public static MainNodeAdapter Of(Node2D node)
        {
            return node switch
            {
                Sprite2D sprite => new Sprite2DAdapter(sprite),
                AnimatedSprite2D as2 => new AnimatedSprite2DAdapter(as2),
                _ => throw new ApplicationException(
                    $"Unknown {nameof(LevelBackgroundManager)} main node type: {node.GetClass()}"
                )
            };
        }
    }
    
    public class Sprite2DAdapter(Sprite2D sprite) : MainNodeAdapter
    {
        public override Node2D Node => sprite;
        public override Vector2 Size { get; } = sprite.Texture.GetSize();
        public override Node2D CreateSub() => new Sprite2D();

        public override void InitSub(Node2D sub)
        {
            ((Sprite2D)sub).Texture = sprite.Texture;
        }
    }
    
    public class AnimatedSprite2DAdapter(AnimatedSprite2D sprite) : MainNodeAdapter
    {
        public override Node2D Node => sprite;
        public override Vector2 Size { get; } = sprite.SpriteFrames.GetFrameTexture(sprite.Animation, 0).GetSize();
        public override Node2D CreateSub() => new AnimatedSprite2D();
        public override void InitSub(Node2D sub)
        {
            ((AnimatedSprite2D)sub).SpriteFrames = sprite.SpriteFrames;
            ((AnimatedSprite2D)sub).Animation = sprite.Animation;
            ((AnimatedSprite2D)sub).Play();
            ((AnimatedSprite2D)sub).Frame = sprite.Frame;
            ((AnimatedSprite2D)sub).FrameProgress = sprite.FrameProgress;
        }
    }

    private Vector2 _intitalOffset;
    private (Node2D, Vector2)[] _subs;
    private MainNodeAdapter _adapter;
    private Node2D MainNode => _adapter.Node;
}