using System.Threading.Tasks;
using Godot;

namespace ChloePrime.MarioForever.Util;

public static class AsyncUtil
{
    public static Task DelayAsync(
        this Node node,
        float time,
        bool processAlways = true,
        bool processInPhysics = false,
        bool ignoreTimeScale = false)
    {
        TaskCompletionSource tsc = new();
        node.GetTree().CreateTimer(time, processAlways, processInPhysics, ignoreTimeScale).Timeout += () =>
        {
            if (GodotObject.IsInstanceValid(node))
            {
                tsc.SetResult();
            }
        };
        return tsc.Task;
    }
}