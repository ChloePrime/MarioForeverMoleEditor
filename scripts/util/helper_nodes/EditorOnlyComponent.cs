using Godot;

namespace ChloePrime.MarioForever.Util.HelperNodes;

[GlobalClass]
public partial class EditorOnlyComponent : Node
{
    public override void _Ready()
    {
        base._Ready();
        GetParent()?.QueueFree();
    }
}