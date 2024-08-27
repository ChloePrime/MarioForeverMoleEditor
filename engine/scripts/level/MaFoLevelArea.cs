using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Level;

[GlobalClass]
public partial class MaFoLevelArea : Node2D
{
    [Export] public Rect2 Border { get; private set; } = DefaultBorder;
    [Export] public AudioStream AreaMusic { get; private set; }
    [Export(PropertyHint.Range, "[0,1]")] public float Darkness { get; set; }

    public MaFoLevel Level => _level ??= this.FindParentOfType<MaFoLevel>();

    public bool SetActivated(bool value)
    {
        SetActivated0(value);
        return value;
    }

    private void SetActivated0(bool value)
    {
        var parent = GetParent();
        if (value && parent == Level) return;
        parent?.RemoveChild(this);
        if (value) Level.AddChild(this);
    }

    public override void _Ready()
    {
        base._Ready();
        LoadTilemaps();
    }

    public virtual void _AreaActivated()
    {
        if (AreaMusic is { } bgm)
        {
            BackgroundMusic.Music = bgm;
        }
        if (Level.FindCamera() is { } camera)
        {
            camera.Border = Border;
            camera.SetDeferred(MaFoCamera.PropertyName.Border, Border);
        }
        if (this.GetLevelManager() is { } manager)
        {
            var isDark = manager.DarknessEnabled = Darkness != 0;
            if (isDark)
            {
                manager.DarknessManager.BaseDarkness = Darkness;
            }
        }
    }

    private static readonly Rect2 DefaultBorder = new(0, 0, 640, 480);
    private MaFoLevel _level;
}