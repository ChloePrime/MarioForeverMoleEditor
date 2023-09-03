using System;
using ChloePrime.MarioForever.Util;
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
        this.GetNode(out _coinSystem, NpCoinSystem);
        this.GetNode(out _coinCounter, NpCoinCounter);
        this.GetNode(out _hpSystem, NpHpSystem);
        this.GetNode(out _hpCounterL, NpHpCounterL);
        this.GetNode(out _hpCounterR, NpHpCounterR);
        this.GetNode(out _hpBarMax, NpHpCounterMaxBar);
        this.GetNode(out _hpBar, NpHpCounterBar);
        this.GetNode(out _go, NpGameOver);
        _rule = this.GetRule();
        _lifeSystem.Watcher = () => !_rule.DisableLives;
        _scoreSystem.Watcher = () => !_rule.DisableScore;
        _coinSystem.Watcher = () => !_rule.DisableCoin;
        _hpSystem.Watcher = HasHitPoint;
        _lifeCounter.Watch(() => GlobalData.Lives);
        _scoreCounter.Watch(() => GlobalData.Score);
        _coinCounter.Watch(() => GlobalData.Coins);
        _hpCounterL.Watch(GetLeftDisplay);
        _hpCounterR.Watch(() => _rule.HitPoint, GetRightDisplay);
        _hpBarMax.Watch(() => _rule.MaxHitPoint, hp => GetBarLength(hp) * _hpBarMax.Texture.GetSize().X);
        _hpBar.Watch(() => _rule.HitPoint, hp => GetBarLength(hp) * _hpBar.Texture.GetSize().X);

        var observer = Observer.WatchOnPhysics(() => _rule.HitPointPolicy == GameRule.HitPointPolicyType.Mario3D);
        var barPosX = _hpBar.Position.X;
        observer.ValueChanged += () =>
        {
            var x = observer.Value ? 0 : barPosX;
            _hpBarMax.Position = _hpBarMax.Position with { X = x };
            _hpBar.Position = _hpBar.Position with { X = x };
        };
        TreeExited += () =>
        {
            if (IsQueuedForDeletion())
            {
                observer.Dispose();
            }
        };
    }

    private bool HasHitPoint()
    {
        return _rule.HitPointPolicy != GameRule.HitPointPolicyType.Disabled && 
               (!_rule.HideHitPointAtZero || _rule.HitPoint > 0);
    }

    private string GetLeftDisplay() => _rule.HitPointPolicy switch
    {
        GameRule.HitPointPolicyType.Disabled => "",
        GameRule.HitPointPolicyType.Metroid => "E",
        GameRule.HitPointPolicyType.Mario3D or GameRule.HitPointPolicyType.JRPG or _ => "HP",
    };

    private string GetRightDisplay(float hp) => _rule.HitPointPolicy switch
    {
        GameRule.HitPointPolicyType.Disabled => "",
        GameRule.HitPointPolicyType.Metroid => $"{GetDisplayHp(hp) % 100:00}",
        GameRule.HitPointPolicyType.Mario3D => "",
        GameRule.HitPointPolicyType.JRPG or _ => $"{GetDisplayHp(hp)}/{GetDisplayHp(_rule.MaxHitPoint)}",
    };

    public float GetBarLength(float hp) => _rule.HitPointPolicy switch
    {
        GameRule.HitPointPolicyType.Mario3D => GetDisplayHp(hp),
        GameRule.HitPointPolicyType.Metroid => GetDisplayHp(hp) / 100,
        GameRule.HitPointPolicyType.Disabled or GameRule.HitPointPolicyType.JRPG or _ => 0,
    };
    
    private int GetDisplayHp(float hp)
    {
        var isMetroid = _rule.HitPointPolicy == GameRule.HitPointPolicyType.Metroid;
        return Math.Max(0, isMetroid ? Mathf.CeilToInt(hp - 1) : Mathf.FloorToInt(hp));
    }

    private static readonly NodePath NpLifeSystem = "Life System";
    private static readonly NodePath NpScoreSystem = "Score System";
    private static readonly NodePath NpCoinSystem = "Coin System";
    private static readonly NodePath NpHpSystem = "Hit Point System";
    private static readonly NodePath NpLifeCounter = "Life System/Life";
    private static readonly NodePath NpScoreCounter = "Score System/Score";
    private static readonly NodePath NpCoinCounter = "Coin System/Coin";
    private static readonly NodePath NpHpCounterL = "Hit Point System/Text L";
    private static readonly NodePath NpHpCounterR = "Hit Point System/Text R";
    private static readonly NodePath NpHpCounterMaxBar = "Hit Point System/Text R/Bar Background";
    private static readonly NodePath NpHpCounterBar = "Hit Point System/Text R/Bar";
    private static readonly NodePath NpGameOver = "GO";

    private GameRule _rule;
    private SubsystemHud _lifeSystem;
    private SubsystemHud _scoreSystem;
    private SubsystemHud _coinSystem;
    private SubsystemHud _hpSystem;
    private ValueWatcherLabel _lifeCounter;
    private ValueWatcherLabel _scoreCounter;
    private ValueWatcherLabel _coinCounter;
    private ValueWatcherLabel _hpCounterL;
    private ValueWatcherLabel _hpCounterR;
    private ValueWatcherBar _hpBarMax;
    private ValueWatcherBar _hpBar;
    private Control _go;
    private int _lastKnownLife = -1;
}