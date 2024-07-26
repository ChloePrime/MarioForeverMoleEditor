using System.Linq;
using ChloePrime.MarioForever.Level;
using Godot;

namespace ChloePrime.MarioForever.Util;

public static class InputHelper
{
    public static void GamepadHitFeedback(float weak, float strong)
    {
        RumbleGamepads(weak, strong, 0.1F);
    }

    public static void RumbleGamepads(float weak, float strong, float duration = 0)
    {
        foreach (var gamepad in GetGamepads())
        {
            Input.StartJoyVibration(gamepad, weak, strong, duration);
        }
    }

    private static int[] GetGamepads()
    {
        if (GamepadManager.TryGetInstance(out var manager))
        {
            return manager.CurrentGamepad is { } gamepad ? [gamepad] : [];
        }
        // [[unlikely]]
        return Input.GetConnectedJoypads().ToArray();
    }
}