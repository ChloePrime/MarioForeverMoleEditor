using System;
using ChloePrime.Godot.Util;
using Godot;

namespace ChloePrime.MarioForever.UI;

[GlobalClass]
public partial class ValueWatcherBar : TextureRect
{
    public delegate float WidthSupplier<in T>(T data);
    
    public void Watch<T>(Func<T> getter, WidthSupplier<T> widthSupplier)
    {
        var observer = Observer.Watch(getter);
        observer.ValueChanged += () => Size = Size with
        {
            X = widthSupplier(observer.Value),
        };
        _updater = observer.Update;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _updater?.Invoke();
    }

    private Action _updater;
}