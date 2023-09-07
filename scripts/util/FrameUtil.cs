#nullable enable
using ChloePrime.MarioForever.Level;
using Godot;

namespace ChloePrime.MarioForever.Util;

public static class FrameUtil
{
    public static Rect2 GetFrame(this Node2D node)
    {
        var viewport = node.GetViewport();
        var rect = viewport.GetVisibleRect();
        rect.Position += (viewport.GetCamera2D()?.GetScreenCenterPosition() - rect.Size / 2) ?? Vector2.Zero;
        return rect;
    }

    public static Vector2 GetSpriteSize(this AnimatedSprite2D sprite)
    {
        if (sprite.SpriteFrames is not { } frames)
        {
            return Vector2.Zero;
        }
        else
        {
            return frames.GetFrameTexture(sprite.Animation, sprite.Frame)?.GetSize() ?? Vector2.Zero;
        }
    }

    public static MaFoLevel? GetLevel(this Node node)
    {
        return node.FindParentOfType<MaFoLevel>();
    }

    public static LevelManager? GetLevelManager(this Node node)
    {
        return node.FindParentOfType<LevelManager>();
    }

    public static T? FindParentOfType<T>(this Node node) where T: class
    {
        do
        {
            if (node is T t)
            {
                return t;
            }
        } while ((node = node.GetParent()) != null);

        return null;
    }
    

    public static bool FindParentOfType<T>(this Node node, out T? parent) where T: class
    {
        return (parent = node.FindParentOfType<T>()) != null;
    }
}