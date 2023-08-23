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

    public static new void Stop()
    {
        CheckInstance();
        ((AudioStreamPlayer)Instance).Stop();
    }

    public BackgroundMusic()
    {
        Instance = this;
    }

    private static void SetStream(AudioStream music)
    {
        CheckInstance();
        if (music == Instance.Stream)
        {
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
    }

    private static void CheckInstance()
    {
        if (Instance == null)
        {
            throw new ApplicationException($"{nameof(BackgroundMusic)}.{Instance} not initialized, you should put this class in Godot's auto load list.");
        }
    }
}