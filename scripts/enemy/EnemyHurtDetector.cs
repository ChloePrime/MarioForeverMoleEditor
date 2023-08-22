using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Util;
using Godot;
using MarioForeverMoleEditor.scripts.util;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class EnemyHurtDetector : EnemyCore, IStompable
{
    [Export] public bool Stompable { get; set; }
    [Export] public float StompBounceStrength { get; set; } = Units.Speed.CtfToGd(9);

    [Export(MaFo.PropertyHint.LayerDamageType)]
    public uint AcceptedDamageTypes = (uint)(DamageType.Environment | DamageType.Star | DamageType.Bump | DamageType.KickShell);

    [Export]
    public AudioStream DeathSound { get; set; } = GD.Load<AudioStream>("res://resources/enemies/SE_enemy_down_2.ogg");

    [Export]
    public PackedScene Corpse { get; set; } = GD.Load<PackedScene>("res://resources/enemies/generic_corpse.tscn");

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D other)
    {
        if (Root is null)
        {
            return;
        }
        if (Stompable && other is Mario mario && mario.WillStomp(Root))
        {
            StompBy(mario);
        }
    }

    public void StompBy(Node2D stomper)
    {
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
        if (!CanBeHurtBy(e))
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
            var corpse = CreateCorpse(e);
            CustomizeCorpse(e, corpse);
            parent.AddChild(corpse);
            corpse.Position = Root.Position;
        }
        
        Root.QueueFree();
        return true;
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
        
        if (Sprite is not {} spr) return;
        cor.SpriteFrames = spr.SpriteFrames;
        cor.Animation = spr.Animation;
        cor.Frame = spr.Frame;
        cor.FlipH = spr.FlipH;
        cor.FlipV = !spr.FlipV;
        cor.Stop();
    }
    
    // IStompable

    public Vector2 StompCenter => Root.GlobalPosition;
}