using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Tool;

[GlobalClass]
public partial class TileImageResizerFormat : Resource
{
    [Export] public Array<TileImageResizerFormatPart> Parts { get; private set; }

    public IEnumerable<Vector4I> Entries => Parts.SelectMany(part => part.RemapTable);
}