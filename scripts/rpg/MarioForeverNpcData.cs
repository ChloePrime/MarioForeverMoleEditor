using Godot;

namespace ChloePrime.MarioForever.RPG;

[GlobalClass]
public partial class MarioForeverNpcData : Resource
{
    public static MarioForeverNpcData SafeFallback => new()
    {
        Friendly = true,
    };

    [Export] public float HitPoint { get; set; } = 50;
    [Export] public float MaxHitPoint { get; set; } = 50;
    [Export] public float DamageLo { get; set; } = 2;
    [Export] public float DamageHi { get; set; } = 40;
    [Export] public bool Friendly { get; set; }
    [Export] public bool DoNotMove { get; set; }

    public void CopyValueFrom(MarioForeverNpcData other)
    {
        HitPoint = other.HitPoint;
        MaxHitPoint = other.MaxHitPoint ;
        DamageLo = other.DamageLo;
        DamageHi = other.DamageHi;
        Friendly = other.Friendly;
        DoNotMove = other.DoNotMove;
    }
}