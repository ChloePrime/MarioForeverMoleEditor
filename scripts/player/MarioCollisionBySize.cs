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
            ShapeBySize[i].CallDeferred(CollisionShape2D.MethodName.SetDisabled, i != (int)size);
        }
    }

    public override void _Ready()
    {
        base._Ready();
        _mario = GetParent<Mario>();
    }

    private Mario _mario;
}