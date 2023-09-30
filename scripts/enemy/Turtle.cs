using System;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Shared;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class Turtle : WalkableNpc
{
    [Export] public float WalkSpeed { get; set; } = Units.Speed.CtfToGd(1);
    [Export] public float RollSpeed { get; set; } = Units.Speed.CtfToGd(5);
    [Export] public bool ShellTurnAtCliff { get; set; }

    public enum TurtleState
    {
        Flying,
        Jumping,
        Walking,
        StaticShell,
        MovingShell,
    }

    [Export]
    public TurtleState State
    {
        get => _state;
        set => SetState(value);
    }

    [Signal]
    public delegate void TurtleStateChangedEventHandler();

    public ComboTracker ComboTracker => 
        _tracker ??= GetNode<ComboTracker>(NpComboTracker);

    public TurtleFlyMovementComponent FlyMovement =>
        _flyMovement ??= GetNode<TurtleFlyMovementComponent>(NpFlyMovement);

    public void KickBy(Node2D kicker)
    {
        XDirection = -Math.Sign(ToLocal(kicker.GlobalPosition).X);
    }

    public override float AnimationDirection => State == TurtleState.Flying
        ? GetAnimDirectionFollow()
        : base.AnimationDirection;

    private float GetAnimDirectionFollow()
    {
        if (!IsInstanceValid(_mario)) _mario = null;
        _mario ??= GetTree()?.GetFirstNodeInGroup(MaFo.Groups.Player) as Node2D;
        if (!IsInstanceValid(_mario)) return base.AnimationDirection;
        return Math.Sign(ToLocal(_mario!.GlobalPosition).X);
    }
    
    public override bool WillHurtOthers => State is TurtleState.MovingShell || base.WillHurtOthers;

    public override void _Ready()
    {
        base._Ready();
        GrabReleased += OnGrabReleased;
        _ready = true;
        RefreshState();
    }

    public override void _PhysicsProcess(double deltaD)
    {
        if (State == TurtleState.Flying && FlyMovement is {} flyMovement)
        {
            flyMovement._ProcessMovement((float)deltaD);
        }
        else
        {
            base._PhysicsProcess(deltaD);
        }
    }

    protected override void _ProcessCollision(float delta)
    {
        base._ProcessCollision(delta);
        if (State is TurtleState.MovingShell && IsOnWall())
        {
            XSpeed = LastXSpeed;
        }
    }

    private void OnGrabReleased(Mario.GrabReleaseEvent e)
    {
        if (e.Flags.HasFlag(Mario.GrabReleaseFlags.TossHorizontally))
        {
            var xSpeed = XSpeed;
            State = TurtleState.MovingShell;
            XSpeed = xSpeed;
        }
    }

    private void SetState(TurtleState value)
    {
        if (_state == value) return;
        _state = value;

        if (_ready)
        {
            RefreshState();
        }
        EmitSignal(SignalName.TurtleStateChanged);
    }

    private void RefreshState()
    {
        var value = State;
        XSpeed = TargetSpeed = value switch
        {
            TurtleState.Jumping or TurtleState.Walking => WalkSpeed,
            TurtleState.MovingShell => RollSpeed,
            TurtleState.StaticShell or _ => 0,
        };

        // 让红乌龟的壳不要在悬崖处掉头
        var tacStand = _tacWhenStanding ??= TurnAtCliff;
        TurnAtCliff = value switch
        {
            TurtleState.Flying or TurtleState.Jumping => false,
            TurtleState.StaticShell => false,
            TurtleState.MovingShell => ShellTurnAtCliff,
            _ => tacStand,
        };
        
        // 重置连击计数器
        if (value != TurtleState.MovingShell)
        {
            ComboTracker.Reset();
        }

        // 切换跳跃高度
        if (value != TurtleState.Jumping)
        {
            _jumpStrengthBackup = JumpStrength;
            JumpStrength = 0;
        }
        else
        {
            if (JumpStrength == 0)
            {
                JumpStrength = _jumpStrengthBackup;
            }
        }

        var disableOtherCollision = value == TurtleState.MovingShell;
        if (disableOtherCollision)
        {
            if (!(this as IGrabbable).IsGrabbed)
            {
                _collideWithOthersRecovery = CollideWithOthers;
            }
            CollideWithOthers = false;
        }
        else if (_collideWithOthersRecovery is {} backup)
        {
            CollideWithOthers = backup;
            _collideWithOthersRecovery = null;
        }
    }

    protected override void _OnShotEnd()
    {
        base._OnShotEnd();
        if (State == TurtleState.MovingShell)
        {
            _collideWithOthersRecovery = CollideWithOthers;
            CollideWithOthers = false;
        }
        else if (_collideWithOthersRecovery is {} backup)
        {
            CollideWithOthers = backup;
            _collideWithOthersRecovery = null;
        }
    }

    private static readonly NodePath NpComboTracker = "Combo Tracker";
    private static readonly NodePath NpFlyMovement = "Fly Movement";

    private TurtleState _state = TurtleState.Walking;
    private bool? _tacWhenStanding;
    private bool? _collideWithOthersRecovery;
    private bool _ready;
    private float _jumpStrengthBackup;
    private TurtleFlyMovementComponent _flyMovement;
    private ComboTracker _tracker;
    private Node2D _mario;
}