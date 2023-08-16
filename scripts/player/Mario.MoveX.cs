using System;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Player;

public partial class Mario
{
    /// <summary>
    /// X 速度的绝对值
    /// </summary>
    private float _xSpeed;

    /// <summary>
    /// X 方向，-1 或 1
    /// </summary>
    private int _xDirection = 1;

    private bool _leftPressed;
    private bool _rightPressed;
    private bool _runPressed;

    private float _walkAxis;
    private float _walkResult;
    [CtfFlag(0)] private bool _running;
    [CtfFlag(1)] private bool _walking;
    [CtfFlag(10)] private bool _turning;
    
    /// <summary>
    /// RE: 马里奥移动
    /// </summary>
    private void PhysicsProcessX(double deltaD)
    {
        var delta = (float)deltaD;
        _running = _runPressed;
        _walkAxis = FetchWalkingInput();
        _walkAxis = (_crouching && !_isInAir) ? 0 : (Mathf.IsZeroApprox(_walkAxis) ? 0 : _walkAxis);
        _walking = _walkAxis != 0;
        
        // RE: Flag10控制转向过程
        if (_xDirection * _walkAxis < 0 && _xSpeed > 0)
        {
            _turning = true;
        }
        if (_xDirection * _walkAxis > 0)
        {
            _turning = false;
        }
        
        // RE: 同时按住左和右会有问题，此处修正
        if (_leftPressed && _rightPressed && _xSpeed > MinSpeed)
        {
            var acc = _running ? AccelerationWhenRunning : AccelerationWhenWalking;
            _xSpeed = Math.Max(MinSpeed, _xSpeed - acc * delta);
        }
        
        // RE: 转向时先减速
        if (_turning)
        {
            if (_walking && _xSpeed > 0)
            {
                _xSpeed -= AccelerationWhenTurning * delta;
            }
            if (_xSpeed <= 0 && _xDirection * _walkAxis < 0)
            {
                _turning = false;
                _xSpeed = -1;
            }
        }
        if (_turning && _xSpeed <= 0 && _walkAxis == 0)
        {
            _turning = false;
            _xSpeed = 0;
        }

        // RE: 改变方向
        if (!_turning && !_completedLevel && _walking)
        {
            _xDirection = Math.Sign(_walkAxis);
        }
        
        // RE: 走/跑
        if (_walking && !_turning)
        {
            if (!_running)
            {
                if (_xSpeed < MaxSpeedWhenWalking)
                {
                    _xSpeed = Math.Min(MaxSpeedWhenWalking, _xSpeed + AccelerationWhenWalking * delta);
                }
                if (_xSpeed > MaxSpeedWhenWalking)
                {
                    _xSpeed = Math.Max(MaxSpeedWhenWalking, _xSpeed - AccelerationWhenWalking * delta);
                }
            }
            if (_running && _xSpeed < MaxSpeedWhenRunning)
            {
                _xSpeed = Math.Min(MaxSpeedWhenRunning, _xSpeed + AccelerationWhenRunning * delta);
            }
        }
        if (!_walking && _xSpeed > 0)
        {
            _xSpeed = Math.Max(0, _xSpeed - AccelerationWhenWalking * delta);
        }
        if (!_running && _xSpeed > MaxSpeedWhenWalking)
        {
            _xSpeed = Math.Max(MaxSpeedWhenWalking, _xSpeed - AccelerationWhenRunning * delta);
        }
        
        // RE: 初速度
        if (_walking && !_turning && _xSpeed < MinSpeed)
        {
            _xSpeed += MinSpeed;
        }
        if (_leftPressed && _rightPressed && _xSpeed < MinSpeed && !_crouching && !_completedLevel)
        {
            _xSpeed += MinSpeed;
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
            if (_xDirection < 0 && leftHitScreen)
            {
                pos.X = xLeftFrame + ScreenBorderPadding;
                Position = pos;
                _xSpeed = 0;
                return;
            }
            if (_xDirection > 0 && rightHitScreen)
            {
                pos.X = xRightFrame - ScreenBorderPadding;
                Position = pos;
                _xSpeed = 0;
                return;
            }
        }
            
        // 属于 Godot 的实际移动部分
        Velocity = new Vector2(_xSpeed * _xDirection, 0);
        var collided = MoveAndSlide();
        _walkResult = Velocity.X * delta;

        if (collided && _xSpeed > 0 && Mathf.IsZeroApprox(Math.Abs(Velocity.X)))
        {
            _xSpeed = 0;
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
}