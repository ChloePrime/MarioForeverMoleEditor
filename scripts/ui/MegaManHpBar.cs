using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.UI;

public partial class MegaManHpBar : Control
{
    [Export] public int Value { get; set; }
    [Export] public int Max { get; set; } = 32;
    [Export] public Color Color1 { get; set; } = DefaultMegaManLayer1Color;
    [Export] public Color Color2 { get; set; } = Colors.White;
    [Export] public int UnitHeightInPixels { get; set; } = 4;

    private static readonly Color DefaultMegaManLayer1Color = Color.Color8(253, 230, 156);

    public void AddHpAnimated(int newValue)
    {
        var value = Value;
        var tween = CreateTween().SetLoops(newValue - value).BindNode(this);
        tween.TweenCallback(Callable.From(() =>
        {
            value++;
            Value = value;
            _addHpSoundPlayer.Stop();
            _addHpSoundPlayer.Play();

            if (value == newValue)
            {
                tween.Stop();
                tween.Dispose();
            }
        })).SetDelay(0.04F);
    }

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _baseBar, "Base");
        this.GetNode(out _valueBar, "Value");
        this.GetNode(out _valueLayer1, "Value/Layer 1");
        this.GetNode(out _valueLayer2, "Value/Layer 2");
        this.GetNode(out _addHpSoundPlayer, "Add HP Sound Player");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_checkV != Value || _checkM != Max)
        {
            UpdateComponents();
        }
        if (_checkC1 != Color1 || _checkC2 != Color2)
        {
            UpdateComponentColor();
        }
    }

    private void UpdateComponents()
    {
        _checkV = Value;
        _checkM = Max;
        var height = _baseBar.Texture.GetHeight();
        
        var mxSizeY = Max * UnitHeightInPixels;
        var mxPosY = height - mxSizeY;
        _baseBar.Size  = _baseBar.Size  with { Y = mxSizeY };
        _baseBar.Position = _baseBar.Position with { Y = mxPosY };
        
        var valSizeY = Value * UnitHeightInPixels;
        var valPosY = height - valSizeY;
        _valueBar.Size     = _valueBar.Size with { Y = valSizeY };
        _valueBar.Position = _valueBar.Position with { Y = valPosY };
    }

    private void UpdateComponentColor()
    {
        _valueLayer1.SelfModulate = _checkC1 = Color1;
        _valueLayer2.SelfModulate = _checkC2 = Color2;
    }
    
    private TextureRect _baseBar;
    private Control _valueBar;
    private TextureRect _valueLayer1;
    private TextureRect _valueLayer2;
    private AudioStreamPlayer _addHpSoundPlayer;
    private Color _checkC1;
    private Color _checkC2;
    private int _checkV = int.MinValue;
    private int _checkM = int.MinValue;
}