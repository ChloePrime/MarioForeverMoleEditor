using System;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Level.Warp;
using Godot;

namespace ChloePrime.MarioForever.Player;

public enum MarioPipeState
{
    NotInPipe,
    Entering,
    TransitionIn,
    TransitionOut,
    Exiting,
    Smb3Moving,
}

public partial class Mario
{
    public MarioPipeState PipeState { get; set; }
    public PackedScene WarpTransitionPrefab { get; set; } = GD.Load<PackedScene>("res://objects/level/O_warp_transition_circle.tscn");
    public StringName PipeForceAnimation { get; set; }
    public int ZIndexBeforePipe { get; set; }
    public float PipeGrabbedObjectXOffsetShrink { get; set; } = 1;

    public event Action RequireTeleport;
    public event Action TransitionCompleted;

    public void BeginWarpTransitionIn()
    {
        Visible = false;
        PipeState = MarioPipeState.TransitionIn;

        StartTransition(WarpTransitionType.In, OnTransitionInFinished);
        return;

        async void OnTransitionInFinished()
        {
            RequireTeleport?.Invoke();
            RequireTeleport = null;
            await this.DelayAsync(0.1F);
            _transitionInstance.QueueFree();
            _transitionInstance = null;
            BeginWarpTransitionOut();
        }
    }
    
    public void BeginWarpTransitionOut()
    {
        PipeState = MarioPipeState.TransitionOut;
        StartTransition(WarpTransitionType.Out, () =>
        {
            _transitionInstance.QueueFree();
            _transitionInstance = null;
            TransitionCompleted?.Invoke();
            TransitionCompleted = null;
            Visible = true;
            PipeState = MarioPipeState.Exiting;
        });
    }

    private static readonly PackedScene ImmediateTransition = GD.Load<PackedScene>("res://objects/level/O_warp_transition_immediate.tscn");
    private static WarpTransition _transitionInstance;

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
        _transitionInstance?.ProcessTransition(delta);
        
        switch (PipeState)
        {
            case MarioPipeState.TransitionIn:
                if (_transitionInstance == null)
                {
                    BeginWarpTransitionIn();
                }
                break;
            case MarioPipeState.TransitionOut:
                if (_transitionInstance == null)
                {
                    BeginWarpTransitionOut();
                }
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

    private void StartTransition(WarpTransitionType type, Action callback)
    {
        var node = WarpTransitionPrefab.Instantiate();
        if (node is not WarpTransition transition)
        {
            node.QueueFree();
            GD.PushError($"The script of WarpTransitionType should be child class of {nameof(WarpTransition)}, found {node.GetType().Name}");
            _transitionInstance = ImmediateTransition.Instantiate<WarpTransition>();
        }
        else
        {
            _transitionInstance = transition;
        }
        var instance = _transitionInstance;
        AddChild(instance);
        instance.TransitionCompleted += () =>
        {
            instance.EndTransition();
            callback();
        };
        instance.BeginTransition(type);
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