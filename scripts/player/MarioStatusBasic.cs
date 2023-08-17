using Godot;

namespace ChloePrime.MarioForever.Player;

public partial class MarioStatusBasic : MarioStatus
{
    [Export] public StringName Id { get; private set; }
    public override StringName GetId() => Id;
}