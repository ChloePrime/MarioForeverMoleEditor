using System;
using ChloePrime.MarioForever.Level;
using ChloePrime.MarioForever.Util;
using Godot;
using Godot.NativeInterop;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.UI;

public partial class LevelHud : Control
{
    public Control GameOverLabel => _go;

    public string WorldName
    {
        get => _worldName.Text;
        set => _worldName.SetText(value);
    }
    
    public MaFoLevel CurrentLevel { get; set; }

    public void HintTimeout()
    {
        _timeoutHintAnim.Play("default");
    }
    
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
        this.GetNode(out _megaManHpSystem, NpMegaManHpSystem);
        this.GetNode(out _hpCounterL, NpHpCounterL);
        this.GetNode(out _hpCounterR, NpHpCounterR);
        this.GetNode(out _megaManHpBar, NpMegaManHpBar);
        this.GetNode(out _hpBarMax, NpHpCounterMaxBar);
        this.GetNode(out _hpBar, NpHpCounterBar);
        this.GetNode(out _world, NpWorld);
        this.GetNode(out _worldName, NpWorldName);
        this.GetNode(out _timeSystem, NpTimeSystem);
        this.GetNode(out _time, NpTime);
        this.GetNode(out _timeoutHintAnim, NpTimeoutHintAnim);
        this.GetNode(out _go, NpGameOver);
        _rule = this.GetRule();
        
        _lifeSystem.Watcher = () => !_rule.DisableLives;
        _scoreSystem.Watcher = () => !_rule.DisableScore;
        _coinSystem.Watcher = () => !_rule.DisableCoin;
        _hpSystem.Watcher = HasHitPoint;
        _megaManHpSystem.Watcher = () => HasHitPoint() && _rule.HitPointPolicy == GameRule.HitPointPolicyType.MegaMan;
        _world.Watcher = () => _worldName.Text.Length > 0;
        _timeSystem.Watcher = () =>
            _rule.TimePolicy != GameRule.TimePolicyType.Disable && CurrentLevel is { TimeLimit: >= 0 };
        
