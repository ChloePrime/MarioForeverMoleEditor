using Godot;

namespace ChloePrime.MarioForever.RPG;

[GlobalClass]
public partial class MarioForeverNpcData : Resource, IMarioForeverNpc
{
    public static readonly MarioForeverNpcData SafeFallback = new()
    {
        Friendly = true,
    };
    
    [Export] public float DamageLo { get; set; } = 2;
    [Export] public float DamageHi { get; set; } = 40;
    [Export] public bool Friendly { get; set; }
    [Export] public bool DoNotMove { get; set; }
    public MarioForeverNpcData NpcData => this;
}