using System;
using ChloePrime.MarioForever.Util;
using Godot;

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
    public int CharacterDirection => GameRule.CharacterDirectionPolicy switch
    {
        GameRule.MarioDirectionPolicy.FollowControlDirection => _controlDirection,
        GameRule.MarioDirectionPolicy.FollowXSpeed or _ => XDirection
    };

    private bool _leftPressed;
    private bool _rightPressed;
    private bool _runPressed;
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
        _running = _runPressed;
        _walkAxis = FetchWalkingInput();
        _walkAxis = (_crouching && !_isInAir) ? 0 : (Mathf.IsZeroApprox(_walkAxis) ? 0 : _walkAxis);
        _walking = _walkAxis != 0;

        if (!_leftPressed && !_rightPressed)
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
        if (!_turning && !_completedLevel && _walking)
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
            
            if (XSpeed < max)
            {
                XSpeed = Math.Min(max, XSpeed + acc * delta);
            }
            if (XSpeed > max)
            {
                XSpeed = Math.Max(max, XSpeed - acc * delta);
            }
        }
        if (!_walking && XSpeed > 0)
        {
            XSpeed = Math.Max(0, XSpeed - AccelerationWhenWalking * delta);
        }
        if (!_running && XSpeed > MaxSpeedWhenWalking)
        {
            XSpeed = Math.Max(MaxSpeedWhenWalking, XSpeed - AccelerationWhenRunning * delta);
        }
        
        // RE: 初速度
        if (_walking && !_turning && XSpeed < MinSpeed)
        {
            XSpeed += MinSpeed;
        }
        if (_leftPressed && _rightPressed && XSpeed < MinSpeed && !_crouching && !_completedLevel)
        {
            XSpeed += MinSpeed;
        }
        
        // RE: 马里奥出屏判定
        if (!AllowMoveOutOfScreen)
        {
            var pos = Position;
            var x = pos.X;
            var frame = this.GetFrame();
            var xLeftFrame = frame.Position.X;
            var xRightFrame = frame.End.X;
            var leftHitScreen = x - xLeftFrame <= ScreenBorderPadding;
            var rightHitScreen = xRightFrame - x <= ScreenBorderPadding;
            if (XDirection < 0 && leftHitScreen)
            {
                pos.X = xLeftFrame + ScreenBorderPadding;
                Position = pos;
                XSpeed = 0;
                return;
            }
            if (XDirection > 0 && rightHitScreen)
            {
                pos.X = xRightFrame - ScreenBorderPadding;
                Position = pos;
                XSpeed = 0;
                return;
            }
        }
            
        // 属于 Godot 的实际移动部分
        Velocity = new Vector2(XSpeed * XDirection, 0);
        var collided = MoveAndSlide();
        _walkResult = Velocity.X * delta;

        if (collided && XSpeed > 0 && Mathf.IsZeroApprox(Math.Abs(Velocity.X)))
        {
            XSpeed = 0;
            Velocity = Vector2.Zero;
        }
    }
    
    private float FetchWalkingInput()
    {
        _leftPressed = Input.IsActionPressed(Constants.ActionMoveLeft);
        _rightPressed = Input.IsActionPressed(Constants.ActionMoveRight);
        // 同时按下左 + 右时判定为停止
        if (_leftPressed && _rightPressed)
        {
            return 0;
        }
        return Input.GetAxis(Constants.ActionMoveLeft, Constants.ActionMoveRight);
    }

    private void ProcessBurst(float delta)
    {
        if (!GameRule.EnableMarioBursting)
        {
            _sprinting = false;
            _burstCharge = 0;
            return;
        }
        if (_running && !_turning && XSpeed >= MaxSpeedWhenRunning - 1e-3)
        {
            if (!_isInAir)
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