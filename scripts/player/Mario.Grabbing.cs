using System;
using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Player;

public partial class Mario
{
    public IGrabbable GrabbedObject { get; private set; }
    public bool CanActivelyGrab => !IsGrabbing && !_crouching && GameRule.EnableActiveGrabbing;
    public bool WillActivelyGrab => CanActivelyGrab && _runPressed;
    
    public bool IsGrabbing => GrabbedObject is not null;
    public bool WasJustGrabbing => IsGrabbing || _grabReleaseCooldown > 0;

    public GrabReleaseFlags GetCurrentReleaseFlags()
    {
        var gently = _downPressed;
        var tossUp = _upPressed;
        var @throw = (!tossUp || _leftPressed || _rightPressed) && !gently;

        var flags = (gently ? GrabReleaseFlags.Gently : GrabReleaseFlags.None) |
                     (tossUp ? GrabReleaseFlags.TossUp : GrabReleaseFlags.None) |
                     (@throw ? GrabReleaseFlags.TossHorizontally : GrabReleaseFlags.None);
        return flags;
    }

    public readonly record struct GrabEvent(Node OldParent);
    public readonly record struct GrabReleaseEvent(GrabReleaseFlags Flags);

    [Flags]
    public enum GrabReleaseFlags
    {
        None   = 0,
        Gently = 1,
        TossUp = 2,
        TossHorizontally = 4,
        Silent = 0x800,
    }
    
    public bool Grab(IGrabbable obj)
    {
        GrabRelease();
        GrabbedObject = obj;

        var oldParent = (obj.AsNode.GetParent() ?? obj.AsNode.GetLevel()) ?? GetTree().Root;
        var newNode = obj.AsNode;
        if (newNode.GetParent() is {} parent)
        {
            newNode.Reparent(_grabRoot, false);
            _oldParent = parent;
        }
        else
        {
            _grabRoot.AddChild(newNode);
        }
        if (obj.AsNode is GravityObjectBase gob)
        {
            gob.Position = new Vector2(gob.Size.X / 4, 0);
        }
        else
        {
            obj.AsNode.Position = Vector2.Zero;
        }

        obj.Grabber = this;
        obj.GrabNotify(new GrabEvent(oldParent), null);
        return true;
    }

    public bool GrabRelease() => GrabRelease(GetCurrentReleaseFlags());
    
    public bool GrabRelease(GrabReleaseFlags flags)
    {
        if (GrabbedObject is not { } grabbing) return false;
        GrabbedObject = null;
        _grabReleaseCooldown = 0.25F;
        
        grabbing.GrabNotify(default, new GrabReleaseEvent(flags));
        grabbing.Grabber = null;

        if (grabbing.AsNode.IsQueuedForDeletion())
        {
            return true;
        }
        
        var gently = flags.HasFlag(GrabReleaseFlags.Gently);
        if (!gently && !flags.HasFlag(GrabReleaseFlags.Silent))
        {
            GrabTossSound?.Play();
        }
        
        var oldNode = grabbing.AsNode;
        if (!IsInstanceValid(oldNode))
        {
            return false;
        }
        Node parent;
        if (_oldParent is { } oldParent && IsInstanceValid(oldParent))
        {
            parent = oldParent;
        }
        else
        {
            parent = (GetParent() ?? this.GetLevel()) ?? GetTree().Root;
        }
        var pos = oldNode.GlobalPosition;
        oldNode.GetParent()?.RemoveChild(oldNode);
        parent.AddChild(oldNode);
        oldNode.GlobalPosition = pos;

        if (grabbing is GravityObjectBase gob)
        {
            var tossUp = flags.HasFlag(GrabReleaseFlags.TossUp);
            var @throw = flags.HasFlag(GrabReleaseFlags.TossHorizontally);
            var coefficient = @throw && tossUp ? 1 / Mathf.Sqrt2 : 1;

            gob.XDirection = CharacterDirection;
            gob.XSpeed = (@throw ? coefficient * GrabReleaseThrowStrength : (tossUp ? 0 : 100)) + XSpeed;
            gob.YSpeed = (tossUp ? coefficient * -GrabReleaseTossUpStrength : 0);
        }

        return true;
    }

    private void InputGrab(InputEvent e)
    {
        if (IsGrabbing && e.IsActionPressed(Constants.ActionUseWeapon))
        {
            GrabRelease();
        }
    }

    private void ProcessGrab(float delta)
    {
        if (_grabReleaseCooldown > 0)
        {
            _grabReleaseCooldown -= delta;
        }
        if (IsGrabbing && !IsInstanceValid(GrabbedObject.AsNode))
        {
            GrabbedObject = null;
        }
        if (IsGrabbing && !_runPressed)
        {
            GrabRelease();
        }
    }

    private Node2D _grabRoot;
    private Node _oldParent;
    private float _grabReleaseCooldown;
    private static readonly NodePath NpGrabRoot = "SpriteRoot/Grab Root";
}