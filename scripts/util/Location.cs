using Godot;

namespace ChloePrime.MarioForever.Util;

public partial class Location : RefCounted
{
    [Export] public PackedScene Level { get; set; }
    [Export] public int Area { get; set; }
    [Export] public Vector2 Position { get; set; }
}