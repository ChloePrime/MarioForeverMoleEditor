using Godot;

namespace ChloePrime.MarioForever.Level;

[GlobalClass]
public partial class LevelFluidPreset : Resource
{
    [Export] public PackedScene Fluid { get; set; }
}