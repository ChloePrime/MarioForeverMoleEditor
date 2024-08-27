using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Shared;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Level;

public partial class Checkpoint : Area2D, ICustomTileOffsetObject, INodeNameIsImportant
{
    [MaybeNull]
    public static Location SavedLocation { get; set; }

    public static readonly HashSet<NodePath> ActivatedCheckpoints = [];

    [Export]
    public AudioStream ActivationSound { get; set; } = GD.Load<AudioStream>("res://engine/resources/bonus/SE_Item_sprout.wav");
    
    public bool IsActivated { get; set; }

    [Signal]
    public delegate void ActivatedEventHandler();

    public NodePath GetCheckpointId()
    {
        return this.GetLevel() is { } level ? level.GetPathTo(this) : GetPath();
    }

    public virtual void _SetActivated()
    {
        IsActivated = true;
        SetDeferred(Area2D.PropertyName.Monitorable, false);
        _sprite.Play(AnimActivated);
    }

    private static readonly NodePath NpSprite = "Sprite";
    private static readonly StringName AnimActivated = "activated";
    private AnimatedSprite2D _sprite;
    private bool _reallyReady;

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _sprite, NpSprite);
        BodyEntered += OnCheckpointBodyEntered;
        if (ActivatedCheckpoints.Contains(GetCheckpointId()))
        {
            _SetActivated();
        }
        _reallyReady = true;
    }

    private void OnCheckpointBodyEntered(Node2D body)
    {
        if (!_reallyReady) return;
        if (IsActivated) return;
        Activate();
    }

    private void Activate()
    {
        IsActivated = true;
        
        if (this.GetLevelManager() is not { LevelInstance: MaFoLevel level })
        {
            this.LogWarn("Checkpoint activated outside of level");
        }
        else
        {
            SavedLocation = new Location
            {
                Level = GD.Load<PackedScene>(level.SceneFilePath),
                Area = level.CurrentAreaId,
                Position = this.GlobalPosition,
            };
        }

        ActivatedCheckpoints.Add(GetCheckpointId());
        
        ActivationSound.Play();
        _SetActivated();
        EmitSignal(SignalName.Activated);
    }

    public static void ClearSaved()
    {
        SavedLocation = null;
        ActivatedCheckpoints.Clear();
    }

    public void CustomOffset()
    {
        Translate(new Vector2(0, 16));
    }
}