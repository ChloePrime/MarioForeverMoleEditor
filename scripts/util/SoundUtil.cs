using System.Collections.Generic;
using Godot;

namespace MarioForeverMoleEditor.scripts.util;

public static class SoundUtil
{
    private const int MaxPooledPlayers = 64;
    
    public static void PlaySound(this Node node, AudioStream sound)
    {
        if (sound is null)
        {
            return;
        }
        var player = TryPurgeAndPop(out var p) ? p : NewPlayer();
        player.Stream = sound;
        if (player.GetParent() is null)
        {
            node.GetTree().Root.AddChild(player);
        }
        player.Play();
    }

    private static AudioStreamPlayer NewPlayer()
    {
        var player = new AudioStreamPlayer();
        player.Finished += () =>
        {
            if (PlayerPool.Count >= MaxPooledPlayers)
            {
                player.QueueFree();
            }
            else
            {
                PlayerPool.Push(player);
            }
        };
        return player;
    }

    private static bool TryPurgeAndPop(out AudioStreamPlayer ret)
    {
        while (PlayerPool.TryPop(out var player))
        {
            // 清理跨场景时可能被释放的实例
            if (player != null && !GodotObject.IsInstanceValid(player))
            {
                continue;
            }

            ret = player;
            return true;
        }

        ret = null;
        return false;
    }

    private static readonly Stack<AudioStreamPlayer> PlayerPool = new();
}