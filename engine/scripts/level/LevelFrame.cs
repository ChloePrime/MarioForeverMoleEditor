using ChloePrime.Godot.Util;
using Godot;

namespace ChloePrime.MarioForever.Level;

public partial class LevelFrame : LevelManager
{
    [ExportGroup("Advanced")]
    [Export] private Control LevelContainer { get; set; }
    [Export] private SubViewport LevelViewport { get; set; }

    public override void _Ready()
    {
        Main.Init();
        base._Ready();
        this.GetNode(out _la, NpLeftAnimFrame); 
        this.GetNode(out _ra, NpRightAnimFrame);
        this.GetNode(out _ln, NpLeftNormalFrame);
        this.GetNode(out _rn, NpRightNormalFrame);
        this.GetNode(out _game, NpGameContainer);
        GetWindow().ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
        GetWindow().SizeChanged += OnResize;
    }

    private void OnResize()
    {
        var sizeRate = _game.Position.X / Size.X;
        _la.AnchorLeft = _ln.AnchorLeft = -sizeRate;
        _ra.AnchorRight = _rn.AnchorRight = 1 + sizeRate;
        _la.Position = _ln.Position = new Vector2(+0.5F, _ra.Position.Y);
        _ra.Position = _rn.Position = new Vector2(-0.5F + _game.Position.X + _game.Size.X, _ra.Position.Y);
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