using System;
using System.Collections.Generic;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.UI;

[GlobalClass]
public partial class ValueWatcherLabel : Label
{
    public delegate string Stringifier<in T>(T data);

    public void Watch<T>(Func<T> getter)
    {
        Watch(getter, t => t.ToString());
    }
    
    public void Watch<T>(Func<T> getter, Stringifier<T> stringifier)
    {
        var observer = Observer.Watch(getter);
        observer.ValueChanged += () =>
        {
            Text = stringifier(observer.Value);
            TextChanged?.Invoke();
        };
        _updater = observer.Update;
    }

    public event Action TextChanged; 

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _updater?.Invoke();
    }

    private Action _updater;
}