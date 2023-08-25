using System;
using System.Collections.Generic;
using Godot;

namespace ChloePrime.MarioForever.UI;

[GlobalClass]
public partial class ValueWatcherLabel : Label
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_ready && _needsUpdate())
        {
            Text = _display();
        }
    }

    public void Watch<T>(Func<T> getter)
    {
        T current = default;
        T temp = default;
        _needsUpdate = () => !EqualityComparer<T>.Default.Equals(current, (temp = getter()));
        _display = () =>
        {
            current = temp;
            return current.ToString();
        };
        _ready = true;
    }

    private Func<bool> _needsUpdate;
    private Func<string> _display;
    private bool _ready;
}