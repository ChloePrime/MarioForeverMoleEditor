using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Level;

[GlobalClass]
public partial class ObjectTilePresetList : Resource
{
    [Export] public Array<ObjectTilePreset> Presets { get; private set; }
}