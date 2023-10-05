using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Player;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Level.Warp;

/// <summary>
/// 水管出口，比入口多了防止出水管后被食人花啃脚的功能
/// </summary>
public partial class PipeExit : PipeEntrance
{
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _clampDetector, NpClampDetector);
        _clampDetector.AreaEntered += OnClampDetectorAreaEntered;
        MarioArrived += OnMarioArrivedPipeExit;
    }

    private void OnMarioArrivedPipeExit(Mario _)
    {
        if (IsInstanceValid(_clamp))
        {
            _clamp.ShrinkAndForceShy();
        }
        else
        {
            _clamp = null;
        }
    }

    private void OnClampDetectorAreaEntered(Area2D area)
    {
        if (area is not EnemyHurtDetector { Root: ClampFlower clamp })
        {
            return;
        }
        _clamp = clamp;
        CallDeferred(MethodName.DisableClampDetector);
    }

    private void DisableClampDetector()
    {
        _clampDetector.Monitoring = _clampDetector.Monitorable = false;
    }

    private static readonly NodePath NpClampDetector = "Clamp Detector";
    private ClampFlower _clamp;
    private Area2D _clampDetector;
}