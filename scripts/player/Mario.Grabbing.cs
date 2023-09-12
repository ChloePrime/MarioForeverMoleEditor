﻿using System;
using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Player;

public partial class Mario
{
    public IGrabbable GrabbedObject { get; private set; }
    public bool IsGrabbing => GrabbedObject is not null;
    public bool WasJustGrabbing => IsGrabbing || _grabReleaseCooldown > 0;

    public GrabReleaseFlags GetCurrentReleaseFlags()
    {
        return (_downPressed ? GrabReleaseFlags.Gently : GrabReleaseFlags.None) |
               (_upPressed ? GrabReleaseFlags.TossUp : GrabReleaseFlags.None);
    }

    public readonly record struct GrabEvent(Node OldParent);
    public readonly record struct GrabReleaseEvent(GrabReleaseFlags Flags);

    [Flags]
    public enum GrabReleaseFlags
    {
        None   = 0,
        Gently = 1,
        TossUp = 2,
        Silent = 4,
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
            var @throw = (!tossUp || _leftPressed || _rightPressed) && !gently;
            var coefficient = @throw && tossUp ? 1 / Mathf.Sqrt2 : 1;

            gob.XDirection = CharacterDirection;
            gob.XSpeed = (@throw ? coefficient * GrabReleaseThrowStrength : 0) + XSpeed;
            gob.YSpeed = (tossUp ? coefficient * -GrabReleaseTossUpStrength : 0);
        }

        grabbing.Grabber = null;
        grabbing.GrabNotify(default, new GrabReleaseEvent(flags));
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