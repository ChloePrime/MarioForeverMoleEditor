using System;
using System.Collections.Generic;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Level;

/// <summary>
/// 自动无限循环的背景图片
/// </summary>
[GlobalClass]
public partial class LevelBackground : Sprite2D
{
    [Export] public bool WrapX { get; set; } = true;
    [Export] public bool WrapY { get; set; }
    [Export] public Vector2 ParallaxRatio { get; set; } = Vector2.One;
    [Export] public Vector2 PositionOffset { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        _intitalOffset = GlobalPosition - this.GetFrame().Size / 2;
        TextureChanged += OnTextureChanged;
        InitSubSprites();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Texture is not { } texture)
        {
            return;
        }
        var size = texture.GetSize() * GlobalScale.Abs();
        var viewport = GetViewport();
        var camera = viewport.GetCamera2D();
        var screenSize = viewport.GetVisibleRect().Size;
        var screenCenter = this.GetFrame().GetCenter();
        var start = new Vector2(camera?.LimitLeft ?? 0, camera?.LimitTop ?? 0);
        var pos = (this.GetFrame().GetCenter() - start - size / 2) * (Vector2.One - ParallaxRatio) + start + size / 2 +
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
            pos.X = ParallaxRatio.X == 0 ? screenCenter.X + _intitalOffset.X : GlobalPosition.X;
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
            pos.Y = ParallaxRatio.Y == 0 ? screenCenter.Y + _intitalOffset.Y : GlobalPosition.Y;
        }
        GlobalPosition = pos;
    }

    private void InitSubSprites()
    {
        var size = Texture.GetSize() * GlobalScale;
        var windowSize = GetViewportRect().Size;
        var builder = new List<(Sprite2D, Vector2)>();
        var w = WrapX ? Math.Max(3, 2 * Mathf.CeilToInt(windowSize.X / size.X) + 2) : 1;
        var h = WrapY ? Math.Max(3, 2 * Mathf.CeilToInt(windowSize.Y / size.Y) + 2) : 1;
        for (var x = -(w / 2); x <= w / 2; x++)
        for (var y = -(h / 2); y <= h / 2; y++)
        {
            if (x == 0 && y == 0) continue;
            builder.Add((new Sprite2D(), new Vector2(x, y)));
        }
        _subs = builder.ToArray();
        foreach (var (sub, _) in _subs)
        {
            AddChild(sub);
        }
        OnTextureChanged();
    }

    private void OnTextureChanged()
    {
        var tex = Texture;
        var size = tex.GetSize();
        foreach (var (sprite, relPos) in _subs)
        {
            sprite.Texture = tex;
            sprite.Position = relPos * size;
        }
    }

    private Vector2 _intitalOffset;
    private (Sprite2D, Vector2)[] _subs;
}