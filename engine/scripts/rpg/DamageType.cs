using System;
using System.Linq;
using System.Numerics;

namespace ChloePrime.MarioForever.RPG;

[Flags]
public enum DamageType : uint
{
    Stomp       = 1 << 0,
    Fireball    = 1 << 1,
    Beetroot    = 1 << 2,
    Environment = 1 << 3,
    KickShell   = 1 << 4,
    Bump        = 1 << 5,
    Star        = 1 << 6,
    Enemy       = 1 << 7,
    Poison      = 1 << 8,
    
    Armored     = Environment | Bump | Star | KickShell,
    Unarmored   = Armored | Fireball | Beetroot,
}

public static class DamageTypeEx
{
    public static ReadOnlySpan<DamageType> Values => ValuesPrivate;
    
    public static bool ContainsAny(this DamageType types, DamageType predicate)
    {
        return (types & predicate) != 0;
    }
    
    public static bool ContainsAny(this uint types, DamageType predicate)
    {
        return ((DamageType)types).ContainsAny(predicate);
    }
    
    public static bool ContainsAll(this DamageType types, DamageType predicate)
    {
        return (types & predicate) == predicate;
    }
    
    public static bool ContainsAll(this uint types, DamageType predicate)
    {
        return ((DamageType)types).ContainsAll(predicate);
    }
    
    private static readonly DamageType[] ValuesPrivate = Enum.GetValues<DamageType>()
        .Where(dt => BitOperations.IsPow2((int)dt))
        .ToArray();
}