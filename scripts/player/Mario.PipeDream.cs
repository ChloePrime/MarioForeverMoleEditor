using System;
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

    public event Action RequireTeleport;
    public event Action TransitionCompleted;

    private void UpdatePipe(float delta)
    {
        _skidSound.Stop();
        switch (PipeState)
        {
            case MarioPipeState.TransitionBegin:
                RequireTeleport?.Invoke();
                RequireTeleport = null;
                PipeState = MarioPipeState.TransitionEnd;
                break;
            case MarioPipeState.TransitionEnd:
                PipeState = MarioPipeState.Exiting;
                TransitionCompleted?.Invoke();
                TransitionCompleted = null;
                break;
        }
    }
}