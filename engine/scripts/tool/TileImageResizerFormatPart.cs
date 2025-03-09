using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Tool;

[GlobalClass]
public partial class TileImageResizerFormatPart : Resource
{
    [Export] private string Comment { get; set; }
    /// <summary>
    /// [src.x, src.y, dst.x, dst.y] in blocks
    /// </summary>
    [Export] public Array<Vector4I> RemapTable { get; private set; }
}