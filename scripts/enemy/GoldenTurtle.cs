using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class GoldenTurtle : Turtle
{
    public override void _Ready()
    {
        base._Ready();
        TurtleStateChanged += OnGoldenTurtleStateChanged;
        OnGoldenTurtleStateChanged();
    }

    private void OnGoldenTurtleStateChanged()
    {
        if (State == TurtleState.Jumping)
        {
            TargetSpeed = 0;
        }
        if (State != TurtleState.Flying && this.FindParentOfType<RotoDiscCore>() is {} xfx)
        {
            CallDeferred(MethodName.DropFromRotoDiscLater, xfx);
        }
    }

    private void DropFromRotoDiscLater(Node xfx)
    {
        var parent = ((Node)this.GetArea() ?? GetTree().Root) ?? xfx.GetParent();
        var pos = GlobalPosition;
        GetParent()?.RemoveChild(this);
            
        parent.AddChild(this);
        GlobalPosition = pos;
    }
}