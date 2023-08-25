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
        this.GetNode(out _scoreSystem, NpScoreSystem);
        this.GetNode(out _scoreCounter, NpScoreCounter);
        this.GetNode(out _go, NpGameOver);
        _rule = this.GetRule();
        _lifeSystem.Watcher = () => !_rule.DisableLives;
        _scoreSystem.Watcher = () => !_rule.DisableScore;
        _lifeCounter.Watch(() => GlobalData.Lives);
        _scoreCounter.Watch(() => GlobalData.Score);
    }

    private static readonly NodePath NpLifeSystem = "Life System";
    private static readonly NodePath NpScoreSystem = "Score System";
    private static readonly NodePath NpLifeCounter = "Life System/Life";
    private static readonly NodePath NpScoreCounter = "Score System/Score";
    private static readonly NodePath NpGameOver = "GO";

    private GameRule _rule;
    private SubsystemHud _lifeSystem;
    private SubsystemHud _scoreSystem;
    private ValueWatcherLabel _lifeCounter;
    private ValueWatcherLabel _scoreCounter;
    private Control _go;
    private int _lastKnownLife = -1;
}