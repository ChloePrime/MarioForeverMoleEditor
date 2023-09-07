﻿using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class EnemyHurtDetector : Area2D, IStompable
{
    [Export] public bool Stompable { get; set; }
    [Export] public float StompBounceStrength { get; set; } = Units.Speed.CtfToGd(9);

    [Export(MaFo.PropertyHint.LayerDamageType)]
    public uint AcceptedDamageTypes = (uint)(DamageType.Environment | DamageType.Star | DamageType.Bump | DamageType.KickShell);

    [Export]
    public AudioStream DeathSound { get; set; } = GD.Load<AudioStream>("res://resources/enemies/SE_enemy_down_2.ogg");

    [Export] public PackedScene Score { get; set; } = GD.Load<PackedScene>("res://objects/ui/O_score_200.tscn");

    [Export]
    public PackedScene Corpse { get; set; } = GD.Load<PackedScene>("res://resources/enemies/generic_corpse.tscn");

    public EnemyCore Core { get; private set; }
    public Node2D Root => Core.Root;

    public override void _Ready()
    {
        base._Ready();
        Core = GetParent<EnemyCore>();
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D other)
    {
        if (Core.NpcData.Friendly)
        {
            return;
        }
        if (Stompable && other is Mario mario && mario.WillStomp(Core.Root))
        {
            StompBy(mario);
        }
    }

    public void StompBy(Node2D stomper)
    {
        if (Core.NpcData.Friendly)
        {
            return;
        }
        HurtBy(new DamageEvent(DamageType.Stomp, stomper));
        if (stomper is Mario mario)
        {
            var strength = Input.IsActionPressed(Mario.Constants.ActionJump) ? mario.JumpStrength : StompBounceStrength;
            mario.CallDeferred(Mario.MethodName.Jump, strength);
        }
    }

    [Signal]
    public delegate void StompedEventHandler();

    public virtual bool HurtBy(DamageEvent e)
    {
        if (!CanBeHurtBy(e) || Core.NpcData.Friendly)
        {
            return false;
        }

        // 死亡流程
        if (e.DamageTypes.ContainsAny(DamageType.Stomp))
        {
            EmitSignal(SignalName.Stomped);
        }
        
        PlayDeathSound(e);

        if (Root.GetParent() is { } parent)
        {
            if (!this.GetRule().DisableScore && CreateScore(e) is { } score)
            {
                parent.AddChild(score);
                score.Position = Root.ToGlobal(ScorePivot);
            }
            if (CreateCorpse(e) is { } corpse)
            {
                CustomizeCorpse(e, corpse);
                parent.CallDeferred(Node.MethodName.AddChild, corpse);
                corpse.Position = Root.Position;
            }
        }
        
        Root.QueueFree();
        return true;
    }

    private static readonly Vector2 ScorePivot = new(0, -16);

    public virtual Node2D CreateScore(DamageEvent e)
    {
        return Score?.Instantiate<Node2D>();
    }

    public virtual bool CanBeHurtBy(DamageEvent e)
    {
        return AcceptedDamageTypes.ContainsAny(e.DamageTypes);
    }

    public virtual void PlayDeathSound(DamageEvent e)
    {
        this.PlaySound(DeathSound);
    }

    public virtual Node2D CreateCorpse(DamageEvent e)
    {
        return Corpse.Instantiate<Node2D>();
    }

    public virtual void CustomizeCorpse(DamageEvent e, Node2D corpse)
    {
        if (corpse is not GenericCorpse cor) return;
        cor.YSpeed = Units.Speed.CtfMovementToGd(-25);
        
        if (Core.Sprite is not {} spr) return;
        cor.SpriteFrames = spr.SpriteFrames;
        cor.Animation = spr.Animation;
        cor.Frame = spr.Frame;
        cor.FlipH = spr.FlipH;
        cor.FlipV = !spr.FlipV;
        cor.Stop();
    }
    
    // IStompable

    public Vector2 StompCenter => Root.GlobalPosition;

    public (float, float) GetDamage()
    {
        return Root is IMarioForeverNpc npc ? (npc.NpcData.DamageLo, npc.NpcData.DamageHi) : (0, 0);
    }
}