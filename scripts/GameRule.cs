using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Level;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Shared;
using ChloePrime.MarioForever.Util;
using DotNext;
using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever;

public partial class GameRule : Resource
{
    public static GameRule Default => _default ??= GD.Load<GameRule>("res://R_game_rule.tres");
    public static GameRule Get() => GetManager(Engine.GetMainLoop() as SceneTree)?.GameRule ?? Default;
    
    [ExportGroup("Vanilla")]
    [Export] public bool DisableLives { get; set; }
    [Export] public bool DisableScore { get; set; }
    
    [ExportSubgroup("Coins")]
    [Export] public bool DisableCoin { get; set; }
    [Export] public bool CoinAutoExchangeLife { get; set; } = true;
    [Export] public int CostOf1Life { get; set; } = 100;

    [ExportSubgroup("Time")]
    [Export] public TimePolicyType TimePolicy { get; set; } = TimePolicyType.Classic;
    [Export] public double ClassicTimeUnitSize { get; set; } = 0.64;
    [Export] public AudioStream TimeoutHintSound { get; set; } = GD.Load<AudioStream>("res://resources/level/ME_timeout.ogg");

    public enum TimePolicyType
    {
        /// <summary>
        /// 禁用时间机制
        /// </summary>
        Disable,

        /// <summary>
        /// 倒计时结束后杀死马里奥
        /// </summary>
        Classic,

        /// <summary>
        /// 倒计时结束后发出信号（TODO 尚未实现）
        /// </summary>
        Countdown,

        /// <summary>
        /// 正计时
        /// </summary>
        CountOnly,
        
        /// <summary>
        /// 显示当前日期
        /// </summary>
        Date,
    }

    [Export]
    public Array<PackedScene> AddLifeMethod { get; set; } = new()
    {
        GD.Load<PackedScene>("res://objects/ui/O_1up.tscn")
    };
    
    [ExportSubgroup("Mario")]
    [Export, MaybeNull] public MarioStatus DefaultStatus { get; set; }
    
    [ExportSubgroup("Shell Combo")]
    [Export] public ComboRule DefaultComboRule { get; set; }
    [Export] public bool ComboOnStomp { get; set; } = true;

    /// <summary>
    /// 开启后夹子在屏幕外面时将不会缩回去。
    /// 在夹子进入屏幕后才会开始缩回。
    /// </summary>
    [ExportSubgroup("Clamps")]
    [Export] public bool W10EClampOffScreenPolicy { get; set; } = true;
    
    /// <summary>
    /// 开启后夹子的子弹撞到实心会爆炸
    /// </summary>
    [Export] public bool ClampFireballExplodeOnWallHit { get; set; } = true;
    
        
    [ExportGroup("Simple QoL")]
    [ExportSubgroup("Direction Calculation")]
    [Export] public MarioDirectionPolicy CharacterDirectionPolicy { get; set; } = MarioDirectionPolicy.FollowXSpeed;

    [ExportSubgroup("Water")]
    [Export] public bool KeepXSpeedInWater { get; set; }
    
    public enum MarioDirectionPolicy
    {
        FollowXSpeed,
        FollowControlDirection,
    }
    
    [ExportSubgroup("Toss Fireball Upward")]
    [Export] public bool EnableTossFireballUpward { get; set; } = true;
    [Export] public float TossFireballUpwardStrength { get; set; } = 400;

    [ExportGroup("")]
    [ExportSubgroup("")] 
    [Export] public bool BulletLauncherBreakable { get; set; } = true;

    [ExportGroup("Advanced")] 
    [Export] public bool EnableMarioBursting { get; set; } = true;

    [ExportSubgroup("Jump Height Bonus")]
    [Export] public float XSpeedBonus { get; set; } = Units.Speed.CtfToGd(1F);
    [Export] public float SprintingBonus { get; set; } = 0;

    [ExportSubgroup("Player's Hit Point")]
    [Export] public HitPointPolicyType HitPointPolicy { get; set; } = HitPointPolicyType.Metroid;

    [Export] public bool HideHitPointAtZero { get; set; } = true;
    
    /// <summary>
    /// 如果为 false，那么玩家在 HP 归零后再次受伤会掉状态。
    /// 如果为 true，那么玩家在 HP 归零时会立刻死亡。
    /// </summary>
    [Export] public bool KillPlayerWhenHitPointReachesZero { get; set; }

    
    [Export] public bool HitPointProtectsYourPowerup { get; set; } = true;
    [Export] public bool HitPointProtectsDeath { get; set; } = true;
    [Export] public float HitPointProtectsDeathCostLo { get; set; } = 3;
    [Export] public float HitPointProtectsDeathCostHi { get; set; } = 100;
    [Export] public bool CoinGivesHitPoint { get; set; } = true;
    [Export] public float MaxHitPointLo { get; set; } = 8;
    [Export] public float MaxHitPointHi { get; set; } = 400;
    [Export] public float DefaultHitPointLo { get; set; } = 8;
    [Export] public float DefaultHitPointHi { get; set; } = 100;
    [Export] public float DefaultTerrainDamageLo { get; set; } = 1;
    [Export] public float DefaultTerrainDamageHi { get; set; } = 16;
    [Export] public float MegaManHitPointBarLengthScale { get; set; } = 4;

    
    [ExportSubgroup("Player Weapon's Power")]
    [Export] public float StompPower { get; set; } = 100;
    [Export] public float FireballPower { get; set; } = 20;

    
    [ExportSubgroup("Grabbing")]
    [Export] public bool EnableActiveGrabbing { get; set; } = true;

    public void ResetGlobalData()
    {
        GlobalData.ResetRuleNeutralValues();
        ReloadStatus();
        ResetHitPoint();
        Checkpoint.ClearSaved();
    }

    public void ReloadStatus()
    {
        GlobalData.Status = DefaultStatus ?? MarioStatus.Small;
    }

    private static Optional<LevelManager> _manager;
    private static GameRule _default;

    private static LevelManager GetManager(SceneTree tree)
    {
        if (_manager.IsUndefined)
        {
            _manager = tree?.Root.Walk().OfType<LevelManager>().FirstOrDefault();
        }
        return _manager.OrDefault();
    }
}

public static class GameRuleEx
{
    public static GameRule GetRule(this Node node) => node.GetLevelManager()?.GameRule ?? GameRule.Get();
}