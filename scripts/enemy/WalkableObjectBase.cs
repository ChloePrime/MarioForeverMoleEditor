using System;
using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

[CtfGroup("1")]
[GlobalClass]
public partial class WalkableObjectBase : GravityObjectBase
{
    [Export] public bool CollideWithOthers { get; set; } = true;
    
    public override void _Ready()
    {
        base._Ready();
        if (GetTree().GetFirstNodeInGroup(MaFo.Groups.Player) is not Node2D mario)
        {
            return;
        }
        var relPos = mario.Position.X - Position.X;
        if (relPos != 0)
        {
            XDirection = Math.Sign(relPos);
        }
    }

    public override void _ProcessCollision()
    {
        foreach (var collision in this.GetSlideCollisions())
        {
            if (collision.GetCollider() is not WalkableObjectBase { CollideWithOthers: true } other)
            {
                continue;
            }
            var relativeX = other.Position.X - Position.X;
            SetDirectionOnCollide(relativeX);
            other.SetDirectionOnCollide(-relativeX);
        }
    }

    private void SetDirectionOnCollide(float relativeX)
    {
        if (relativeX != 0)
        {
            XDirection = Math.Sign(relativeX);
        }
    }
}