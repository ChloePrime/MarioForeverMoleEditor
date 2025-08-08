using System;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Player;

public partial class Mario
{
    public IGrabbable GrabbedObject { get; private set; }
    public bool CanActivelyGrab => !IsGrabbing && !IsCrouching && GameRule.EnableActiveGrabbing;
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
    
    public async void Grab(IGrabbable obj)
    {
        if (!Engine.IsInPhysicsFrame())
        {
            await this.WaitForPhysicsProcess();
        }
        GrabRelease();
        GrabbedObject = obj;

        var oldParent = (obj.AsNode.GetParent() ?? obj.AsNode.GetArea()) ?? GetTree().Root;
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
    }

    public void GrabRelease() => GrabRelease(GetCurrentReleaseFlags());
    
    public async void GrabRelease(GrabReleaseFlags flags)
    {
        if (!Engine.IsInPhysicsFrame())
        {
            await this.WaitForPhysicsProcess();
        }
        
        if (GrabbedObject is not { } grabbing) return;
        GrabbedObject = null;
        _grabReleaseCooldown = 0.25F;
        
        grabbing.GrabNotify(default, new GrabReleaseEvent(flags));
        grabbing.Grabber = null;

        if (grabbing.AsNode.IsQueuedForDeletion())
        {
            return;
        }
        
        var gently = flags.HasFlag(GrabReleaseFlags.Gently);
        if (!gently && !flags.HasFlag(GrabReleaseFlags.Silent))
        {
            GrabTossSound?.Play();
        }
        
        var grabbed = grabbing.AsNode;
        if (!IsInstanceValid(grabbed))
        {
            return;
        }
        Node parent;
        if (_oldParent is { } oldParent && IsInstanceValid(oldParent))
        {
            parent = oldParent.IsInsideTree() ? oldParent : this.GetArea();
        }
        else
        {
            parent = (GetParent() ?? this.GetArea()) ?? GetTree().Root;
        }
        grabbed.ReparentSafely(parent);

        if (grabbing is GravityObjectBase gob)
        {
            var tossUp = flags.HasFlag(GrabReleaseFlags.TossUp);
            var @throw = flags.HasFlag(GrabReleaseFlags.TossHorizontally);
            var coefficient = @throw && tossUp ? 1 / Mathf.Sqrt2 : 1;

            gob.XDirection = CharacterDirection;
            gob.XSpeed = (@throw ? coefficient * GrabReleaseThrowStrength : (tossUp ? 0 : 100)) + XSpeed;
            gob.YSpeed = (tossUp ? coefficient * -GrabReleaseTossUpStrength : 0);
        }
    }

    private void InputGrab()
    {
        if (IsGrabbing && Input.IsActionJustPressed(Constants.ActionUseWeapon))
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