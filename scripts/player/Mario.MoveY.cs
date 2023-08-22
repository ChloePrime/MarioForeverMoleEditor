using System;
using System.Linq;
using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Player;

public partial class Mario
{
    public float GetGravityScale()
    {
        if (!_jumpPressed || YSpeed >= 0)
        {
            return 1;
        }
        return _xSpeed >= MinSpeed ? 0.5F : 0.6F;
    }
    
    private void PhysicsProcessY(double deltaD)
    {
        var delta = (float)deltaD;
        MoveY(delta);

        if (_isInAir)
        {
            if (!_isInWater)
            {
                YSpeed = Math.Min(YSpeed + Gravity * GetGravityScale() * delta, MaxYSpeed);
            }
            else
            {
                var acc = _downPressed ? GravityWhenSinking : GravityInWater;
                YSpeed += acc * delta;
                
                if (!_downPressed && YSpeed > MaxYSpeedInWater)
                {
                    YSpeed = Math.Max(MaxYSpeedInWater, YSpeed - 10 * GravityInWater * delta);
                }
                if (YSpeed < -MaxYSpeedInWater)
                {
                    YSpeed = Math.Min(-MaxYSpeedInWater, YSpeed + 2 * GravityInWater * delta);
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

    private void MoveY(float delta)
    {
        if (_isInAir)
        {
            Velocity = new Vector2(0, YSpeed);
            var collided = MoveAndSlide();

            if (!collided && YSpeed > 0)
            {
                PredictAndStomp(delta);
            }

            if (collided && Mathf.IsZeroApprox(Math.Abs(Velocity.Y)))
            {
                if (YSpeed >= 0)
                {
                    _isInAir = false;
                    OnFallOnGround();
                }
                else
                {
                    OnHeadHit();
                    YSpeed = 0;
                }
            }
        }
        else
        {
            var collided = MoveAndCollide(GroundTestVec, true) != null;
            if (collided)
            {
                YSpeed = 0;
            }
            _isInAir = !collided;
        }

        if (!_isInAir)
        {
            _wilyJumpTime = JumpTolerateTime;
        }
    }

    private void PredictAndStomp(float delta)
    {
        var trans = CurrentCollisionShape.GetGlobalTransform().TranslatedLocal(new Vector2(0, YSpeed / Engine.PhysicsTicksPerSecond));
        // trans.Origin += GetRealVelocity() * delta;

        var query = new PhysicsShapeQueryParameters2D()
        {
            Shape = CurrentCollisionShape.Shape,
            CollisionMask = MaFo.CollisionMask.Enemy,
            Transform = trans,
            Exclude = MeInAnArray,
            CollideWithAreas = true
        };
        foreach (var result in GetWorld2D().DirectSpaceState.IntersectShape(query).Select(d => new ShapeHitResult3D(d)))
        {
            if (result.Collider is IStompable stompable && WillStomp(stompable))
            {
                stompable.StompBy(this);
            }
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
                Swim(Math.Clamp(min, max, -YSpeed + acc));
            }
            else
            {
                
            }
        }
    }

    private static readonly Vector2 GroundTestVec = 8 * Vector2.Down;
    [CtfFlag(11)] private bool _isInAir = true;
    private float _wilyJumpTime;
    private bool _jumpPressed;
    private bool _upPressed;
    private bool _downPressed;
    private bool _comboJumpAsked;
}