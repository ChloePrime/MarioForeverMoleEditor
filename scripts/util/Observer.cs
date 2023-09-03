using System;
using System.Collections.Generic;
using Godot;

namespace ChloePrime.MarioForever.Util;

public static class Observer
{
    public static Observer<T> Watch<T>(Func<T> getter)
    {
        return new Observer<T>(getter);
    }
    
    public static Observer<T> WatchOnProcess<T>(Func<T> getter)
    {
        var observer = new Observer<T>(getter);
        if (Engine.GetMainLoop() is SceneTree tree)
        {
            tree.ProcessFrame += observer.Update;
        }
        return observer;
    }
    
    public static Observer<T> WatchOnPhysics<T>(Func<T> getter)
    {
        var observer = new Observer<T>(getter);
        if (Engine.GetMainLoop() is SceneTree tree)
        {
            tree.PhysicsFrame += observer.Update;
        }
        return observer;
    }
}

public class Observer<T>: IDisposable
{
    public Observer(Func<T> getter) : this(getter, EqualityComparer<T>.Default)
    {
    }

    public Observer(Func<T> getter, EqualityComparer<T> comparer)
    {
        _getter = getter;
        _comparer = comparer;
    }
    
    public ref readonly T Value => ref _newValue;

    public ref readonly T OldValue => ref _oldValue;
    
    public event Action ValueChanged;

    public void Update()
    {
        var temp = _newValue = _getter();
        if (_comparer.Equals(_oldValue, temp))
        {
            return;
        }
        ValueChanged?.Invoke();
        _oldValue = temp;
    }

    private T _oldValue;
    private T _newValue;
    private readonly Func<T> _getter;
    private readonly EqualityComparer<T> _comparer;

    public void Dispose()
    {
        if (Engine.GetMainLoop() is SceneTree tree)
        {
            tree.ProcessFrame -= Update;
            tree.PhysicsFrame -= Update;
        }
        GC.SuppressFinalize(this);
    }
}