using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Facility;

public partial class SpringCollider : Area2D, IStompable
{
    [Export] public float JumpStrengthLo { get; set; } = Units.Speed.CtfToGd(10);
    [Export] public float JumpStrengthHi { get; set; } = Units.Speed.CtfToGd(19);
    
    [Signal] public delegate void SpringStompedEventHandler();
    
    public Vector2 StompCenter => GlobalPosition;
    
    public void StompBy(Node2D stomper)
    {
        switch (stomper)
        {
            case Mario mario:
                mario.Jump(Input.IsActionPressed(Mario.Constants.ActionJump) ? JumpStrengthHi : JumpStrengthLo);
                break;
            case GravityObjectBase gob:
                gob.YSpeed = -JumpStrengthLo;
                break;
            default:
                return;
        }
        
        EmitSignal(SignalName.SpringStomped);
    }
}