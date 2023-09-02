using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Level;

[GlobalClass]
public partial class ObjectTilePreset : Resource
{
    [Export(PropertyHint.NodeType)] public Script BaseClass { get; private set; }
    [Export] public Array<Dictionary<StringName, Variant>> PropertiesById { get; private set; }
}