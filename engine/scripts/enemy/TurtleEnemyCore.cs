using ChloePrime.MarioForever.Shared;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class TurtleEnemyCore : EnemyCore
{
    public Turtle Turtle => _turtle ??= GetParent<Turtle>();

    public override ComboTracker GetComboTracker()
    {
        return Turtle.State == Turtle.TurtleState.MovingShell 
            ? Turtle.ComboTracker
            : base.GetComboTracker();
    }

    public override void _Ready()
    {
        base._Ready();
        Turtle.TurtleStateChanged += OnTurtleStateChanged;
        OnTurtleStateChanged();
    }

    private void OnTurtleStateChanged()
    {
        DieWhenThrownAndHitOther = _turtle.State is not Turtle.TurtleState.MovingShell;
        
        if (Sprite is { } sprite)
        {
            sprite.Animation = Turtle.State.GetAnimation();
        }
    }

    private Turtle _turtle;
}

file static class TurtleStateEx
{
    public static StringName GetAnimation(this Turtle.TurtleState state)
    {
        return AnimationNames[(int)state];
    }

    private static readonly StringName[] AnimationNames =
    {
        "1_flying", "1_flying", "0_walking", "3_shell_static", "4_shell_moving"
    };
}