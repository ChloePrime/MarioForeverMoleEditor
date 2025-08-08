global using static ChloePrime.MarioForever.Util.InputHelper;
using Godot;

namespace ChloePrime.MarioForever;

public static class Main
{
    public static readonly bool HasNativePhysicsInterpolation = true;
    
    public static void Init()
    {
        if (_init) return;
        _init = true;

        if (!HasNativePhysicsInterpolation)
        {
            var rate = DisplayServer.ScreenGetRefreshRate();

            if (rate < 119)
            {
                Engine.PhysicsTicksPerSecond = Mathf.CeilToInt(rate * 2);
                GD.Print("Using double fps for physics");
            }
            else
            {
                Engine.PhysicsTicksPerSecond = Mathf.CeilToInt(rate);
                GD.Print($"Limiting physics fps rate to {Mathf.CeilToInt(rate)}");
            }

            if (Engine.MaxFps == 0 && DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Disabled)
            {
                Engine.MaxFps = Mathf.CeilToInt(rate);
                GD.Print($"Limiting max fps to {Mathf.CeilToInt(rate)}");
            }
        }
        
        RenderingServer.SetDefaultClearColor(Colors.Black);
    }

    private static bool _init;
}