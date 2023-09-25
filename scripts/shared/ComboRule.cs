using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Shared;

[GlobalClass]
public partial class ComboRule : Resource
{
    [Export] public Array<PackedScene> ScoreList { get; private set; }
    [Export] public Array<AudioStream> SoundList { get; private set; }
    [Export] public bool ResetAtLast { get; set; }
}