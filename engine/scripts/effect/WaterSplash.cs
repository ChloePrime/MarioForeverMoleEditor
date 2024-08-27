using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Util.HelperNodes;
using Godot;

namespace ChloePrime.MarioForever.Effect;

public partial class WaterSplash : SelfDestroyingEffect
{
    public static readonly Vector2 WaterDetectDistance = new(0, 96);
    
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _waterDetector, NpWaterDetector);
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        _pending = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_pending)
        {
            if (_waterDetector.MoveAndCollide(WaterDetectDistance) is not null)
            {
                GlobalPosition = _waterDetector.GlobalPosition;
                _waterDetector.Position = Vector2.Zero;
            }
            _pending = false;
        }
    }

    private static readonly NodePath NpWaterDetector = "Surface Detector";
    private bool _pending;
    private PhysicsBody2D _waterDetector;
}