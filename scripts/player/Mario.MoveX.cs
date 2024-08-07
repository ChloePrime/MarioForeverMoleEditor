﻿using System;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Util;
using Godot;
using Vector2 = Godot.Vector2;

namespace ChloePrime.MarioForever.Player;

public partial class Mario
{
    /// <summary>
    /// X 速度的绝对值
    /// </summary>
    public float XSpeed { get; set; }

    /// <summary>
    /// X 速度的方向，-1 或 1，不一定等于角色朝向
    /// </summary>
    /// <see cref="CharacterDirection"/> 代表角色朝向的值
    /// <see cref="ChloePrime.MarioForever.GameRule.CharacterDirectionPolicy"/>
    public int XDirection { get; private set; } = 1;

    /// <summary>
    /// 角色的横向朝向，不一定等于 X 速度的方向
    /// </summary>
    public int CharacterDirection
    {
        get => GameRule.CharacterDirectionPolicy switch
        {
            GameRule.MarioDirectionPolicy.FollowControlDirection => _controlDirection,
            GameRule.MarioDirectionPolicy.FollowXSpeed => XDirection,
            _ => throw new ArgumentOutOfRangeException(),
        };
        set
        {
            value = value >= 0 ? 1 : -1;
            _ = GameRule.CharacterDirectionPolicy switch
            {
                GameRule.MarioDirectionPolicy.FollowControlDirection => _controlDirection = value,
                GameRule.MarioDirectionPolicy.FollowXSpeed => XDirection = value,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public float NaturalXFriction => 1 / Math.Max(1e-4F, Slipperiness + 1);

    private bool _leftPressed;
    private bool _rightPressed;
    private bool _runPressed;
    private bool _firePressed;
    private int _controlDirection = 1;

    private float _walkAxis;
    private float _walkResult;
    [CtfFlag(0)] private bool _running;
    [CtfFlag(1)] private bool _walking;
    [CtfFlag(10)] private bool _turning;
    private bool _sprinting;
    private float _burstCharge;

    /// <summary>
    /// RE: 马里奥移动
    /// </summary>
    private void PhysicsProcessX(float delta)
    {
        if (IsClimbing) return;

        if (MotionMode is not MotionModeEnum.Grounded)
        {
            MotionMode = MotionModeEnum.Grounded;
        }
        
        _running = _runPressed;
        _walkAxis = FetchWalkingInput();
        _walkAxis = (IsCrouching && !IsInAir) ? 0 : (Mathf.IsZeroApprox(_walkAxis) ? 0 : _walkAxis);
        _walking = _walkAxis != 0;

        if (XSpeed <= 0 && GameRule.CharacterDirectionPolicy == GameRule.MarioDirectionPolicy.FollowControlDirection)
        {
            XDirection = _controlDirection;
        }
        if (!_leftPressed && !_rightPressed && !IsInAir && XSpeed > MaxSpeedWhenWalking)
        {
            _controlDirection = XDirection;
        }
        else if (_leftPressed != _rightPressed)
        {
            _controlDirection = (_leftPressed ? -1 : 0) + (_rightPressed ? 1 : 0);
        }

        // RE: Flag10控制转向过程
        if (XDirection * _walkAxis < 0 && XSpeed > 0)
        {
            _turning = true;
        }
        if (XDirection * _walkAxis > 0)
        {
            _turning = false;
        }

        // RE: 同时按住左和右会有问题，此处修正
        if (_leftPressed && _rightPressed && XSpeed > MinSpeed)
        {
            var acc = _running ? AccelerationWhenRunning : AccelerationWhenWalking;
            XSpeed = Math.Max(MinSpeed, XSpeed - acc * delta);
        }

        // RE: 转向时先减速
        if (_turning)
        {
            if (_walking && XSpeed > 0)
            {
                XSpeed -= AccelerationWhenTurning * delta;
            }
            if (XSpeed <= 0 && XDirection * _walkAxis < 0)
            {
                _turning = false;
                XSpeed = -1;
            }
        }
        if (_turning && XSpeed <= 0 && _walkAxis == 0)
        {
            _turning = false;
            XSpeed = 0;
        }

        // RE: 改变方向
        if (!_turning && !HasCompletedLevel && _walking)
        {
            XDirection = Math.Sign(_walkAxis);
        }

        // ME: 冲刺计算
        ProcessBurst(delta);

        // RE: 走/跑
        if (_walking && !_turning)
        {
            float max, acc;
            if (!_running)
            {
                max = MaxSpeedWhenWalking;
                acc = AccelerationWhenWalking;
            }
            else
            {
                max = _sprinting ? MaxSpeedWhenSprinting : MaxSpeedWhenRunning;
                acc = AccelerationWhenRunning;
            }
            if (IsInWater && !GameRule.KeepXSpeedInWater)
            {
                max = MaxSpeedInWater;
            }

            if (XSpeed < max)
            {
                XSpeed = Math.Min(max, XSpeed + acc * delta);
            }
            if (XSpeed > max)
            {
                XSpeed = Math.Max(max, XSpeed - acc * delta);
            }
        }

        var natFriction = NaturalXFriction;
        if (!_walking && XSpeed > 0)
        {
            XSpeed = Math.Max(0, XSpeed - AccelerationWhenWalking * natFriction * delta);
        }
        if (!_running && XSpeed > MaxSpeedWhenWalking)
        {
            XSpeed = Math.Max(MaxSpeedWhenWalking, XSpeed - AccelerationWhenRunning * natFriction * delta);
        }

        // RE: 初速度
        if (_walking && !_turning && XSpeed < MinSpeed)
        {
            XSpeed += MinSpeed;
        }
        if (_leftPressed && _rightPressed && XSpeed < MinSpeed && !IsCrouching && !HasCompletedLevel)
        {
            XSpeed += MinSpeed;
        }

        // RE: 马里奥出屏判定
        if (!AllowMoveOutOfScreen && !HasCompletedLevel)
        {
            var pos = GlobalPosition;
            var x = pos.X;
            var frame = this.GetFrame();
            var xLeftFrame = frame.Position.X;
            var xRightFrame = frame.End.X;
            var farOutScreen = x - xLeftFrame < -ScreenBorderPadding || xRightFrame - x < -ScreenBorderPadding;
            if (!farOutScreen)
            {
                var leftHitScreen = x - xLeftFrame < ScreenBorderPadding;
                var rightHitScreen = xRightFrame - x < ScreenBorderPadding;
                var leftAtBorder = Mathf.IsEqualApprox(x - xLeftFrame, ScreenBorderPadding);
                var rightAtBorder = Mathf.IsEqualApprox(xRightFrame - x, ScreenBorderPadding);
                if (leftHitScreen || ((XDirection < 0 || Mathf.IsZeroApprox(XSpeed)) && leftAtBorder))
                {
                    pos.X = xLeftFrame + ScreenBorderPadding;
                    _posBeforePhProcess = _posAfterPhProcess = GlobalPosition = pos;
                    XSpeed = 0;
                    return;
                }
                if (rightHitScreen || ((XDirection > 0 || Mathf.IsZeroApprox(XSpeed)) && rightAtBorder))
                {
                    pos.X = xRightFrame - ScreenBorderPadding;
                    _posBeforePhProcess = _posAfterPhProcess = GlobalPosition = pos;
                    XSpeed = 0;
                    return;
                }
            }
        }

        if (HasCompletedLevel)
        {
            XDirection = 1;
            XSpeed = Units.Speed.CtfMovementToGd(20);
        }

        // 属于 Godot 的实际移动部分
        Velocity = new Vector2(XSpeed * XDirection, 0);
        var collided = MoveAndSlide();
        _walkResult = Velocity.X * delta;

        if (collided && XSpeed > 0 && Mathf.IsZeroApprox(Math.Abs(Velocity.X)))
        {
            if (IsOnFloor() || _xSpeedKeepTimer > 1F / 15)
            {
                XSpeed = 0;
                Velocity = Vector2.Zero;
                _xSpeedKeepTimer = 0;
            }
            else
            {
                _xSpeedKeepTimer += delta;
            }
        }
        else
        {
            _xSpeedKeepTimer = 0;
        }
    }

    private float _xSpeedKeepTimer;
    
    private float FetchWalkingInput()
    {
        if (HasCompletedLevel)
        {
            _leftPressed = false;
            _rightPressed = true;
            return 1;
        }
        if (ControlIgnored)
        {
            _leftPressed = _rightPressed = false;
            return 0;
        }
        _leftPressed = Input.IsActionPressed(Constants.ActionMoveLeft);
        _rightPressed = Input.IsActionPressed(Constants.ActionMoveRight);
        // 同时按下左 + 右时判定为停止
        if (_leftPressed && _rightPressed)
        {
            return 0;
        }
        return Input.GetAxis(Constants.ActionMoveLeft, Constants.ActionMoveRight);
    }

    private void OnMarioEnterWaterMoveX()
    {
        if (!GameRule.KeepXSpeedInWater)
        {
            XSpeed = Mathf.MoveToward(XSpeed, 0, EnterWaterDepulse);
        }
    }

    private void ProcessBurst(float delta)
    {
        if (!GameRule.EnableMarioBursting)
        {
            _sprinting = false;
            _burstCharge = 0;
            return;
        }
        if (_running && !_turning && !IsInWater && XSpeed >= MaxSpeedWhenRunning - 1e-3)
        {
            if (!IsInAir)
            {
                _burstCharge.MoveToward(SprintChargeTime, delta);
            }
            if (!_sprinting && _burstCharge >= SprintChargeTime)
            {
                SprintStartSound?.Play();
                _sprintSmokeTimer.EmitSignal(Timer.SignalName.Timeout);
                _sprintSmokeTimer.Start();
                XSpeed = MaxSpeedWhenSprinting;
                _sprinting = true;
            }
        }
        else
        {
            if (_sprinting)
            {
                _sprintSmokeTimer.Stop();
                _sprinting = false;
            }
            _burstCharge.MoveToward(0, SprintCooldownSpeed * delta);
        }
    }
}