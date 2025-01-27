using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Level;

public partial class LevelFrame : LevelManager
{
    [Export]
    public bool UseFilter
    {
        get => _useFilter;
        set => SetUseFilter(value);
    }

    [Export] 
    public Material FilterMaterial { get; private set; } = GD.Load<Material>("res://engine/resources/level/super_sai.mtl.tres");
    
    
    [ExportGroup("Advanced")]
    [Export] private Control LevelContainer { get; set; }
    [Export] private SubViewport LevelViewport { get; set; }
    
    private Vector2I LevelViewportInitialSize { get; set; }

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
        _ready.Value = true;
        LevelViewportInitialSize = LevelViewport.Size;
        SetUseFilter(UseFilter);
    }

    public override void _Input(InputEvent e)
    {
        base._Input(e);
        if (e is InputEventKey { PhysicalKeycode: Key.F6, Pressed: true })
        {
            UseFilter = !UseFilter;
        }
    }

    private bool SetUseFilter(bool value)
    {
        if (!_ready.Value)
        {
            goto ret;
        }

        LevelContainer.Material = value ? FilterMaterial : null;
        LevelViewport.Size = value ? LevelViewportInitialSize / 2 : LevelViewportInitialSize;
        
        ret:
        return _useFilter = value;
    }

    private void OnResize()
    {
        var sizeRate = _game.Position.X / this.Size.X;
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
    private UnserializedContainer<bool> _ready;
    private bool _useFilter;
}