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

    [Export] public MarioForeverNpcData NpcData { get; private set; }
    
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _sprite, NpSprite);
        NpcData = NpcData.ForceLocalToScene();

        _entering = false;
        ChildData.EnsureCapacity(GetChildCount());
        this.Children().ForEach(OnChildEnteredTree);
        ChildEnteredTree += OnChildEnteredTree;
        ChildExitingTree += OnChildExitingTree;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        var count = GetChildCount();
        var deltaAngle = Units.AngularSpeed.CtfToGd(RotationSpeed) * (float)delta;
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
        if (!_entering && child is IGrabbable grabbable)
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
        base._ExitTree();
        _exiting = true;
    }

    private void OnChildExitingTree(Node child)
    {
        if (child.IsQueuedForDeletion())
        {
            OnChildDeleted(child);
        }
        else
        {
            OnChildExitingTree0(child);
            // CallDeferred(MethodName.OnChildExitingTree0, child);
        }
    }

    private void OnChildDeleted(Node child)
    {
        ChildData.RemoveAt(child.GetIndex());
    }

    private void OnChildExitingTree0(Node child)
    {
        ChildData.RemoveAt(child.GetIndex());
        if (!_exiting && child is IGrabbable grabbable && grabbable.Grabber == this)
        {
            grabbable.GrabNotify(default, new Mario.GrabReleaseEvent(Mario.GrabReleaseFlags.Gently));
            grabbable.Grabber = null;
        }
    }

    private static readonly NodePath NpSprite = "Sprite";
    private Sprite2D _sprite;
    private bool _entering;
    private bool _exiting;
}