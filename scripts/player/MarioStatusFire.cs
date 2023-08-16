using Godot;

namespace ChloePrime.MarioForever.Player;

public partial class MarioStatusFire : MarioStatus
{
    public static readonly StringName FireId = "fire";
    public override StringName GetId() => FireId;
    public override MarioSize GetSize() => MarioSize.Big;
}