using System;
using System.Linq;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Player;

public enum MarioClimbStatus
{
    NotClimbing,
    VerticalOnly,
    TwoDof,
}

public partial class Mario
{
    public MarioClimbStatus ClimbStatus { get; private set; }
    public bool IsClimbing => ClimbStatus is not MarioClimbStatus.NotClimbing;

    private void ClimbingReady()
    {
        this.GetNode(out _climbDetectorVert, NpClimbDetectorVert);
        this.GetNode(out _climbDetector2Dof, NpClimbDetector2Dof);
    }

    private void ProcessClimbDetection(float delta)
    {
        if (_climbCdAfterJump > 0)
        {
            _climbCdAfterJump -= delta;
            return;
        }
        
        if (IsClimbing || PipeState != MarioPipeState.NotInPipe) return;
        if (ControlIgnored || !Input.IsActionPressed(Constants.ActionMoveUp)) return;
        
        if (_climbDetectorVert.HasOverlappingBodies())
        {
            ClimbStatus = MarioClimbStatus.VerticalOnly;
            StickToVerticalClimbable();
        }
        else if(_climbDetector2Dof.HasOverlappingBodies())
        {
            ClimbStatus = MarioClimbStatus.TwoDof;
        }
        else
        {
            return;
        }

        XSpeed = YSpeed = 0;
    }

    private void StickToVerticalClimbable()
    {
        if (_climbDetectorVert.GetOverlappingBodies().FirstOrDefault() is not { } ladder)
        {
            return;
        }
        if (ladder is CollisionObject2D ladder2d)
        {
            GlobalPosition = GlobalPosition with { X = ladder2d.GlobalPosition.X };
        }
        else if (ladder is TileMapLayer tilemap)
        {
            var tileSize = tilemap.TileSet.TileSize;
            var phState = GetWorld2D().DirectSpaceState;
            var testShape = _climbDetectorVert.CurrentShape;

            var dtx = 0;
            foreach (var dx in (ReadOnlySpan<int>)[0, -tileSize.X, tileSize.X])
            {
                LadderTestParams.Transform = testShape.GlobalTransform with { Origin = testShape.ToGlobal(new Vector2(dx, 0)) };
                if (phState.IntersectShape(LadderTestParams, 1).Count != 0)
                {
                    dtx = Math.Sign(dx);
                    break;
                }
            }
            var tmLocalPos = tilemap.ToLocal(GlobalPosition);
            GlobalPosition = tilemap.ToGlobal(new Vector2(
                (Mathf.Floor((tmLocalPos.X + 0.001F) / tileSize.X) + dtx + 0.5F) * tileSize.X,
                tmLocalPos.Y)
            );
        }
    }

    private static readonly PhysicsShapeQueryParameters2D LadderTestParams = new()
    {
        Shape = new RectangleShape2D
        {
            Size = new Vector2(0.125F, 32),
        },
        CollideWithAreas = false,
        CollideWithBodies = true,
        CollisionMask = MaFo.CollisionMask.ClimbableVert,
    };

    private void ProcessClimb(float delta)
    {
        if (MotionMode is MotionModeEnum.Grounded)
        {
            MotionMode = MotionModeEnum.Floating;
        }

        var detector = ClimbStatus is MarioClimbStatus.TwoDof ? _climbDetector2Dof : _climbDetectorVert;
        ClimbStopTestParams.CollisionMask = detector.CollisionMask;
        if (!detector.CurrentShape.IntersectTyped(ClimbStopTestParams).Any())
        {
            ClimbStatus = MarioClimbStatus.NotClimbing;
            return;
        }
        
        XSpeed = YSpeed = 0;
        
        if (!ControlIgnored)
        {
            var xInput = Input.GetAxis(Constants.ActionMoveLeft, Constants.ActionMoveRight);
            if (xInput != 0)
            {
                XDirection = _controlDirection = Math.Sign(xInput);
            }
            
            var x = ClimbStatus is MarioClimbStatus.TwoDof ? xInput : 0;
            var y = Input.GetAxis(Constants.ActionMoveUp, Constants.ActionMoveDown);
            if (Mathf.IsZeroApprox(x) && Mathf.IsZeroApprox(y))
            {
                _isClimbMoving = false;
            }
            else
            {
                ClimbStopTestParams.CollisionMask = detector.CollisionMask;
                ClimbStopTestParams.Shape = detector.CurrentShape.Shape;
                bool canMoveX, canMoveY;
                if (x == 0)
                {
                    canMoveX = false;
                }
                else
                {
                    var delta2 = Math.Max(delta, 1 / 50F);
                    ClimbStopTestParams.Transform = detector.CurrentShape.GlobalTransform with
                    {
                        Origin = detector.CurrentShape.ToGlobal(new Vector2(Mathf.Sign(x) * ClimbSpeed * delta2, 0)),
                    };
                    canMoveX = GetWorld2D().DirectSpaceState.IntersectShape(ClimbStopTestParams, 1).Count != 0;
                }
                if (y == 0)
                {
                    canMoveY = false;
                }
                else
                {
                    ClimbStopTestParams.Transform = detector.CurrentShape.GlobalTransform with
                    {
                        Origin = detector.CurrentShape.ToGlobal(new Vector2(0, Mathf.Sign(y) * ClimbSpeed / 50F)),
                    };
                    canMoveY = GetWorld2D().DirectSpaceState.IntersectShape(ClimbStopTestParams, 1).Count != 0;
                }

                if (canMoveX || canMoveY)
                {
                    Velocity = new Vector2(canMoveX ? x : 0, canMoveY ? y : 0).Normalized() * ClimbSpeed;
                    MoveAndSlide();
                    _isClimbMoving = !Velocity.IsZeroApprox();
                }
                else
                {
                    _isClimbMoving = false;
                }
            }
        }
        else
        {
            _isClimbMoving = false;
        }
    }

    private static readonly PhysicsShapeQueryParameters2D ClimbStopTestParams = new()
    {
        CollideWithAreas = false,
        CollideWithBodies = true,
    };

    private static readonly NodePath NpClimbDetectorVert = "Climb Detector (Vert)";
    private static readonly NodePath NpClimbDetector2Dof = "Climb Detector (2DOF)";
    private MarioCollisionBySize _climbDetectorVert;
    private MarioCollisionBySize _climbDetector2Dof;
    private bool _isClimbMoving;
    private float _climbCdAfterJump;
}