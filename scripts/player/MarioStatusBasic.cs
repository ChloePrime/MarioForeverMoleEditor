using Godot;

namespace ChloePrime.MarioForever.Player;

public partial class MarioStatusBasic : MarioStatus
{
    [Export] public StringName Id { get; private set; }
    [Export] public MarioSize Size { get; private set; } = MarioSize.Big;
    public override StringName GetId() => Id;
    public override MarioSize GetSize() => Size;
}