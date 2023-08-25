using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.UI;

public partial class LevelHud : Control
{
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _lifeCounter, NpLifeCounter);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_lastKnownLife != GlobalData.Lives)
        {
            _lastKnownLife = GlobalData.Lives;
            _lifeCounter.Text = _lastKnownLife.ToString();
        }
    }

    private static readonly NodePath NpLifeCounter = "Life";

    private Label _lifeCounter;
    private int _lastKnownLife = -1;
}