using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Player;

[GlobalClass]
public partial class MarioCollisionBySize: Area2D
{
    [Export] public Array<CollisionShape2D> ShapeBySize { get; private set; }

    public void SetSize(MarioSize size)
    {
        for (var i = 0; i < ShapeBySize.Count; i++)
        {
            var shape = ShapeBySize[i];
            Callable.From((bool disabled) => shape.Disabled = disabled).CallDeferred(i != (int)size);
        }
    }

    public override void _Ready()
    {
        base._Ready();
        _mario = GetParent<Mario>();
        _mario.SizeChanged += () => SetSize(_mario.CurrentSize);
    }

    private Mario _mario;
}