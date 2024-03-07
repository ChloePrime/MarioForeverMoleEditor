using System;
using System.Collections.Generic;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Effect;
using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Facility;
using ChloePrime.MarioForever.Shared;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Player;

public partial class Mario
{
    /// <summary>
    /// 从水管进入水区时不会被触发
    /// </summary>
    [Signal]
    public delegate void JumpedIntoWaterEventHandler();

    /// <summary>
    /// 从水管离开水区时不会被触发
    /// </summary>
    [Signal]
    public delegate void JumpedOutOfWaterEventHandler();
    
    public float GetGravityScale()
    {
        if (!_jumpPressed || YSpeed >= 0)
        {
            return 1;
        }
        return XSpeed >= MinSpeed ? 0.5F : 0.6F;
    }
    
    public void Jump()
    {
        Jump(JumpStrength);
        _jumpSound.Play();
    }

    public void JumpOutOfWater()
    {
        Jump(JumpStrengthOutOfWater);
        _canLeaveOfWater = true;
    }

    public void Swim()
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
            Swim(Mathf.Clamp(-YSpeed + acc, min, max));
        }
        else if (!_downPressed)
        {
            JumpOutOfWater();
        }
    }

    public void Swim(float strength)
    {
        Jump(strength);
        _swimSound.Play();
        if (_currentSprite is { } sprite)
        {
            sprite.Reset();
        }
    }

    public void Jump(float strength)
    {
        var bonus = GameRule.XSpeedBonus * XSpeed / MaxSpeedWhenRunning + (_sprinting ? GameRule.SprintingBonus : 0);
        YSpeed = -strength - bonus;
        _isInAir = true;
        _wilyJumpTime = -1;
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
                if (YSpeed < 0 && !_canLeaveOfWater)
                {
                    if (!_waterSurfaceDetector2.HasOverlappingAreas() && !_waterSurfaceDetector2.HasOverlappingBodies())
                    {
                        if (_upPressed && !_downPressed)
                        {
                            if (!_canLeaveOfWater)
                            {
                                JumpOutOfWater();
                            }
                        }
                        else
                        {
                            YSpeed = 0;
                            goto Input;
                        }
                    }
                }
                if (YSpeed > 0)
                {
                    _canLeaveOfWater = false;
                }
                
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
        
        Input:

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
            MoveAndSlide();
            var onFloor = IsOnFloor();
            var onCeil = IsOnCeiling();
            var collidedY = onFloor || onCeil;

            if (!collidedY)
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
            else
            {
                if (onFloor)
                {
                    _isInAir = false;
                    OnFallOnGround();
                }
                if (onCeil)
                {
                    OnHeadHit();
                }
                YSpeed = 0;
            }
        }

        if (!_isInAir)
        {
            _wilyJumpTime = JumpTolerateTime;
            
            var collided = MoveAndCollide(GroundTestVec, true);
            if (collided != null)
            {
                YSpeed = 0;
                if (collided.GetCollider() is IMarioStandable standable)
                {
                    standable.ProcessMarioStandOn(this);
                }
            }
            _isInAir = collided == null;
        }
    }

    private void PredictAndStomp(float delta)
    {
        var trans = CurrentCollisionShape.GetGlobalTransform().TranslatedLocal(new Vector2(0, YSpeed / Engine.PhysicsTicksPerSecond));

        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = CurrentCollisionShape.Shape,
            CollisionMask = MaFo.CollisionMask.Solid | MaFo.CollisionMask.SolidMarioOnly | MaFo.CollisionMask.Enemy,
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
            OnHeadHit(predict);
            YSpeed = 0;
        }
    }

    private void OnHeadHit(float predict = 0)
    {
        foreach (var result in TestBump(MaFo.CollisionMask.Solid, predict, BumpWidthPolicy.UseIdealWidth))
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
            BumpWidthPolicy.UseIdealWidth => CurrentSize.GetIdealWidth(),
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
        StompComboTracker.Reset();
        if (_comboJumpAsked)
        {
            if (MoveAndCollide(GroundTestVec, true) is {} collision)
            {
                if (collision.GetCollider() is IMarioStandable standable)
                {
                    standable.ProcessMarioStandOn(this);
                }
            }
            Callable.From(_isInWater ? Swim : Jump).CallDeferred();
            _comboJumpAsked = false;
        }
    }

    /// <summary>
    /// 跳跃
    /// </summary>
    private void ConsumeJumpInput()
    {
        if (ControlIgnored) return;
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
        if (ControlIgnored) return;
        // 游泳
        if (Input.IsActionJustPressed(Constants.ActionJump))
        {
            Swim();
        }
    }

    private void WaterReady()
    {
        this.GetNode(out _waterDetector, Constants.NpWaterDetector);
        this.GetNode(out _waterSurfaceDetector, Constants.NpWaterSurfaceDetector);
        this.GetNode(out _waterSurfaceDetector2, Constants.NpWaterSurfaceDetector2);
        _waterDetector.AreaEntered += OnMarioJumpedIntoWater;
        _waterDetector.BodyEntered += OnMarioJumpedIntoWater;
        _waterDetector.AreaExited += OnMarioJumpedOutOfWater;
        _waterDetector.BodyExited += OnMarioJumpedOutOfWater;
        _waterSurfaceDetector.AreaEntered += OnMarioEnterDeepwater;
        _waterSurfaceDetector.BodyEntered += OnMarioEnterDeepwater;
        _waterSurfaceDetector.AreaExited += OnMarioNearWaterSurface;
        _waterSurfaceDetector.BodyExited += OnMarioNearWaterSurface;
    }

    private void OnMarioJumpedIntoWater(Node2D _)
    {
        _waterStack++;
        if (!_isInWater)
        {
            _isInWater = true;
            EnterWater();
        }
    }

    private void EnterWater()
    {
        if (PipeState != MarioPipeState.NotInPipe) return;
        JumpIntoWaterSound?.Play();
        Callable.From(PopWaterSplash).CallDeferred();
        EmitSignal(SignalName.JumpedIntoWater);
    }

    private static readonly PackedScene WaterSplashPrefab = GD.Load<PackedScene>("res://resources/shared/water_splash.tscn");
    private static readonly Vector2 WaterSplashMuzzle = new(0, -64);

    private void PopWaterSplash()
    {
        var splash = WaterSplashPrefab.Instantiate<WaterSplash>();
        this.GetPreferredRoot().AddChild(splash);
        splash.GlobalPosition = ToGlobal(WaterSplashMuzzle);
    }
    
    private void OnMarioJumpedOutOfWater(Node2D _)
    {
        _waterStack--;
        if (_waterStack == 0 && _isInWater)
        {
            _isInWater = false;
            ExitWater();
        }
    }

    private void ExitWater()
    {
        if (PipeState != MarioPipeState.NotInPipe) return;
        if (!IsInstanceValid(this)) return;
        JumpOutOfWaterSound?.Play();
        EmitSignal(SignalName.JumpedOutOfWater);
    }

    private void OnMarioEnterDeepwater(Node2D _)
    {
        _deepWaterStack++;
        _isNearWaterSurface = false;
    }

    private void OnMarioNearWaterSurface(Node2D _)
    {
        _deepWaterStack--;
        if (_deepWaterStack == 0)
        {
            _isNearWaterSurface = true;
        }
    }

    private static readonly Vector2 GroundTestVec = 8 * Vector2.Down;
    private Area2D _waterDetector;
    private Area2D _waterSurfaceDetector;
    private Area2D _waterSurfaceDetector2;
    [CtfFlag(11)] private bool _isInAir = true;
    [CtfFlag(12)] private bool _isInWater;
    private bool _isNearWaterSurface = true;
    private bool _canLeaveOfWater;
    private float _wilyJumpTime;
    private bool _jumpPressed;
    private bool _upPressed;
    private bool _downPressed;
    private bool _comboJumpAsked;
    private int _waterStack;
    private int _deepWaterStack;
}