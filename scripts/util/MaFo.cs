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
        public const int Solid          = 1;
        public const int SolidMarioOnly = 2;
        public const int SolidEnemyOnly = 3;
        public const int HiddenBonus    = 4;
        public const int Water          = 5;
        public const int ClimbableVert  = 6;
        public const int Climbable2Dof  = 7;
        public const int DamageSource   = 9;
        public const int DeathSource    = 10;
        public const int Mario          = 17;
        public const int Enemy          = 18;
    }
    
    public static class CollisionMask
    {
        public const uint Solid          = 1U << (CollisionLayers.Solid - 1);
        public const uint SolidMarioOnly = 1U << (CollisionLayers.SolidMarioOnly - 1);
        public const uint SolidEnemyOnly = 1U << (CollisionLayers.SolidEnemyOnly - 1);
        public const uint HiddenBonus    = 1U << (CollisionLayers.HiddenBonus - 1);
        public const uint Water          = 1U << (CollisionLayers.Water - 1);
        public const uint ClimbableVert  = 1U << (CollisionLayers.ClimbableVert - 1);
        public const uint Climbable2Dof  = 1U << (CollisionLayers.Climbable2Dof - 1);
        public const uint DamageSource   = 1U << (CollisionLayers.DamageSource - 1);
        public const uint DeathSource    = 1U << (CollisionLayers.DeathSource - 1);
        public const uint Mario          = 1U << (CollisionLayers.Mario - 1);
        public const uint Enemy          = 1U << (CollisionLayers.Enemy - 1);
    }
    
    public static class PropertyHint
    {
        public const global::Godot.PropertyHint LayerDamageType = global::Godot.PropertyHint.Layers3DNavigation;
    }
}