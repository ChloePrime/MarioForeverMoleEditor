using Godot;

namespace ChloePrime.MarioForever.Player;

public partial class MarioHurtZone : MarioCollisionBySize
{
    [Export] public Mario Root { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Root ??= GetParent() as Mario;
    }
}