        _lifeCounter.Watch(() => GlobalData.Lives);
        _scoreCounter.Watch(() => GlobalData.Score);
        _coinCounter.Watch(() => GlobalData.Coins);
        _hpCounterL.Watch(GetLeftHpDisplay);
        _hpCounterR.Watch(() => _rule.HitPoint, GetRightHpDisplay);
        _hpBarMax.Watch(() => _rule.MaxHitPoint, hp => GetHpBarLength(hp) * _hpBarMax.Texture.GetSize().X);
        _hpBar.Watch(() => _rule.HitPoint, hp => GetHpBarLength(hp) * _hpBar.Texture.GetSize().X);
        _time.Watch(GetTimeWatchee, GetTimeDisplay);
        _time.TextChanged += () => _time.Theme = _rule.TimePolicy switch
        {
            GameRule.TimePolicyType.Date => _worldName.ThemeForLongText,
            _ => _lifeCounter.Theme,
        };

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

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_rule?.HitPointPolicy == GameRule.HitPointPolicyType.MegaMan)
        {
            _megaManHpBar.Value = (int)(_rule.HitPoint * _rule.MegaManHitPointBarLengthScale + 1e-5);
            _megaManHpBar.Max = (int)(_rule.MaxHitPoint * _rule.MegaManHitPointBarLengthScale + 1e-5);
        }
    }

    private bool HasHitPoint()
    {
        return _rule.HitPointPolicy != GameRule.HitPointPolicyType.Disabled && 
               (!_rule.HideHitPointAtZero || _rule.HitPoint > 0);
    }

    private string GetLeftHpDisplay() => _rule.HitPointPolicy switch
    {
        GameRule.HitPointPolicyType.Disabled => "",
        GameRule.HitPointPolicyType.MegaMan => "",
        GameRule.HitPointPolicyType.Metroid => "E",
        GameRule.HitPointPolicyType.Mario3D or GameRule.HitPointPolicyType.JRPG or _ => "HP",
    };

    private string GetRightHpDisplay(float hp) => _rule.HitPointPolicy switch
    {
        GameRule.HitPointPolicyType.Disabled => "",
        GameRule.HitPointPolicyType.Mario3D => "",
        GameRule.HitPointPolicyType.MegaMan => "",
        GameRule.HitPointPolicyType.Metroid => $"{GetDisplayHp(hp) % 100:00}",
        GameRule.HitPointPolicyType.JRPG or _ => $"{GetDisplayHp(hp)}/{GetDisplayHp(_rule.MaxHitPoint)}",
    };

    public float GetHpBarLength(float hp) => _rule.HitPointPolicy switch
    {
        GameRule.HitPointPolicyType.Mario3D => GetDisplayHp(hp),
        GameRule.HitPointPolicyType.MegaMan => 0,
        GameRule.HitPointPolicyType.Metroid => GetDisplayHp(hp) / 100,
        GameRule.HitPointPolicyType.Disabled or GameRule.HitPointPolicyType.JRPG or _ => 0,
    };
    
    private int GetDisplayHp(float hp)
    {
        var isMetroid = _rule.HitPointPolicy == GameRule.HitPointPolicyType.Metroid;
        return Math.Max(0, isMetroid ? Mathf.CeilToInt(hp - 1) : Mathf.FloorToInt(hp));
    }

    private double GetTimeWatchee() => _rule.TimePolicy switch
    {
        GameRule.TimePolicyType.Disable => 0,
        GameRule.TimePolicyType.Date => BitConverter.Int64BitsToDouble(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
        _ => Math.Max(0, GlobalData.Time),
    };

    private string GetTimeDisplay(double payload) => _rule.TimePolicy switch
    {
        GameRule.TimePolicyType.Disable => "",
        GameRule.TimePolicyType.Date => GetDateDisplay(),
        GameRule.TimePolicyType.Classic => Mathf.CeilToInt(payload).ToString(),
        GameRule.TimePolicyType.Countdown or GameRule.TimePolicyType.CountOnly => FormatTime(payload),
        _ => ""
    };

    private static string GetDateDisplay()
    {
        return DateTime.Now.ToString("yyyy/M/d");
    }

    private static string FormatTime(double time)
    {
        return TimeSpan.FromSeconds(time).ToString(@"mm\:ss\:ff");
    }

    private static readonly NodePath NpLifeSystem = "Life System";
    private static readonly NodePath NpScoreSystem = "Score System";
    private static readonly NodePath NpCoinSystem = "Coin System";
    private static readonly NodePath NpHpSystem = "Hit Point System";
    private static readonly NodePath NpMegaManHpSystem = "Mega Man HP";
    private static readonly NodePath NpMegaManHpBar = "Mega Man HP/Bar";
    private static readonly NodePath NpWorld = "World";
    private static readonly NodePath NpWorldName = "World/World Name";
    private static readonly NodePath NpTimeSystem = "Time System";
    private static readonly NodePath NpTime = "Time System/Time";
    private static readonly NodePath NpTimeoutHintAnim = "Time System/Timeout Hint Animation";
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
    private SubsystemHud _megaManHpSystem;
    private SubsystemHud _world;
    private SubsystemHud _timeSystem;
    private WorldName _worldName;
    private ValueWatcherLabel _lifeCounter;
    private ValueWatcherLabel _scoreCounter;
    private ValueWatcherLabel _coinCounter;
    private MegaManHpBar _megaManHpBar;
    private ValueWatcherLabel _hpCounterL;
    private ValueWatcherLabel _hpCounterR;
    private ValueWatcherBar _hpBarMax;
    private ValueWatcherBar _hpBar;
    private ValueWatcherLabel _time;
    private AnimationPlayer _timeoutHintAnim;
    private Control _go;
    private int _lastKnownLife = -1;
}