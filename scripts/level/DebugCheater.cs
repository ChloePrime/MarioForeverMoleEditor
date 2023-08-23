using ChloePrime.MarioForever.Player;
using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Level;

[GlobalClass]
public partial class DebugCheater : Node
{
#if TOOLS
    public static readonly Dictionary<Key, MarioStatus> StatusTable = new()
    {
        { Key.Key1, MarioStatus.Small },
        { Key.Key2, MarioStatus.Big },
        { Key.Key3, MarioStatus.FireFlower },
    };

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
#endif
}