using System;
using System.Collections.Generic;
using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Shared;
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
        return XSpeed >= MinSpeed ? 0.5F : 0.6F;
    }
    
    private void PhysicsProcessY(float delta)
    {
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

            if (!collided)
            {
                if (YSpeed > 0)
                {
                    PredictAndStomp(delta);
                }
                if (YSpeed < 0)
                {
                    TestHiddenBumpables(delta);
                }
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

        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = CurrentCollisionShape.Shape,
            CollisionMask = MaFo.CollisionMask.Enemy,
            Transform = trans,
            Exclude = MeInAnArray,
            CollideWithAreas = true,
        };
        foreach (var result in GetWorld2D().DirectSpaceState.IntersectShapeTyped(query))
        {
            if (result.Collider is IStompable stompable && WillStomp(stompable))
            {
                stompable.StompBy(this);
            }
        }
    }

    private void TestHiddenBumpables(float delta)
    {
        var any = false;
        var predict = -YSpeed * delta - 4;
        foreach (var result in TestBump(MaFo.CollisionMask.HiddenBonus, predict, BumpWidthPolicy.UseHitboxWidth))
        {
            if (result.Collider is not CollisionObject2D collider) continue;
            if (result.Collider is not IBumpable { Hidden: true } bumpable) continue;
            var shape = collider.ShapeOwnerGetShape(collider.ShapeFindOwner(result.Shape), 0);
            var blockHeight = shape.GetRect().Size.Y;
            var marioHeight = CurrentCollisionShape.Shape.GetRect().Size.Y;
            if (-ToLocal(collider.GlobalPosition).Y >= marioHeight + blockHeight / 2)
            {
                bumpable.OnBumpBy(this);
                any = true;
            }
        }
        if (any)
        {
            YSpeed = 0;
            OnHeadHit();
        }
    }

    private void OnHeadHit()
    {
        foreach (var result in TestBump(MaFo.CollisionMask.Solid, 0, BumpWidthPolicy.UseIdealWidth))
        {
            if (result.Collider is IBumpable bumpable)
            {
                bumpable.OnBumpBy(this);
            }
        }
    }


    private static readonly RectangleShape2D BumpDetector = new();
    
    private enum BumpWidthPolicy
    {
        UseHitboxWidth,
        UseIdealWidth,
    }
    
    private IEnumerable<ShapeHitResult> TestBump(uint mask, float predict, BumpWidthPolicy widthPolicy)
    {
        var marioSize = CurrentCollisionShape.Shape.GetRect().Size;
        var width = widthPolicy switch
        {
            BumpWidthPolicy.UseIdealWidth => _currentSize.GetIdealWidth(),
            BumpWidthPolicy.UseHitboxWidth or _ => marioSize.X,
        };
        BumpDetector.Size = new Vector2(width, 8);
        var trans = GlobalTransform.TranslatedLocal(new Vector2(0, -marioSize.Y - predict));
        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = BumpDetector,
            CollisionMask = mask,
            Transform = trans,
            CollideWithAreas = false,
        };
        return GetWorld2D().DirectSpaceState.IntersectShapeTyped(query);
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