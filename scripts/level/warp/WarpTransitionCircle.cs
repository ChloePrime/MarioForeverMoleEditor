using System;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Level.Warp;

public enum WarpTransitionType
{
    In,
    Out,
}

public partial class WarpTransitionCircle : WarpTransition
{
    [Export] public float Duration = 0.5F;
    [Export] public Tween.TransitionType TweenType = Tween.TransitionType.Quad;
    
    private static readonly string NpViewport = "AspectRatioContainer/SubViewportContainer/SubViewport";
    private static readonly string NpCircle = "AspectRatioContainer/SubViewportContainer/SubViewport/Circle";
    private PlayerFinder _player;
    private Sprite2D _circle;
    private Vector2 _originalCirclePos;
    private float _originalViewportSize;
    private Tween _tween;

    public override void _Ready()
    {
        this.GetNode(out _circle, NpCircle);
        _player = new PlayerFinder(this);
        _originalCirclePos = _circle.Position;
        _originalViewportSize = GetNode<SubViewport>(NpViewport).Size2DOverride.X / 2F;
    }

    protected override void _BeginTransition(WarpTransitionType type)
    {
        var (pos, scale) = _player.TryGet(out var player)
            ? GetScaleOfCircle(player)
            : (null, 1);
        
        GD.Print($"pos={pos},scale={scale}");
        
        base._BeginTransition(type);
        _tween?.Free();
        _tween = null;
        var tween = _tween = this.CreateTween();
        var scaleVec = new Vector2(scale, scale);
        _circle.Position = _originalCirclePos + (pos ?? Vector2.Zero);
        _circle.Scale = type switch
        {
            WarpTransitionType.In => scaleVec,
            WarpTransitionType.Out => Vector2.Zero,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
        tween
            .TweenProperty(_circle, (string)Node2D.PropertyName.Scale, scaleVec - _circle.Scale, Duration)
            .SetEase(type switch
            {
                WarpTransitionType.In => Tween.EaseType.Out,
                WarpTransitionType.Out => Tween.EaseType.In,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            })
            .SetTrans(TweenType);

        tween
            .TweenCallback(Callable.From(CompleteTransition))
            .SetDelay(Duration);
    }

    protected override void _EndTransition()
    {
        _tween = null;
    }

    private (Vector2?, float) GetScaleOfCircle(Node2D mario)
    {
        if (mario.GetViewport()?.GetCamera2D() is not { } camera)
        {
            return (null, 1);
        }
        var centerPos = mario.ToGlobal(mario is Mario m
            ? new Vector2(0, -m.CurrentSize.GetIdealHeight() / 2.0F)
            : Vector2.Zero);
        var circlePos = ToScreenLocal(camera, centerPos);
        var frame = mario.GetFrame();
        var distance = FurthestDistanceToEndpoint(circlePos, new Rect2(Vector2.Zero, frame.Size));
        GD.Print($"distance={distance}");
        return (circlePos, (float)(distance / _originalViewportSize));
    }

    private static Vector2 ToScreenLocal(Camera2D camera, Vector2 globalPos)
    {
        var transform = camera.GlobalTransform with { Origin = camera.GetScreenCenterPosition() };
        return transform.AffineInverse() * globalPos;
    }

    private static double FurthestDistanceToEndpoint(Vector2 point, Rect2 frame)
    {
        var resultSq = 0.0;
        foreach (var endpoint in (Span<Vector2>)[
                     frame.Position,
                     frame.End,
                     new Vector2(frame.Position.X, frame.End.Y), 
                     new Vector2(frame.End.X, frame.Position.Y)
                 ])
        {
            resultSq = Math.Max(resultSq, point.DistanceSquaredTo(endpoint));
        }
        return Mathf.Sqrt(resultSq);
    }
}