using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
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
        if (!Enabled)
        {
            return;
        }
        foreach (var (key, status) in StatusTable)
        {
            if (Input.IsPhysicalKeyPressed(key))
            {
                GlobalData.Status = status;
                return;
            }
        }
        if (e is InputEventKey { PhysicalKeycode: Key.Key9 })
        {
            if (e.IsPressed() && Input.IsPhysicalKeyPressed(Key.Ctrl))
            {
                GlobalData.Lives = 999999;
                return;
            }
            if (e.IsPressed() || e.IsEcho())
            {
                GlobalData.Lives++;
            }
        }
        if (e is InputEventKey { PhysicalKeycode: Key.Q } && e.IsPressed())
        {
            var mole = GD.Load<PackedScene>("res://objects/enemies/O_mole_smb3.tscn").Instantiate<GravityObjectBase>();
            this.GetLevelManager()?.LevelInstance?.AddChild(mole);
            (GetTree().GetMario())?.Grab(mole);
        }
        if (e is InputEventKey { PhysicalKeycode: Key.H } && e.IsPressed())
        {
            var bar = ((LevelFrame)this.GetLevelManager()!).Hud.MegaManBossHpBar;
            bar.Visible = true;
            bar.Value = 0;
            bar.AddHpAnimated(32);
        }
    }
}