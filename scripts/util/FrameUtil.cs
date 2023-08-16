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
}