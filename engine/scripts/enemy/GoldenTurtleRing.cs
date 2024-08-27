using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class GoldenTurtleRing : RotoDiscCore
{
    [Export] public PackedScene Prefab = GD.Load<PackedScene>("res://engine/objects/enemies/O_turtle_golden.tscn");
    [Export] public int Count { get; set; } = 6;
    [Export] public float Radius { get; set; } = 150;

    public override void _InitChildren()
    {
        for (int i = 0; i < Count; i++)
        {
            var turtle = Prefab.Instantiate<Node2D>();
            if (turtle is Turtle t) t.State = Turtle.TurtleState.Flying;

            var angle = Mathf.Tau * i / Count;
            var (sin, cos) = Mathf.SinCos(angle);
            turtle.Position = new Vector2(cos * Radius, -sin * Radius);
            
            AddChild(turtle);
        }
        base._InitChildren();
    }
}