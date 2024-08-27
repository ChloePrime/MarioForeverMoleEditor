using Godot;

namespace ChloePrime.MarioForever.Enemy;

public abstract partial class TurtleFlyMovementComponent : Node
{
    public Turtle Turtle => _turtle ??= GetParent<Turtle>();
    
    public abstract void _ProcessMovement(float delta);
    
    
    private Turtle _turtle;
}