using System.Collections.Generic;
using System.Linq;
using ChloePrime.MarioForever.Player;
using DotNext.Collections.Generic;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

/**
 * 怪物伤害马里奥的判定
 */
[GlobalClass]
public partial class EnemyDamageSource : Area2D
{
    [Export] public EnemyHurtDetector Core { get; private set; }
    
    public override void _Ready()
    {
        base._Ready();
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
        // BodyEntered += OnBodyEntered;
        // BodyExited += OnBodyExited;
        Core ??= GetParent()?.Children().OfType<EnemyHurtDetector>().FirstOrNone().Or(null);
        if (Core is { } core)
        {
            core.Stomped += OnStomped;
        }
    }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (!IsInstanceValid(Core))
        {
            return;
        }
        if (Core.Root is GravityObjectBase { ReallyEnabled: false })
        {
            return;
        }
        if (Core.Stompable && _stompProtection > 0)
        {
            _stompProtection -= (float)delta;
            return;
        }
        
        var filteredOverlaps = Core.Stompable ? _overlaps.Where(m => !m.WillStomp(Core.Root)) : _overlaps;
        foreach (var mario in filteredOverlaps)
        {
            mario.Hurt();
        }
    }

    private float _stompProtection;

    private void OnStomped()
    {
        _stompProtection = 0.2F;
    }

    private void OnAreaEntered(Area2D other)
    {
        if (other is MarioHurtZone hurtZone)
        {
            OnBodyEntered(hurtZone.Root);
        }
    }
    
    private void OnAreaExited(Area2D other)
    {
        if (other is MarioHurtZone hurtZone)
        {
            OnBodyExited(hurtZone.Root);
        }
    }

    private void OnBodyEntered(Node2D other)
    {
        if (other is not Mario mario)
        {
            return;
        }

        _overlaps.Add(mario);
    }

    private void OnBodyExited(Node2D other)
    {
        if (other is not Mario mario)
        {
            return;
        }

        _overlaps.Remove(mario);
    }

    protected override void Dispose(bool disposing)
    {
        _overlaps.Clear();
        base.Dispose(disposing);
    }

    private readonly HashSet<Mario> _overlaps = new();
}