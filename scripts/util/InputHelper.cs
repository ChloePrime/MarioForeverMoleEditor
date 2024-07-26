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
        foreach (var gamepad in Input.GetConnectedJoypads())
        {
            Input.StartJoyVibration(gamepad, weak, strong, duration);
        }
    }
}