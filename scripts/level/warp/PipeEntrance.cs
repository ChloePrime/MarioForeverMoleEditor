using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Level.Warp;

public partial class PipeEntrance : WarpObject
{
    [Export] public PassDirection Direction { get; private set; }
    [Export] public AudioStream EnterSound { get; set; } = GD.Load<AudioStream>("res://resources/shared/SE_pipe.ogg");

    public const float EnteringSpeed = 100;
    public const float EnterDistanceH = 28;
    public const float EnterDistanceV = 64;
    public const float ShrinkSpeed = 100;
    public const float ShrinkMin = 0F;
    public const float MaxShrink = 16;
    
    public enum PassDirection
    {
        Down,
        Up,
        Left,
        Right,
        Max,
    }
    
    public enum Phase
    {
        Deactivated,
        Positioning,
        Entering,
        Exiting,
    }
    
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _editor, NpEditorDisplay);
        this.GetNodeOrNull(out _waitTimer, NpEnterWaitTimer);
        this.GetNodeOrNull(out _hitbox, NpHitbox);
        _editor.Visible = false;
        if (_hitbox is { } hitbox)
        {
            hitbox.BodyEntered += OnHitboxBodyEntered;
            hitbox.BodyExited += OnHitboxBodyExited;
        }
        if (_waitTimer is { } timer)
        {
            timer.Timeout += MarioCompleteEnteringPipe;
        }
    }

    public override void _PhysicsProcess(double deltaD)
    {
        base._PhysicsProcess(deltaD);
        var delta = (float)deltaD;
        
        TryEnterPipe();
        if (_phase is not Phase.Deactivated)
        {
            EnteringPipe(delta);
        }
    }

    private void TryEnterPipe()
    {
        if (_mario is not { PipeState: MarioPipeState.NotInPipe } mario || !IsInstanceValid(mario)) return;
        var willEnter = Direction switch
        {
            PassDirection.Up => Input.IsActionPressed(Mario.Constants.ActionMoveUp),
            PassDirection.Down => Input.IsActionPressed(Mario.Constants.ActionMoveDown),
            PassDirection.Left => Input.IsActionPressed(Mario.Constants.ActionMoveLeft),
            PassDirection.Right => Input.IsActionPressed(Mario.Constants.ActionMoveRight),
            _ => false,
        };
        if (!willEnter) return;
        MarioEnterPipe(mario);
    }

    private void MarioEnterPipe(Mario mario)
    {
        mario.PipeState = MarioPipeState.Entering;
        mario.ZIndexBeforePipe = mario.ZIndex;
        mario.ZIndex = -1;
        _phase = Phase.Positioning;
        mario.PipeForceAnimation = Direction switch
        {
            PassDirection.Up => mario.IsGrabbing ? Mario.Constants.AnimGrabJump : Mario.Constants.AnimJumping,
            PassDirection.Down => mario.IsGrabbing ? null : Mario.Constants.AnimCrouching,
            _ => Mario.Constants.AnimWalking,
        };
        if (Direction != PassDirection.Down)
        {
            mario.ForceCancelCrouch();
        }
    }

    private void EnteringPipe(float delta)
    {
        if (_mario is not {} mario || !IsInstanceValid(mario)) return;
        switch (_phase)
        {
            case Phase.Positioning when Direction is PassDirection.Up or PassDirection.Down:
            {
                var relDeltaX = ToLocal(mario.GlobalPosition).X;
                if (Mathf.IsZeroApprox(relDeltaX))
                {
                    goto case Phase.Positioning;
                }
                var movement = relDeltaX.MoveToward(0, EnteringSpeed * delta);
                mario.GlobalPosition += new Vector2(movement, 0).Rotated(GlobalRotation);
                return;
            }
            case Phase.Positioning when Direction is PassDirection.Left or PassDirection.Right:
                mario.GlobalPosition = ToGlobal(ToLocal(mario.GlobalPosition) with { Y = 0 });
                goto case Phase.Positioning;
            case Phase.Positioning:
                EnterSound?.Play();
                _phase = Phase.Entering;
                _distanceLeft = Direction.GetAxis() == Vector2.Axis.X ? EnterDistanceH : EnterDistanceV;
                _shrink = MaxShrink;
                if (_waitTimer is { } timer && timer.IsStopped())
                {
                    timer.Start();
                }
                break;
            case Phase.Entering when Direction is PassDirection.Left or PassDirection.Right:
                mario.GlobalPosition = ToGlobal(ToLocal(mario.GlobalPosition) with { Y = 0 });
                goto case Phase.Entering;
            case Phase.Entering:
            {
                if (Mathf.IsZeroApprox(_distanceLeft))
                {
                    break;
                }
                var mov = _distanceLeft.MoveToward(0, EnteringSpeed * delta);
                mario.GlobalPosition += (Direction.GetNormal() * new Vector2(-mov, -mov)).Rotated(GlobalRotation);
                _shrink.MoveToward(0, ShrinkSpeed * delta);
                mario.PipeGrabbedObjectXOffsetShrink = Mathf.Lerp(ShrinkMin, 1F, _shrink / MaxShrink);
                break;
            }
            case Phase.Exiting:
            {
                var mov = _distanceLeft.MoveToward(0, EnteringSpeed * delta);
                mario.GlobalPosition += (Direction.GetNormal() * new Vector2(-mov, -mov)).Rotated(GlobalRotation);
                var shrink = Mathf.Clamp(_distanceLeft / 16F, 0, 1);
                mario.PipeGrabbedObjectXOffsetShrink = Mathf.Lerp(1F, ShrinkMin, shrink);
                if (Mathf.IsZeroApprox(_distanceLeft))
                {
                    MarioExitPipe(mario);
                    _phase = Phase.Deactivated;
                    _mario = null;
                }
                break;
            }
        }
    }
    
    private void MarioCompleteEnteringPipe()
    {
        if (_mario is not {} mario) return;
        mario.RequireTeleport += () => OnMarioRequireTeleport(mario);
        mario.PipeState = MarioPipeState.TransitionBegin;
        _phase = Phase.Deactivated;
    }

    private void MarioExitPipe(Mario mario)
    {
        mario.PipeState = MarioPipeState.NotInPipe;
        mario.XSpeed = mario.YSpeed = 0;
        mario.PipeForceAnimation = null;
        mario.ZIndex = mario.ZIndexBeforePipe;
        // 防止因为误差导致马里奥传送后被判定为卡在地面里
        mario.GlobalPosition -= new Vector2(0, 1.5F * mario.SafeMargin);
        if (Direction != PassDirection.Down)
        {
            mario.ForceCancelCrouch();
        }
    }

    private void ProcessMoving(Mario mario, float delta, MarioPipeState pipeStateAfter)
    {
        var mov = _distanceLeft.MoveToward(0, EnteringSpeed * delta);
        mario.GlobalPosition += (Direction.GetNormal() * new Vector2(-mov, -mov)).Rotated(GlobalRotation);
        if (Mathf.IsZeroApprox(_distanceLeft))
        {
            mario.RequireTeleport += () => OnMarioRequireTeleport(mario);
            mario.PipeState = pipeStateAfter;
            _phase = Phase.Deactivated;
        }
    }

    protected override void _OnMarioArrived(Mario mario)
    {
        base._OnMarioArrived(mario);
        EnterSound?.Play();
        _mario = mario;
        _phase = Phase.Exiting;
        
        if (Direction != PassDirection.Up)
        {
            mario.ForceCancelCrouch();
        }
        _distanceLeft = Direction.GetAxis() == Vector2.Axis.X 
            ? mario.CurrentSize.GetIdealWidth() 
            : mario.CurrentSize.GetIdealHeight() - (Direction == PassDirection.Down ? 4 : 0);
        _shrink = MaxShrink;
        mario.PipeForceAnimation = Direction switch
        {
            PassDirection.Down or PassDirection.Up => mario.IsGrabbing
                ? Mario.Constants.AnimGrabJump
                : Mario.Constants.AnimJumping,
            _ => Mario.Constants.AnimWalking,
        };
    }

    private void OnHitboxBodyEntered(Node2D body)
    {
        if (body is Mario m) _mario = m;
    }
    
    private void OnHitboxBodyExited(Node2D body)
    {
        if (body == _mario && _phase == Phase.Deactivated) _mario = null;
    }

    private void OnMarioRequireTeleport(Mario mario)
    {
        _mario = null;
        if (Target is not { } target
            || this.GetArea() is not {} oldArea
            || target.GetArea() is not { } newArea)
        {
            mario.GlobalPosition = GlobalPosition;
            MarioExitPipe(mario);
            return;
        }
        if (oldArea != newArea)
        {
            mario.GetParent()?.RemoveChild(mario);
            this.GetLevel()!.SetArea(newArea);
            newArea.AddChild(mario);
        }
        mario.GlobalPosition = target.GlobalPosition;
        target.PrepareMarioExit(mario);
    }

    private Mario _mario;
    private static readonly NodePath NpHitbox = "Hitbox";
    private static readonly NodePath NpEditorDisplay = "Editor Display";
    private static readonly NodePath NpEnterWaitTimer = "Enter Wait Timer";
    private Area2D _hitbox;
    private Sprite2D _editor;
    private Timer _waitTimer;
    private Phase _phase = Phase.Deactivated;
    private float _shrink;
    private float _distanceLeft;
}

public static class PipeEntrancePassDirectionEx
{
    public static Vector2 GetNormal(this PipeEntrance.PassDirection direction)
    {
        return NormalTable[(int)direction];
    }
    
    public static Vector2.Axis GetAxis(this PipeEntrance.PassDirection direction)
    {
        return direction switch
        {
            PipeEntrance.PassDirection.Down or PipeEntrance.PassDirection.Up => Vector2.Axis.Y,
            PipeEntrance.PassDirection.Left or PipeEntrance.PassDirection.Right => Vector2.Axis.X,
            _ => Vector2.Axis.Y,
        };
    }

    private static readonly Vector2[] NormalTable = { Vector2.Down, Vector2.Up, Vector2.Left, Vector2.Right };
}