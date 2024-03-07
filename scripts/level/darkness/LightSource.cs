using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Level.Darkness;

public partial class LightSource : Node2D
{
    [Export] public PackedScene Light { get; private set; } = GD.Load<PackedScene>("res://resources/level/darkness/spot_light.tscn");

    [Export]
    public float LightSize
    {
        get => _lightSize;
        set => _lightSize = value;
    }

    [Export]
    public bool AnimatedDestroy
    {
        get => _animatedDestroy;
        set => SetAnimatedDestroy(value);
    }

    public void SpawnLight()
    {
        if (Light is not { } prefab || this.GetLevelManager() is not { } manager)
        {
            return;
        }
        if (_light is { } oldLight)
        {
            oldLight.QueueFree();
        }
        var light = prefab.Instantiate();
        manager.DarknessManager.LightRoot.AddChild(light);
        if (light is Node2D light2d)
        {
            light2d.GlobalPosition = GlobalPosition;
        }
        if (light is Light lightObj)
        {
            lightObj.BoundObject = this;
            _light = lightObj;
            SetAnimatedDestroy(AnimatedDestroy);
            SetLightSize(LightSize);
        }
    }

    private void SetAnimatedDestroy(bool value)
    {
        _animatedDestroy = value;
        if (_light is { } light)
        {
            light.Animated = _animatedDestroy;
        }
    }
    

    private void SetLightSize(float value)
    {
        if (value == 0) value = 1;
        _lightSize = value;
        
        if (_light is { } light)
        {
            light.Scale = new Vector2(value, value);
        }
    }

    private Light _light;
    private bool _animatedDestroy;
    private float _lightSize = 1;

    public override void _Ready()
    {
        base._Ready();
        SpawnLight();
    }
}