using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.UI;

public partial class OneUp : Score
{
    [Export] public AudioStream Sound { get; private set; } = GD.Load<AudioStream>("res://resources/ui/SE_1up.wav");

    public override void _Ready()
    {
        base._Ready();
        if (Sound is { } sound)
        {
            this.PlaySound(sound);
        }
    }

    public override void TakeEffect()
    {
        if (!this.GetRule().DisableLives)
        {
            GlobalData.Lives += (int)Amount;
        }
        else
        {
            Visible = false;
            QueueFree();
        }
    }
}