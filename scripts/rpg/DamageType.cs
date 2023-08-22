using System;

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
}

public static class DamageTypeEx
{
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
}