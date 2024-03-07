using System.Collections.Generic;
using System.Threading.Tasks;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Facility;

public partial class MovingPlatformBorderMark : MovingPlatformMark
{
    [Export] public float Acceleration { get; set; } = Units.Acceleration.CtfToGd(0.1F);

    protected override void OnPlatformEntered(MovingPlatform platform)
    {
        var velocity = platform.Velocity;
        if (velocity.IsZeroApprox())
        {
            return;
        }
        var axis = velocity.Abs().MaxAxisIndex();
        var speed = axis == Vector2.Axis.X ? velocity.X : velocity.Y;
        var relPos = ToLocal(platform.ToGlobal(new Vector2(0, 8)));
        var relPosInAxis = axis == Vector2.Axis.X ? relPos.X : relPos.Y;
        if ((speed > 0 && relPosInAxis > 8) || (speed < 0 && relPosInAxis < -8))
        {
            return;
        }  
        HandlePlatform(platform, speed, axis, Acceleration);
        OnMarkUsed();
    }

    private async void HandlePlatform(MovingPlatform platform, float speed, Vector2.Axis axis, float acceleration)
    {
        var id = platform.GetInstanceId();
        if (!_handling.Add(id))
        {
            return;
        }
        await HandlePlatform0(platform, speed, axis, acceleration);
        if (IsInstanceValid(this))
        {
            _handling.Remove(id);
        }
    }

    private static async ValueTask HandlePlatform0(MovingPlatform platform, float speed, Vector2.Axis axis, float acceleration)
    {
        var targetSpeed = -speed;
        while (!Mathf.IsEqualApprox(speed, targetSpeed))
        {
            await platform.WaitForPhysicsProcess();
            speed.MoveToward(targetSpeed, acceleration * (float)platform.GetPhysicsProcessDeltaTime());
            if (axis == Vector2.Axis.X)
            {
                platform.XSpeed = speed;
            }
            else
            {
                platform.YSpeed = speed;
            }
        }
    }

    private readonly HashSet<ulong> _handling = [];
}