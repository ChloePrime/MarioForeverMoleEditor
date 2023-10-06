using System.Collections.Generic;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Player;

[GlobalClass]
public partial class YsmMarioSprite3D : Node3D, IAnimatedSprite
{
    [Export] public float WalkingSpeedScale { get; private set; } = 4;
    [Export] public float RunningSpeedScale { get; private set; } = 2;

    public float GetIntrinsicSpeedScale(StringName anim)
    {
        if (anim == Mario.Constants.AnimWalking)
        {
            return WalkingSpeedScale;
        }
        if (anim == Mario.Constants.AnimRunning)
        {
            return RunningSpeedScale;
        }
        if (anim == Mario.Constants.AnimCrouching)
        {
            return 0;
        }
        return 1;
    }
    
    private static readonly Dictionary<StringName, StringName> AnimNameMappings = new()
    {
        { Mario.Constants.AnimStopped, "idle" },
        { Mario.Constants.AnimWalking, "walk" },
        { Mario.Constants.AnimRunning, "run" },
        { Mario.Constants.AnimTurning, "run" },
        { Mario.Constants.AnimLaunching, "swing_hand" },
        { Mario.Constants.AnimJumping, "jump" },
        { Mario.Constants.AnimLeaping, "fly" },
        { Mario.Constants.AnimFalling, "jump" },
        { Mario.Constants.AnimCrouching, "sneak" },
    };

    public override void _Ready()
    {
        base._Ready();
        _mario = this.FindParentOfType<Mario>();
    }

    public StringName Animation
    {
        get => _animation;
        set
        {
            _animation = value;
            if (AnimNameMappings.TryGetValue(value, out var ysmAnim))
            {
                Player.Play(ysmAnim);
            }
            Player.SpeedScale = SpeedScale * GetIntrinsicSpeedScale(value);
        }
    }

    public float SpeedScale
    {
        get => Player.SpeedScale / GetIntrinsicSpeedScale(_animation);
        set => Player.SpeedScale = value * GetIntrinsicSpeedScale(_animation);
    }
    
    public void Play()
    {
        Player.Play();
    }

    public void Reset()
    {
        Player.Stop();
        Player.Play();
    }

    public event AnimationPlayer.AnimationFinishedEventHandler AnimationFinished
    {
        add => Player.AnimationFinished += value;
        remove => Player.AnimationFinished -= value;
    }

    private static readonly NodePath NpAnimPlayer = "AnimationPlayer";
    private Mario _mario;
    public AnimationPlayer Player => _player ??= GetNode<AnimationPlayer>(NpAnimPlayer);
    private AnimationPlayer _player;
    private StringName _animation;
}