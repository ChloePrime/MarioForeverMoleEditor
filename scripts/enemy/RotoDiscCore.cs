using System;
using System.Collections.Generic;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Util;
using DotNext.Collections.Generic;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class RotoDiscCore : Node2D, IMarioForeverNpc
{
    /// <summary>
    /// °/frame，CTF里的单位
    /// </summary>
    [Export] public float RotationSpeed { get; set; } = 1;

    [Export] public bool ModifyChildrenRotation { get; set; }

    [Export]
    public bool CoreVisible
    {
        get => _coreVisible;
        set => SetCoreVisible(value);
    }

    [Export] public MarioForeverNpcData NpcData { get; private set; }
    
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _sprite, NpSprite);
        _sprite.Visible = CoreVisible;
        NpcData = NpcData.ForceLocalToScene();

        _entering = false;
        _InitChildren();
        ChildEnteredTree += OnChildEnteredTree;
        ChildExitingTree += OnChildExitingTree;
    }
    
    private bool SetCoreVisible(bool value)
    {
        _coreVisible = value;
        if (_sprite is { } sprite) sprite.Visible = value;
        return value;
    }

    public virtual void _InitChildren()
    {
        ChildData.EnsureCapacity(GetChildCount());
        this.Children().ForEach(OnChildEnteredTree);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        var count = GetChildCount();
        var deltaAngle = -Units.AngularSpeed.CtfToGd(RotationSpeed) * (float)delta;
        for (int i = 0; i < count; i++)
        {
            var child = GetChild(i);
            if (child == _sprite || child is not Node2D child2D) continue;

            var (distance, oldAngle) = ChildData[i];
            var newAngle = oldAngle + deltaAngle;
            ChildData[i] = new Vector2(distance, newAngle);
            child2D.Position = Vector2.FromAngle(newAngle) * distance;
            if (ModifyChildrenRotation)
            {
                child2D.Rotation += deltaAngle;
            }
        }
    }

    private List<Vector2> ChildData { get; } = new();

    private static Vector2 DumpChildData(Node child)
    {
        if (child is not Node2D child2D)
        {
            return Vector2.Zero;
        }
        var distance = child2D.Position.Length();
        var angle = child2D.Position.Angle();
        return new Vector2(distance, angle);
    }

    private void OnChildEnteredTree(Node child)
    {
        if (_entering) return;
        if (child is IGrabbable grabbable)
        {
            grabbable.Grabber = this;
            grabbable.GrabNotify(new Mario.GrabEvent(this), null);
        }
        ChildData.Insert(child.GetIndex(), DumpChildData(child));
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        _entering = true;
        _exiting = false;
        SetDeferred(PropertyName._entering, false);
    }

    public override void _ExitTree()
    {
        _exiting = true;
        base._ExitTree();
    }

    private void OnChildExitingTree(Node child)
    {
        if (child.IsQueuedForDeletion())
        {
            OnChildDeleted(child);
        }
        else
        {
            Callable.From<Node, int>(OnChildExitingTree0).CallDeferred(child, child.GetIndex());
        }
    }

    private void OnChildDeleted(Node child)
    {
        ChildData.RemoveAt(child.GetIndex());
    }

    private void OnChildExitingTree0(Node child, int index)
    {
        if (_exiting) return;
        ChildData.RemoveAt(index);
        if (child is IGrabbable grabbable && grabbable.Grabber == this)
        {
            grabbable.GrabNotify(default, new Mario.GrabReleaseEvent(Mario.GrabReleaseFlags.Gently));
            grabbable.Grabber = null;
        }
    }

    private static readonly NodePath NpSprite = "Sprite";
    private Sprite2D _sprite;
    private bool _entering;
    private bool _exiting;
    private bool _coreVisible;
}