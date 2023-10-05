using System.Collections.Immutable;
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
        public static readonly StringName ActionUseWeapon = "[battle] use weapon";
        public static readonly NodePath NpSpriteRoot = "SpriteRoot";
        public static readonly NodePath NpSprite3DRoot = "SpriteRoot/Sprite_3D/Viewport/3D Character Root";
        public static readonly NodePath NpHurtZone = "Hurt Zone";
        public static readonly NodePath NpDeathZone = "Death Zone";
        public static readonly NodePath NpJumpSound = "Jump Sound";
        public static readonly NodePath NpSwimSound = "Swim Sound";
        public static readonly NodePath NpHurtSound = "Hurt Sound";
        public static readonly NodePath NpSkidSound = "Skid Sound";
        public static readonly NodePath NpInvTimer = "Invulnerable Timer";
        public static readonly NodePath NpSlipperyGas = "Slippery Foot Gas";
        public static readonly NodePath NpSmkTimer = "Sprint Smoke Timer";
        public static readonly NodePath NpSkdTimer = "Skid Smoke Timer";
        public static readonly NodePath NpStompTracker = "Stomp Combo Tracker";
        public static readonly NodePath NpWaterDetector = "Water Detector";
        public static readonly NodePath NpWaterSurfaceDetector = "Water Surface Detector";
        public static readonly NodePath NpWaterSurfaceDetector2 = "Water Surface Detector 2";
        public static readonly MarioStatus SmallStatus;
        public static readonly MarioStatus BigStatus;
        public static readonly MarioStatus FireStatus;
        public static readonly Vector2 FlipX = new(-1, 1);
        public static readonly Vector2 DoNotFlipX = new(1, 1);
        public static readonly StringName AnimStopped = "[00] stopped";
        public static readonly StringName AnimGrabStop = "[00g] grabbing-stop";
        public static readonly StringName AnimWalking = "[01] walking";
        public static readonly StringName AnimGrabWalk = "[01g] grabbing-walk";
        public static readonly StringName AnimRunning = "[02] running";
        public static readonly StringName AnimTurning = "[03] turning";
        public static readonly StringName AnimLaunching = "[04] launching";
        public static readonly StringName AnimJumping = "[05] jumping";
        public static readonly StringName AnimGrabJump = "[05g] grabbing-jump";
        public static readonly StringName AnimLeaping = "[05ex] leaping";
        public static readonly StringName AnimFalling = "[06] falling";
        public static readonly StringName AnimCrouching = "[07] crouching";
        public static readonly StringName ShaderParamAlpha = "rainbow_alpha";
        [CtfAnimation(12)] public static readonly StringName AnimSwimming = "[12] swimming";
        public static readonly StringName DeprecatedAnimAppearing = "[03] appearing";

        public static readonly ImmutableHashSet<StringName> OptionalAnimations = ImmutableHashSet.Create(
            AnimTurning,
            AnimRunning,
            AnimFalling,
            AnimCrouching,
            AnimLaunching,
            AnimLeaping,
            AnimGrabStop,
            AnimGrabWalk,
            AnimGrabJump
        );

        public static readonly ImmutableHashSet<StringName> SpecialAnimations = ImmutableHashSet.Create(
            AnimLaunching
        );
        
        public static readonly PackedScene CorpsePrefab = GD.Load<PackedScene>("res://resources/mario/mario_corpse.tscn");
        public static readonly PackedScene SprintSmoke = GD.Load<PackedScene>("res://resources/mario/mario_sprint_smoke.tscn");
        public static readonly PackedScene SkidSmoke = GD.Load<PackedScene>("res://objects/effect/O_smoke_s.tscn");

        static Constants()
        {
            NodeEx.Load(out SmallStatus, "res://resources/mario/status_small.tres");
            NodeEx.Load(out BigStatus, "res://resources/mario/status_big.tres");
            NodeEx.Load(out FireStatus, "res://resources/mario/status_fire.tres");
        }
    }
}