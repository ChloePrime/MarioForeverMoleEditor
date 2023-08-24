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
    
    public LevelBackground()
    {
        ZIndex = -4000;
    }
    
    public override void _Ready()
    {
        base._Ready();
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
        var pos = (this.GetFrame().GetCenter() - start - size / 2) * (Vector2.One - ParallaxRatio) + start + size / 2;
        
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
            pos.X = GlobalPosition.X;
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
            pos.Y = GlobalPosition.Y;
        }
        GlobalPosition = pos;
    }

    private void InitSubSprites()
    {
        _tl = new Sprite2D();
        _t = new Sprite2D();
        _tr = new Sprite2D();
        _l = new Sprite2D();
        _r = new Sprite2D();
        _bl = new Sprite2D();
        _b = new Sprite2D();
        _br = new Sprite2D();
        foreach (var (sub, _) in SubSprites)
        {
            AddChild(sub);
        }
        OnTextureChanged();
    }

    private void OnTextureChanged()
    {
        var tex = Texture;
        var size = tex.GetSize();
        foreach (var (sprite, relPos) in SubSprites)
        {
            sprite.Texture = tex;
            sprite.Position = relPos * size;
        }
    }

    private IEnumerable<(Sprite2D, Vector2)> SubSprites
    {
        get
        {
            yield return (_tl, RelPosList[0]);
            yield return (_t, RelPosList[1]);
            yield return (_tr, RelPosList[2]);
            yield return (_l, RelPosList[3]);
            yield return (_r, RelPosList[4]);
            yield return (_bl, RelPosList[5]);
            yield return (_b, RelPosList[6]);
            yield return (_br, RelPosList[7]);
        }
    }

    private Sprite2D _tl, _t, _tr, _l, _r, _bl, _b, _br;

    private static readonly Vector2[] RelPosList =
    {
        new(-1, -1),
        new(0, -1),
        new(1, -1),
        new(-1, 0),
        new(1, 0),
        new(-1, 1),
        new(0, 1),
        new(1, 1),
    };
}