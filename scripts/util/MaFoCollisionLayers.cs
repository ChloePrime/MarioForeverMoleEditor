namespace ChloePrime.MarioForever.Util;

public static class MaFoCollisionLayers
{
    public const int Solid = 1;
    public const int SolidMarioOnly = 2;
    public const int SolidEnemyOnly = 4;
    public const int DamageSource = 1 << 8;
    public const int DeathSource = 1 << 9;
    public const int Mario = 1 << 16;
    public const int Enemy = 1 << 17;
}