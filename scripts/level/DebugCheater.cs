using ChloePrime.MarioForever.Player;
using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Level;

[GlobalClass]
public partial class DebugCheater : Node
{
    [Export] public bool Enabled { get; set; }
    
    private static readonly Dictionary<Key, MarioStatus> StatusTable = new()
    {
        { Key.Key1, MarioStatus.Small },
        { Key.Key2, MarioStatus.Big },
        { Key.Key3, MarioStatus.FireFlower },
        { Key.Quoteleft, GD.Load<MarioStatus>("res://resources/mario/status_chloe_3d.tres") },
    };

    public override void _Ready()
    {
        base._Ready();
#if TOOLS
        Enabled = true;
#endif
    }

    public override void _Input(InputEvent e)
    {
        base._Input(e);
        foreach (var (key, status) in StatusTable)
        {
            if (Input.IsPhysicalKeyPressed(key))
            {
                GlobalData.Status = status;
                return;
            }
        }
    }
}