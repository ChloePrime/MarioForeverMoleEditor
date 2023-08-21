using Godot;

namespace ChloePrime.MarioForever.Util;

public static class MaFo
{
    public static class Groups
    {
        public static readonly StringName Player = "player";
    }

    public static class CollisionLayers
    {
        public const int Solid = 1;
        public const int SolidMarioOnly = 2;
        public const int SolidEnemyOnly = 3;
        public const int DamageSource = 9;
        public const int DeathSource = 10;
        public const int Mario = 17;
        public const int Enemy = 18;
    }
}