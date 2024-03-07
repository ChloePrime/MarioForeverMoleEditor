using System;
using System.Threading.Tasks;
using Godot;

namespace ChloePrime.Godot.Util;

public static class AsyncUtil
{
    public static Task DelayAsync(
        this Node node,
        float time,
        bool processAlways = true,
        bool processInPhysics = false,
        bool ignoreTimeScale = false)
    {
        return WrapCall(node, () =>
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
        });
    }

    public static Task WaitForPhysicsProcess(this Node node)
    {
        return WrapCall(node, () =>
        {
            TaskCompletionSource tsc = new();
            var tree = node.GetTree();

            tree.PhysicsFrame += OnPhysicsFrame;
            return tsc.Task;

            void OnPhysicsFrame()
            {
                if (GodotObject.IsInstanceValid(tree))
                {
                    tree.PhysicsFrame -= OnPhysicsFrame;
                }
                if (GodotObject.IsInstanceValid(node))
                {
                    tsc.SetResult();
                }
            }
        });
    }

    private static Task WrapCall(Node node, Func<Task> call)
    {
        if (node.IsInsideTree())
        {
            return call.Invoke();
        }
        
        TaskCompletionSource tsc2 = new();
        node.TreeEntered += OnNodeEnteredTree;
        return tsc2.Task;

        async void OnNodeEnteredTree()
        {
            node.TreeEntered -= OnNodeEnteredTree;
            await call();
            tsc2.SetResult();
        }
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