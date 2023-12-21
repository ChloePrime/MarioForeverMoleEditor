using System;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Player;

public enum MarioPipeState
{
    NotInPipe,
    Entering,
    TransitionBegin,
    TransitionEnd,
    Exiting,
    Smb3Moving,
}

public partial class Mario
{
    public MarioPipeState PipeState { get; set; }
    public StringName PipeForceAnimation { get; set; }
    public float PipeGrabbedObjectXOffsetShrink { get; set; } = 1;

    public event Action RequireTeleport;
    public event Action TransitionCompleted;

    private void ProcessPipe(float delta)
    {
        if (PipeState != MarioPipeState.NotInPipe)
        {
            _internalTrackedInPipe = true;
        }
        else
        {
            _internalTrackedInPipe = false;
            EndInPipe();
            return;
        }
        _skidSound.Stop();
        ShrinkGrabMuzzle();
        switch (PipeState)
        {
            case MarioPipeState.TransitionBegin:
                PipeState = MarioPipeState.TransitionEnd;
                if (_camera.GetParent() == this)
                {
                    _camera.Reparent(this.GetLevel() ?? this.GetPreferredRoot());
                }
                RequireTeleport?.Invoke();
                RequireTeleport = null;
                break;
            case MarioPipeState.TransitionEnd:
                PipeState = MarioPipeState.Exiting;
                TransitionCompleted?.Invoke();
                TransitionCompleted = null;
                break;
        }
    }

    private void ShrinkGrabMuzzle()
    {
        var grabMuzzle = GrabMuzzle;
        grabMuzzle.Position = grabMuzzle.Position with
        {
            X = _grabMuzzleOriginalXBySize[(int)CurrentSize] * PipeGrabbedObjectXOffsetShrink
        };
    }

    private void EndInPipe()
    {
        for (var i = 0; i < GrabMuzzleBySize.Count; i++)
        {
            var grabMuzzle = GrabMuzzleBySize[i];
            grabMuzzle.Position = grabMuzzle.Position with { X = _grabMuzzleOriginalXBySize[i] };
        }
    }

    private bool _internalTrackedInPipe;
}