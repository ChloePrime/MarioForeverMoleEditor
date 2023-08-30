using Godot;

namespace ChloePrime.MarioForever.Util;

public interface IAnimatedSprite
{
    public StringName Animation { get; set; }
    public float SpeedScale { get; set; }
    public void Play();
    public Node AsNode() => (Node)this;
    public Node GetParent();

    public event AnimationPlayer.AnimationFinishedEventHandler AnimationFinished;
}