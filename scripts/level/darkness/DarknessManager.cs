using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Level.Darkness;

public partial class DarknessManager : Control
{
    public float BaseDarkness { get => _baseDarkness ??= GetBaseDarkness() ; set => SetBaseDarkness((_baseDarkness = value).Value); }

    public Node LightRoot => _lr ??= GetNode("Viewport Container/Sub Viewport/Light Root");
    public CanvasItem ViewportRenderer => _vr ??= GetNode<CanvasItem>("Viewport Container");
    public LevelManager LevelManager => _lm ??= this.FindParentOfType<LevelManager>();

    private static readonly StringName BaseDarknessParamName = "base_darkness";

    private Node _lr;
    private LevelManager _lm;
    private Camera2D _camera;
    private CanvasItem _vr;

    private float GetBaseDarkness()
    {
        if (ViewportRenderer.Material is ShaderMaterial sm)
        {
            return sm.GetShaderParameter(BaseDarknessParamName).AsSingle();
        }
        
        GD.PrintErr($"Failed to get base darkness of {nameof(DarknessManager)}");
        return 0;
    }

    private void SetBaseDarkness(float value)
    {
        if (ViewportRenderer.Material is ShaderMaterial sm)
        {
            sm.SetShaderParameter(BaseDarknessParamName, value);
            return;
        }
        
        GD.PrintErr($"Failed to set base darkness of {nameof(DarknessManager)}");
    }

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _camera, "Viewport Container/Sub Viewport/Camera");
        this.GetNode<Timer>("GC Timer").Timeout += OnGcTimeTimeout;
        LightRoot.ChildEnteredTree += OnLightRootChildEnteredTree;
    }

    private void OnGcTimeTimeout()
    {
        foreach (var child in LightRoot.Children())
        {
            if (child is Light light)
            {
                Callable.From(light.OnLightGC).CallDeferred();
            }
        }
    }

    private void OnLightRootChildEnteredTree(Node node)
    {
        if (node is Light light)
        {
            light.DarknessManager = this;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (LevelManager.LevelInstance?.GetViewport()?.GetCamera2D() is { } camera)
        {
            _camera.GlobalPosition = camera.GetScreenCenterPosition();
            _camera.GlobalRotation = camera.GlobalRotation;
            _camera.GlobalScale = camera.GlobalScale;
            _camera.GlobalSkew = camera.GlobalSkew;
        }
    }

    private float? _baseDarkness;
}