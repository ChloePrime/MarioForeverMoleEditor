using System;
using System.Diagnostics.CodeAnalysis;
using ChloePrime.MarioForever.Player;
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
    public uint AcceptedDamageTypes = (uint)(DamageType.Unarmored | DamageType.Stomp);
    
    [Export(MaFo.PropertyHint.LayerDamageType)]
    public uint OneHitDamageTypes = (uint)(DamageType.Armored | DamageType.Stomp);


    [Export, MaybeNull]
    public AudioStream HurtSound { get; set; } = GD.Load<AudioStream>("res://resources/enemies/SE_hit_common.wav");
    
    [Export, MaybeNull]
    public AudioStream DeathSound { get; set; } = GD.Load<AudioStream>("res://resources/enemies/SE_enemy_down_2.ogg");
    
    [Export, MaybeNull]
    public AudioStream StompedSound { get; set; } = GD.Load<AudioStream>("res://resources/enemies/SE_stomp.wav");

    [Export] public PackedScene Score { get; set; } = GD.Load<PackedScene>("res://objects/ui/O_score_200.tscn");

    [Export]
    public PackedScene Corpse { get; set; } = GD.Load<PackedScene>("res://resources/enemies/generic_corpse.tscn");

    public EnemyCore Core { get; private set; }
    public Node2D Root => Core.Root;

    public void StompBy(Node2D stomper)
    {
        if (Core.NpcData.Friendly)
        {
            return;
        }
        HurtBy(new DamageEvent(DamageType.Stomp, stomper)
        {
            DamageToEnemy = stomper.GetRule().StompPower,
        });
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
        
        if (e.DamageTypes.ContainsAny(DamageType.Stomp))
        {
            EmitSignal(SignalName.Stomped);
        }

        // 先掉血，血没掉完就不死
        if (!CanBeOneHitKilledBy(e) && (Core.AsNpc.HitPoint > 0))
        {
            Core.AsNpc.AlterHitPoint(-e.DamageToEnemy);
            if (Core.AsNpc.HitPoint > 0)
            {
                OnHurt(e);
                return false;
            }
        }
        
        return Kill(e);
    }

    public virtual bool Kill(DamageEvent e)
    {
        if (_killed)
        {
            return true;
        }
        _killed = true;

        if (!e.IsSilent)
        {
            PlayDeathSound(e);
        }

        if (_oldParent is { } oldParent && Root.GetParent() is not null)
        {
            Root.Reparent(oldParent);
        }
        if (Root.GetParent() is {} parent)
        {
            if (!this.GetRule().DisableScore && CreateScore(e) is { } score)
            {
                parent.AddChild(score);
                score.GlobalPosition = Root.ToGlobal(ScorePivot);
            }
            if (CreateCorpse(e) is { } corpse)
            {
                CustomizeCorpse(e, corpse);
                parent.AddChild(corpse);
                corpse.GlobalPosition = Root.GlobalPosition;
            }
        }

        Core.EmitSignal(EnemyCore.SignalName.Died);
        Root.QueueFree();
        return true;
    }

    private bool _killed;
    private Node _oldParent;
    private static readonly StringName AnimHurt = "hurt";
    private static readonly Vector2 ScorePivot = new(0, -16);
    
    public override void _Ready()
    {
        base._Ready();
        Core = GetParent<EnemyCore>();
        BodyEntered += OnBodyEntered;
        if (Core.Root is IGrabbable grabbable)
        {
            grabbable.Grabbed += e => _oldParent = e.OldParent;
            grabbable.GrabReleased += _ => _oldParent = null;
        }
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

    private void OnHurt(DamageEvent e)
    {
        Core.EmitSignal(EnemyCore.SignalName.Hurt, e.DamageToEnemy);
        if (Core.Animation is {} animation)
        {
            animation.Play(AnimHurt);
        }
        if (!e.IsSilent)
        {
            HurtSound?.Play();
        }
    }

    public virtual Node2D CreateScore(DamageEvent e)
    {
        return Score?.Instantiate<Node2D>();
    }

    public virtual bool CanBeHurtBy(DamageEvent e)
    {
        return AcceptedDamageTypes.ContainsAny(e.DamageTypes);
    }
    
    public virtual bool CanBeOneHitKilledBy(DamageEvent e)
    {
        return OneHitDamageTypes.ContainsAny(e.DamageTypes);
    }

    public virtual void PlayDeathSound(DamageEvent e)
    {
        if (e.DamageTypes == DamageType.Stomp)
        {
            StompedSound?.Play();
        }
        else
        {
            DeathSound?.Play();
        }
    }

    public virtual Node2D CreateCorpse(DamageEvent e)
    {
        return Corpse.Instantiate<Node2D>();
    }

    public virtual void CustomizeCorpse(DamageEvent e, Node2D corpse)
    {
        if (corpse is not GenericCorpse cor) return;
        cor.XSpeed = Units.Speed.CtfToGd(2);
        cor.YSpeed = Units.Speed.CtfMovementToGd(-35);
        cor.XDirection = -Math.Sign((e.DirectSource ?? e.TrueSource).GlobalPosition.X - Root.GlobalPosition.X);
        cor.GlobalScale = Root.GlobalScale.Abs();
        cor.Rotator.Cycle *= cor.XDirection;

        if (Core.Sprite is not {} spr) return;
        cor.SpriteFrames = spr.SpriteFrames;
        cor.Animation = spr.Animation;
        cor.Frame = spr.Frame;
        cor.FlipH = spr.FlipH;
        cor.FlipV = spr.FlipV;
        cor.Stop();
    }
    
    // IStompable

    public Vector2 StompCenter => Root.GlobalPosition;

    public (float, float) GetDamage()
    {
        return Root is IMarioForeverNpc npc ? (npc.NpcData.DamageLo, npc.NpcData.DamageHi) : (0, 0);
    }
}