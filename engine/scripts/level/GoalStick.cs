using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Util;
using ChloePrime.MarioForever.Util.HelperNodes;
using Godot;
using static Godot.GD;
using static Godot.Mathf;

namespace ChloePrime.MarioForever.Level;

public partial class GoalStick : Area2D
{
    [Export] public Vector2 Velocity;
    [Export] public Vector2 MGravity = new(0, Units.Acceleration.CtfToGd(0.2F));
    [Export] public float LaunchSpeed = Units.Speed.CtfMovementToGd(40);
    
    public bool IsActivated
    {
        get => _rotator.ProcessMode != ProcessModeEnum.Disabled;
        set => _rotator.ProcessMode = value ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
    }
    
    [Signal]
    public delegate void ActivatedEventHandler();
    
    
    private static readonly NodePath NpRotator = "Sprite/Rotator";
    private Rotator2D _rotator;
    
    
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _rotator, NpRotator);

        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (IsActivated || Goal.HasCompletedLevel(body)) return;
        
        Callable.From(() => Reparent(this.GetPreferredRoot())).CallDeferred();
        Activate();
    }

    public void Activate()
    {
        if (IsActivated) return;
        IsActivated = true;

        Velocity = LaunchSpeed * Vector2.FromAngle(DegToRad((float)RandRange(225 - 11.25, 225 + 11.25)));
        EmitSignal(SignalName.Activated);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (IsActivated)
        {
            Position += Velocity * (float)delta;
            Velocity += MGravity * (float)delta;
        }
    }
}