using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.UI;

public partial class LevelHud : Control
{
    public Control GameOverLabel => _go;
    
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _lifeSystem, NpLifeSystem);
        this.GetNode(out _lifeCounter, NpLifeCounter);
        this.GetNode(out _go, NpGameOver);
        _rule = this.GetRule();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_lifeSystem.Visible == _rule.DisableLives)
        {
            _lifeSystem.Visible = !_rule.DisableLives;
        }
        if (_lastKnownLife != GlobalData.Lives)
        {
            _lastKnownLife = GlobalData.Lives;
            _lifeCounter.Text = _lastKnownLife.ToString();
        }
    }

    private static readonly NodePath NpLifeSystem = "Life System";
    private static readonly NodePath NpLifeCounter = "Life System/Life";
    private static readonly NodePath NpGameOver = "GO";

    private GameRule _rule;
    private Control _lifeSystem;
    private Control _go;
    private Label _lifeCounter;
    private int _lastKnownLife = -1;
}