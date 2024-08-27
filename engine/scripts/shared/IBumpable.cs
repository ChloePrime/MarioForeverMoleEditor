using Godot;

namespace ChloePrime.MarioForever.Shared;

public interface IBumpable
{
    public bool Hidden { get; set; }
    public void OnBumpBy(Node2D bumper);
}