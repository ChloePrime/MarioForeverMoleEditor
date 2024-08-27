using System;
using Godot;

namespace ChloePrime.MarioForever.UI;

public partial class SubsystemHud : Control
{
    public Func<bool> Watcher { get; internal set; }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        bool visible;
        if (Watcher is { } watcher && Visible != (visible = watcher()))
        {
            Visible = visible;
        }
    }
}