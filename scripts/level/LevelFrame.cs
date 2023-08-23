﻿using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Level;

public partial class LevelFrame : LevelManager
{
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _la, NpLeftAnimFrame);
        this.GetNode(out _ra, NpRightAnimFrame);
        this.GetNode(out _ln, NpLeftNormalFrame);
        this.GetNode(out _rn, NpRightNormalFrame);
        this.GetNode(out _game, NpGameContainer);
        GetWindow().SizeChanged += OnResize;
    }

    private void OnResize()
    {
        var sizeRate = _game.Position.X / this.Size.X;
        _la.AnchorRight = _ln.AnchorRight = sizeRate;
        _ra.AnchorLeft = _rn.AnchorLeft = 1 - sizeRate;
        _ra.Position = new Vector2(_game.Position.X + _game.Size.X, _ra.Position.Y);
    }

    private static readonly NodePath NpLeftNormalFrame = "Frame Curtain/L"; 
    private static readonly NodePath NpRightNormalFrame = "Frame Curtain/R"; 
    private static readonly NodePath NpLeftAnimFrame = "Frame Anime/L"; 
    private static readonly NodePath NpRightAnimFrame = "Frame Anime/R"; 
    private static readonly NodePath NpGameContainer = "Aspect Ratio Container"; 
    private Control _ln, _rn;
    private Control _la, _ra;
    private AspectRatioContainer _game;
}