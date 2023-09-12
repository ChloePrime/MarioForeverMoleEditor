using System;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class HorizontalHomingNpc : WalkableNpc, IDynamicAnimationSpeedEnemy
{
    [Export] public float MaxSpeed = Units.Speed.CtfToGd(1.5F);
    [Export] public bool ScaleAnimationSpeed { get; set; } = true; 
    [Export] public Node2D Target { get; set; }

    public float AnimationSpeedScale => ScaleAnimationSpeed ? XSpeed / MaxSpeed : 1;

    public override float AnimationDirection
    {
        get
        {
            if (XSpeed >= 2 * MaxSpeed ||
                Target is not { } target || 
                !IsInstanceValid(target))
            {
                return base.AnimationDirection;
            }
            return Math.Sign(ToLocal(target.GlobalPosition).X);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (CanMove)
        {
            Target ??= GetTree().GetFirstNodeInGroup(MaFo.Groups.Player) as Node2D;
            if (Target is not { } target || !IsInstanceValid(target))
            {
                Target = null;
            }
            else
            {
                var sameDir = Mathf.IsEqualApprox(Math.Sign(ToLocal(target.GlobalPosition).X), XDirection);
                if (sameDir)
                {
                    TargetSpeed = MaxSpeed;
                }
                else
                {
                    TargetSpeed = 0;
                    if (XSpeed <= 0)
                    {
                        XDirection *= -1;
                        TargetSpeed = MaxSpeed;
                    }
                }
            }
        }
        base._PhysicsProcess(delta);
    }
}