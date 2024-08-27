using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Facility;

[GlobalClass]
public partial class MovingPlatform : AnimatableBody2D, IMarioStandable
{
    /// <summary>
    /// 平台的移动速度。
    /// 默认生效，但开启 <see cref="MoveOnlyAfterBeStand"/> 后，在马里奥踩到平台上之前不会生效。
    /// </summary>
    [Export] public Vector2 Velocity { get; set; }
    
    /// <summary>
    /// 平台的重力（加速度），
    /// 需要开启 <see cref="AlwaysEnableGravity"/> 或者在开启 <see cref="FallOnlyAfterBeStand"/> 后被马里奥踩上才能生效。
    /// </summary>
    [Export] public Vector2 Gravity { get; set; } = new(0, Units.Acceleration.CtfToGd(0.2F));
    
    /// <summary>
    /// 踩上后才开始应用 <see cref="Velocity"/>
    /// </summary>
    [Export] public bool MoveOnlyAfterBeStand { get; set; }
    
    /// <summary>
    /// 踩上后才开始应用 <see cref="Gravity"/>，
    /// 无视 <see cref="AlwaysEnableGravity"/>。
    /// </summary>
    [Export] public bool FallOnlyAfterBeStand { get; set; }
    
    /// <summary>
    /// 让 <see cref="Gravity"/> 在平台没有被踩上时也生效
    /// </summary>
    [Export] public bool AlwaysEnableGravity { get; set; }

    public float XSpeed
    {
        get => Velocity.X;
        set => Velocity = Velocity with { X = value };
    }
    
    public float YSpeed
    {
        get => Velocity.Y;
        set => Velocity = Velocity with { Y = value };
    }

    public bool MovementEnabled => !MoveOnlyAfterBeStand || _everGotStand;
    
    public bool GravityEnabled => AlwaysEnableGravity || (FallOnlyAfterBeStand && _everGotStand);

    public CollisionShape2D CollisionShape => _collisionShape ??= GetNode<CollisionShape2D>(NpCollisionShape);

    public override void _Ready()
    {
        base._Ready();
        _lastGlobalPos = GlobalPosition;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (MovementEnabled)
        {
            Translate(Velocity * (float)delta);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (GravityEnabled)
        {
            Velocity += Gravity * (float)delta;
        }
        if (MovementEnabled)
        {
            if ((GlobalPosition - _lastGlobalPos).Abs() is { X: <= 8 } and { Y: <= 8 })
            {
                GlobalPosition = _lastGlobalPos;
            }
            Translate(Velocity * (float)delta);
            _lastGlobalPos = GlobalPosition;
        }
    }
    
    public void ProcessMarioStandOn(Node2D mario)
    {
        _everGotStand = true;
    }

    private static readonly NodePath NpCollisionShape = "Collision Shape";
    private CollisionShape2D _collisionShape;
    private Vector2 _lastGlobalPos;
    private bool _everGotStand;
}