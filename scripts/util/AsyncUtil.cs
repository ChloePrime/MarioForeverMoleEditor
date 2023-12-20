using System;
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
        if (!node.IsInsideTree())
        {
            TaskCompletionSource tsc2 = new();
            node.TreeEntered += OnNodeEnteredTree;
            return tsc2.Task;

            async void OnNodeEnteredTree()
            {
                node.TreeEntered -= OnNodeEnteredTree;
                await DelayAsync(node, time, processAlways, processInPhysics, ignoreTimeScale);
                tsc2.SetResult();
            }
        }
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

    public static void StartSafely(this Timer timer, double timeSec = -1D)
    {
        if (timer.IsInsideTree())
        {
            timer.Start(timeSec);
            return;
        }
        timer.TreeEntered += OnTimerEnteredTree;
        return;

        void OnTimerEnteredTree()
        {
            timer.Start(timeSec);
            timer.TreeEntered -= OnTimerEnteredTree;
        }
    }
}