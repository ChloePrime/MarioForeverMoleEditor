using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Player;

public partial class Mario
{
    public static class Constants
    {
        public static readonly StringName ActionMoveUp = "[moving] up";
        public static readonly StringName ActionMoveDown = "[moving] down";
        public static readonly StringName ActionMoveLeft = "[moving] left";
        public static readonly StringName ActionMoveRight = "[moving] right";
        public static readonly StringName ActionJump = "[moving] jump";
        public static readonly StringName ActionRun = "[moving] run";
        public static readonly StringName ActionFire = "[battle] fire";
        public static readonly NodePath NpSpriteRoot = "SpriteRoot";
        public static readonly NodePath NpHurtZone = "Hurt Zone";
        public static readonly NodePath NpJumpSound = "Jump Sound";
        public static readonly NodePath NpSwimSound = "Swim Sound";
        public static readonly NodePath NpHurtSound = "Hurt Sound";
        public static readonly MarioStatus SmallStatus;
        public static readonly MarioStatus BigStatus;
        public static readonly Vector2 FlipX = new(-1, 1);
        public static readonly Vector2 DoNotFlipX = new(1, 1);
        public static readonly StringName AnimStopped = "stopped";
        public static readonly StringName AnimWalking = "walking";
        public static readonly StringName AnimJumping = "jumping";
        public static readonly StringName AnimFalling = "falling";
        public static readonly StringName AnimCrouching = "crouching";
        public static readonly PackedScene CorpsePrefab = GD.Load<PackedScene>("res://objects/_internal/mario_corpse.tscn");
        [CtfAnimation(12)] public static readonly StringName AnimSwimming = "swimming";

        static Constants()
        {
            NodeEx.Load(out SmallStatus, "res://objects/_internal/mario_resources/status_small.tres");
            NodeEx.Load(out BigStatus, "res://objects/_internal/mario_resources/status_big.tres");
        }
    }
}