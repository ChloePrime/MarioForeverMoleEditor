using System;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;
using Vector2 = Godot.Vector2;

namespace ChloePrime.MarioForever.Bonus;

public partial class Brick : BumpableBlock
{
    [Export]
    public AudioStream BreakSound { get; set; } = GD.Load<AudioStream>("res://engine/resources/bonus/SE_brick_break.wav");

    public static readonly Int128 ScorePerBrick = 50;
    
    protected override void _OnBumpedBy(Node2D bumper)
    {
        if (bumper is not Mario { StandingSize: MarioSize.Small })
        {
            Break(bumper);
        }
        else
        {
            base._OnBumpedBy(bumper);
        }
    }

    public void Break(Node2D bumper)
    {
        BreakSound?.Play();
        KillMobsAbove(bumper);
        if (!bumper.GetRule().DisableScore)
        {
            GlobalData.Score += ScorePerBrick;
        }
        Callable.From(CreateBlockPieces).CallDeferred();
        Callable.From(QueueFree).CallDeferred();
    }
    
    // ********
    // 碎砖
    // ********

    private static readonly PackedScene BrickPiecePrefab = GD.Load<PackedScene>("res://engine/resources/bonus/brick_piece.tscn");
    private static readonly Vector2[] BrickPieceRelPosTable =
    {
        new (+5, +5),
        new (-6, +6),
        new (-8, -7),
        new (+6, -6),
    };
    
    private static readonly Vector2[] BrickPieceVelocityTable =
    {
        new (Units.Speed.CtfToGd(+4), Units.Speed.CtfToGd(-7)),
        new (Units.Speed.CtfToGd(-4), Units.Speed.CtfToGd(-7)),
        new (Units.Speed.CtfToGd(-2), Units.Speed.CtfToGd(-8)),
        new (Units.Speed.CtfToGd(+2), Units.Speed.CtfToGd(-8)),
    };
    
    private static readonly float[] BrickPieceRotationTable =
    {
        Mathf.Pi / 2,
        0,
        Mathf.Pi / 2,
        0,
    };

    private static readonly float[] BrickPieceAngularVelocityTable =
    {
        -3.5F * 2 * Mathf.Pi,
        +3.5F * 2 * Mathf.Pi,
        +3.0F * 2 * Mathf.Pi,
        -3.0F * 2 * Mathf.Pi,
    };

    private void CreateBlockPieces()
    {
        var root = this.GetPreferredRoot();
        for (int i = 0; i < BrickPieceRelPosTable.Length; i++)
        {
            var brick = BrickPiecePrefab.Instantiate<RigidBody2D>();
            root.AddChildAt(brick, ToGlobal(BrickPieceRelPosTable[i]));
            brick.DisablePhysicsInterpolationUntilNextFrame();
            brick.Rotation = BrickPieceRotationTable[i];
            brick.LinearVelocity = BrickPieceVelocityTable[i];
            brick.AngularVelocity = BrickPieceAngularVelocityTable[i];
        }
    }
}