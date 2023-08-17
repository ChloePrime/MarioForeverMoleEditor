using System;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Player;

public partial class Mario
{
    public float GetGravityScale()
    {
        if (!_jumpPressed || _ySpeed >= 0)
        {
            return 1;
        }
        return _xSpeed >= MinSpeed ? 0.5F : 0.6F;
    }
    
    private void PhysicsProcessY(double deltaD)
    {
        var delta = (float)deltaD;
        MoveY();

        if (_isInAir)
        {
            if (!_isInWater)
            {
                _ySpeed = Math.Min(_ySpeed + Gravity * GetGravityScale() * delta, MaxYSpeed);
            }
            else
            {
                var acc = _downPressed ? GravityWhenSinking : GravityInWater;
                _ySpeed += acc * delta;
                
                if (!_downPressed && _ySpeed > MaxYSpeedInWater)
                {
                    _ySpeed = Math.Max(MaxYSpeedInWater, _ySpeed - 10 * GravityInWater * delta);
                }
                if (_ySpeed < -MaxYSpeedInWater)
                {
                    _ySpeed = Math.Min(-MaxYSpeedInWater, _ySpeed + 2 * GravityInWater * delta);
                }
            }
        }

        if (!_completedLevel)
        {
            if (_isInWater)
            {
                ConsumeSwimInput();
            }
            else
            {
                ConsumeJumpInput();
            }
        }

        _wilyJumpTime -= delta;
    }

    private void MoveY()
    {
        if (_isInAir)
        {
            Velocity = new Vector2(0, _ySpeed);
            var collided = MoveAndSlide();

            if (collided && Mathf.IsZeroApprox(Math.Abs(Velocity.Y)))
            {
                if (_ySpeed >= 0)
                {
                    _isInAir = false;
                    OnFallOnGround();
                }
                else
                {
                    OnHeadHit();
                    _ySpeed = 0;
                }
            }
        }
        else
        {
            var collided = MoveAndCollide(GroundTestVec, true) != null;
            if (collided)
            {
                _ySpeed = 0;
            }
            _isInAir = !collided;
        }

        if (!_isInAir)
        {
            _wilyJumpTime = JumpTolerateTime;
        }
    }

    private void OnHeadHit()
    {
        
    }

    private void OnFallOnGround()
    {
        if (_comboJumpAsked)
        {
            CallDeferred(MethodName.Jump);
            _comboJumpAsked = false;
        }
    }

    /// <summary>
    /// 跳跃
    /// </summary>
    private void ConsumeJumpInput()
    {
        if (Input.IsActionJustPressed(Constants.ActionJump))
        {
            if (!_isInAir || _wilyJumpTime >= 0)
            {
                Jump();
            }
            else
            {
                _comboJumpAsked = true;
            }
        }
        if (Input.IsActionJustReleased(Constants.ActionJump))
        {
            _comboJumpAsked = false;
        }
    }

    private void ConsumeSwimInput()
    {
        // 游泳
        if (Input.IsActionJustPressed(Constants.ActionJump))
        {
            if (!_isNearWaterSurface)
            {
                var (min, max, acc) = (_upPressed, _downPressed) switch
                {
                    (true, true) or (false, false) => (SwimStrength, int.MaxValue, SwimStrengthAcc),
                    (true, false) => (
                        MinSwimStrengthWhenFloatingUp,
                        MaxSwimStrengthWhenFloatingUp,
                        SwimStrengthAccWhenFloatingUp
                    ),
                    (false, true) => (
                        MinSwimStrengthWhenSinking,
                        MaxSwimStrengthWhenSinking,
                        SwimStrengthAccWhenSinking
                    )
                };
                Swim(Math.Clamp(min, max, -_ySpeed + acc));
            }
            else
            {
                
            }
        }
    }

    private static readonly Vector2 GroundTestVec = 8 * Vector2.Down;

    private float _ySpeed;
    [CtfFlag(11)] private bool _isInAir = true;
    private float _wilyJumpTime;
    private bool _jumpPressed;
    private bool _upPressed;
    private bool _downPressed;
    private bool _comboJumpAsked;
}