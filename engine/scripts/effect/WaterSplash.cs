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
        Visible = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_pending)
        {
            var detectorOldPos = _waterDetector.Position;
            if (_waterDetector.MoveAndCollide(WaterDetectDistance) is not null)
            {
                this.TeleportTo(ToGlobal(_waterDetector.Position - detectorOldPos));
                _waterDetector.Position = detectorOldPos;
            }
            _pending = false;
            Visible = true;
        }
    }

    private static readonly NodePath NpWaterDetector = "Surface Detector";
    private bool _pending;
    private PhysicsBody2D _waterDetector;
}