using System.Linq;
using ChloePrime.MarioForever.Level;
using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
[Icon("res://resources/enemies/AT_clamp_icon.tres")]
public partial class ClampFlower : Node2D, ICustomTileOffsetObject
{
    public const float ForceShyShyDistance = 80;
    [Export] public Vector2 MoveDirection { get; private set; } = Vector2.Down;
    [Export] public float MoveDistance { get; private set; } = 56;
    [Export] public float MoveSpeed { get; set; } = Units.Speed.CtfToGd(2);
    [Export] public float WaitTimeUp { get; set; } = 0.5F;
    [Export] public float WaitTimeDown { get; set; } = 1.4F;
    [Export] public float ShyDetectDistance { get; set; } = 80;

    [Signal] public delegate void ClampMovedToTopEventHandler();
    [Signal] public delegate void ClampMovedToBottomEventHandler();
    
    public VisibleOnScreenNotifier2D VisibleOnScreenNotifier => _vosn;
    
    public void ShrinkAndForceShy()
    {
        if (!_moving)
        {
            SwapMovement();
        }
        _forceShy = true;
        _rev = false;
        Move(MoveDistance);
    }

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _waitTimer, NpWaitTimer);
        this.GetNode(out _vosn, NpVosn);
        _waitTimer.Timeout += OnWaitTimerTimeout;
        _rule = this.GetRule();
    }

    public override void _Process(double deltaD)
    {
        base._Process(deltaD);
        var delta = (float)deltaD;
        if (_moving)
        {
            Move(MoveSpeed * delta);
        }
        else if (_waitingForMarioToLeave)
        {
            if (_rule.W10EClampOffScreenPolicy && _rev && !_vosn.IsOnScreen())
            {
                return;
            }
            var shyDistance = _forceShy ? ForceShyShyDistance : ShyDetectDistance;
            bool isGrowBlocked;
            if (shyDistance != 0)
            {
                isGrowBlocked = !_rev && GetTree().GetNodesInGroup(MaFo.Groups.Player)
                    .OfType<Node2D>()
                    .Select(mario => ToLocal(mario.GlobalPosition))
                    .Any(rp => Mathf.Abs(rp.X) < shyDistance);
            }
            else
            {
                isGrowBlocked = false;
            }
            if (!isGrowBlocked)
            {
                SwapMovement();
                _forceShy = false;
            }
        }
    }
    

    private void Move(float distance)
    {
        if (!_moving) return;
        
        var end = _rev ? 0 : MoveDistance;
        var offset = _moved.MoveToward(end, distance);
        Translate(MoveDirection * offset);
        if (Mathf.IsEqualApprox(_moved, end))
        {
            EmitMoveEndSignal();
            _waitTimer.WaitTime = _rev ? WaitTimeUp : WaitTimeDown;
            _waitTimer.Start();
            _moving = false;
        }
    }

    private void EmitMoveEndSignal()
    {
        EmitSignal(_rev ? SignalName.ClampMovedToTop : SignalName.ClampMovedToBottom);
    }

    private void OnWaitTimerTimeout()
    {
        _waitingForMarioToLeave = true;
    }

    private void SwapMovement()
    {
        _rev = !_rev;
        _moving = true;
        _waitingForMarioToLeave = false;
    }

    private static readonly NodePath NpWaitTimer = "Wait Timer";
    private static readonly NodePath NpVosn = "VisibleOnScreenNotifier";
    private Timer _waitTimer;
    private VisibleOnScreenNotifier2D _vosn;
    private GameRule _rule;
    private bool _rev;
    private bool _moving = true;
    private float _moved;
    private bool _waitingForMarioToLeave;
    private bool _forceShy;
    
    // ICustomTileOffsetObject implementation
    
    public void CustomOffset()
    {
        Translate(BasicOffset + DirectionalOffset.Rotated(Rotation));
    }

    private static readonly Vector2 BasicOffset = new(16, -16);
    private static readonly Vector2 DirectionalOffset = new(0, 32);
}