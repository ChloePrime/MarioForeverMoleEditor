using Godot;

namespace ChloePrime.MarioForever.Enemy;

public interface IStompable
{
    public Vector2 StompCenter { get; }

    public void StompBy(Node2D stomper);
}