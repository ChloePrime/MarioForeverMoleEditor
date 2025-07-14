using System;
using Godot;

namespace ChloePrime.MarioForever.Level;

/**
 * 需要挂到 AutoLoad 上
 */
public partial class BackgroundMusic : AudioStreamPlayer
{
    public static BackgroundMusic Instance { get; private set; }
    public static AudioStream Music
    {
        get => Instance.Stream;
        set => SetStream(value);
    }

    public static float Speed
    {
        get => _speed;
        set => SetSpeed(value);
    }

    /// <summary>
    /// 播放音乐，但是重复调用不会重置播放进度
    /// </summary>
    public static void Start()
    {
        CheckInstance();
        if (Instance.Stream != null && !Instance.Playing)
        {
            Instance.Play();
        }
    }

    public new static void Stop()
    {
        CheckInstance();
        ((AudioStreamPlayer)Instance).Stop();
    }

    public BackgroundMusic()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        base._Ready();
        _bus = AudioServer.GetBusIndex(Bus = "Background Music");
    }

    private new static void SetStream(AudioStream music)
    {
        CheckInstance();
        if (music == Instance.Stream)
        {
            if (!Instance.Playing)
            {
                Instance.Play();
            }
            return;
        }
        if (music == null)
        {
            ((AudioStreamPlayer)Instance).Stop();
        }
        Instance.Stream = music;
        if (music != null)
        {
            Instance.Play();
        }
        
        // 刷新音乐速度，以修复
        // 时间小于 100 时在 MPT 音乐与非 MPT 音乐之间切换时播放速度变为倒数的 bug
        Speed = Speed;
    }

    private static void CheckInstance()
    {
        if (Instance == null)
        {
            throw new ApplicationException($"{nameof(BackgroundMusic)}.{Instance} not initialized, you should put this class in Godot's auto load list.");
        }
    }

    private static void SetSpeed(float speed)
    {
        _speed = speed;

        if (Music.IsMetaStream())
        {
            return;
        }

        Instance.PitchScale = Music.GetClass() == "AudioStreamMPT" ? Math.Clamp(1 / speed, 0, 10_0000) : speed;

        if (AudioServer.GetBusEffect(_bus, 0) is AudioEffectPitchShift effect)
        {
            if (Mathf.IsEqualApprox(speed, 1))
            {
                AudioServer.SetBusEffectEnabled(_bus, 0, false);
            }
            else
            {
                AudioServer.SetBusEffectEnabled(_bus, 0, true);
                effect.PitchScale = 1 / speed;
            }
        }
    }

    private static int _bus;
    private static float _speed = 1;
}