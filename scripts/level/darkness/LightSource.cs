using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Level.Darkness;

public partial class LightSource : Node2D
{
    [Export] public PackedScene Light { get; private set; } = GD.Load<PackedScene>("res://resources/level/darkness/spot_light.tscn");

    public override void _Ready()
    {
        base._Ready();
        SpawnLight();
    }

    public void SpawnLight()
    {
        if (Light is not { } prefab || this.GetLevelManager() is not { } manager)
        {
            return;
        }
        var light = Light.Instantiate();
        manager.DarknessManager.LightRoot.AddChild(light);
        if (light is Node2D light2d)
        {
            light2d.GlobalPosition = GlobalPosition;
        }
        if (light is Light lightObj)
        {
            lightObj.BoundObject = this;
        }
    }
}