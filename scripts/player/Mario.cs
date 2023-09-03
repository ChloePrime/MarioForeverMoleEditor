using System.Collections.Generic;
using System.Linq;
using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Util;
using DotNext.Collections.Generic;
using Godot;
using Godot.Collections;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Player;

[GlobalClass]
[Icon("res://resources/mario/AS_icon.tres")]
public partial class Mario : CharacterBody2D
{
    #region Movement Params
    
    [ExportGroup("Initial Values")]

    [Export] public float YSpeed { get; private set; }

    [ExportGroup("Mario Movement")]
    [ExportSubgroup("Walking & Running")]
    [Export] public float MaxSpeedWhenWalking { get; set; } = Units.Speed.CtfMovementToGd(35);
    [Export] public float MaxSpeedWhenRunning { get; set; } = Units.Speed.CtfMovementToGd(60);
    [Export] public float MaxSpeedWhenSprinting { get; set; } = Units.Speed.CtfMovementToGd(80);
    [Export] public float MinSpeed { get; set; } = Units.Speed.CtfMovementToGd(8);
    [Export] public float AccelerationWhenWalking { get; set; } = Units.Acceleration.CtfMovementToGd(1);
    [Export] public float AccelerationWhenRunning { get; set; } = Units.Acceleration.CtfMovementToGd(1);
    [Export] public float AccelerationWhenTurning { get; set; } = Units.Acceleration.CtfMovementToGd(4);
    [Export] public float SprintChargeTime { get; set; } = 24F / 64;
    [Export] public float SprintCooldownSpeed { get; set; } = 1 / 6.4F;
    [Export] public bool AllowMoveOutOfScreen { get; set; }
    [Export] public float ScreenBorderPadding { get; set; } = 16;

    [Export]
    public AudioStream SprintStartSound { get; set; } = GD.Load<AudioStream>("res://resources/mario/SE_run.ogg");
    
    
    [ExportSubgroup("Jumping & Swimming")]
    [Export] public float Gravity { get; set; } = Units.Acceleration.CtfToGd(1);
    [Export] public float MaxYSpeed { get; set; } = Units.Speed.CtfToGd(10);
    [Export] public float JumpStrength { get; set; } = Units.Speed.CtfToGd(13);
    
    /// <summary>
    /// 在脚滑踩空后的多少时间内仍然可以跳起来，
    /// 即 "威利狼跳"
    /// </summary>
    [Export] public float JumpTolerateTime { get; set; } = 0.1F;
    [Export] public float GravityInWater { get; set; } = Units.Acceleration.CtfToGd(1) / 10;
    [Export] public float MaxYSpeedInWater { get; set; } = Units.Speed.CtfToGd(3);
    [Export] public float SwimStrength { get; set; } = Units.Speed.CtfToGd(3);

    
    [ExportSubgroup("Swimming (Advanced)")]
    [Export] public float SwimStrengthAcc { get; set; } = Units.Speed.CtfToGd(0.5F);

    [Export] public float JumpStrengthOutOfWater { get; set; } = Units.Speed.CtfToGd(9);

    /// <summary>
    /// 水下按住上时的最小起跳速度（Factory Engine 专属）
    /// </summary>
    [Export] public float MinSwimStrengthWhenFloatingUp { get; set; } = Units.Speed.CtfToGd(6);
        
    /// <summary>
    /// 水下按住上时的最大起跳速度（Factory Engine 专属）
    /// </summary>
    [Export] public float MaxSwimStrengthWhenFloatingUp { get; set; } = Units.Speed.CtfToGd(8);

    [Export] public float SwimStrengthAccWhenFloatingUp { get; set; } = Units.Speed.CtfToGd(1);
    
    /// <summary>
    /// 水下按住下时的加速度（Factory Engine 专属）
    /// </summary>
    [Export] public float GravityWhenSinking { get; set; } = Units.Acceleration.CtfToGd(1) * 3 / 10;
    
    /// <summary>
    /// 水下按住下时的最大速度（Factory Engine 专属）
    /// </summary>
    [Export] public float MaxYSpeedWhenSinking { get; set; } = Units.Speed.CtfToGd(6);
    
    /// <summary>
    /// 水下下沉时的最小起跳速度（Factory Engine 专属）
    /// </summary>
    [Export] public float MinSwimStrengthWhenSinking { get; set; } = Units.Speed.CtfToGd(1);
        
    /// <summary>
    /// 水下下沉时的最大起跳速度（Factory Engine 专属）
    /// </summary>
    [Export] public float MaxSwimStrengthWhenSinking { get; set; } = Units.Speed.CtfToGd(2);
    [Export] public float SwimStrengthAccWhenSinking { get; set; } = Units.Speed.CtfToGd(0.5F);

    #endregion
    
    /// <summary>
    /// 每个大小所对应的碰撞体积，数量必须是 3 个，对应小个子，大个子，迷你状态
    /// </summary>
    /// <see cref="MarioSize"/> 本数组适用的 key 值
    [ExportGroup("Mario Collision")]
    [Export] public Array<CollisionShape2D> CollisionBySize { get; private set; }

    
    [ExportGroup("RPG")]
    [Export] public float InvulnerableTimeOnHurt { get; private set; } = 2;
    [Export] public float InvulnerableTimeOnSmallHpHurt { get; private set; } = 1;
    [Export] public float SmallHpThreshold { get; private set; } = 20;
    [Export] public bool FastRetry { get; set; }
    [Export] public float DefaultRainbowFlashTime { get; set; } = 1.2F;
    [Export] public Array<MarioStatus> StatusList { get; private set; }
    [Export] public Array<Node2D> MuzzleBySize { get; private set; }
    [Export] public Array<Node> StatusSpriteNodeList { get; private set; }

    [ExportGroup("")]
    [Export] public float InvulnerabilityFlashSpeed { get; set; } = 8;
    
    public Node2D Muzzle => MuzzleBySize[(int)_currentSize];
    
    public GameRule GameRule { get; private set; }
    public StringName ExpectedAnimation { get; private set; }
    public float ExpectedAnimationSpeed { get; private set; }

    public void Jump()
    {
        Jump(JumpStrength);
        _jumpSound.Play();
    }

    public void Swim(float strength)
    {
        Jump(strength);
        _swimSound.Play();
    }

    public void Jump(float strength)
    {
        var bonus = GameRule.XSpeedBonus * XSpeed / MaxSpeedWhenRunning + (_sprinting ? GameRule.SprintingBonus : 0);
        YSpeed = -strength - bonus;
        _isInAir = true;
        _wilyJumpTime = -1;
    }
    
    public bool WillStomp(IStompable other)
    {
        return YSpeed >= 0 && ToLocal(other.StompCenter).Y >= -8;
    }

    public bool WillStomp(Node2D other)
    {
        return YSpeed >= 0 && ToLocal(other.GlobalPosition).Y >= -8;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _camera = GetViewport().GetCamera2D() ?? _camera;
        if (_camera != null)
        {
            var pos = GlobalPosition;
            _camera.GlobalPosition = new Vector2(Mathf.Round(pos.X), Mathf.Round(pos.Y));
        }
        ProcessFlashing((float)delta);
    }

    public override void _PhysicsProcess(double deltaD)
    {
        base._PhysicsProcess(deltaD);
        if (_hurtStack > 0)
        {
            Hurt(new DamageEvent
            {
                DamageTypes = DamageType.Environment,
                DirectSource = null,
                TrueSource = null,
                DamageLo = GameRule.DefaultTerrainDamageLo,
                DamageHi = GameRule.DefaultTerrainDamageHi,
            });
        }
        if (GlobalPosition.Y > this.GetFrame().End.Y + 48)
        {
            Kill();
            return;
        }
        var delta = (float)deltaD;
        PhysicsProcessX(delta);
        PhysicsProcessY(delta);
        UpdateCrouch();
        UpdateAnimation();

        var shouldSkid = (_leftPressed || _rightPressed) && !_isInAir && (_turning || (_crouching && XSpeed > 0));
        if (_skidding != shouldSkid)
        {
            _skidding = shouldSkid;
            if (shouldSkid)
            {
                _skidSound.Play();
                _skidSmokeTimer.EmitSignal(Timer.SignalName.Timeout);
                _skidSmokeTimer.Start();
            }
            else
            {
                _skidSound.Stop();
                _skidSmokeTimer.Stop();
            }
        }

        if (_firePreInput > 0)
        {
            _firePreInput -= delta;
            TryFire();
        }
    }

    private void TryFire()
    {
        if (GlobalData.Status.Fire(this))
        {
            _firePreInput = 0;
            if (!_crouching && _currentSprite is { } sprite && _optionalAnimations.Contains(Constants.AnimLaunching))
            {
                sprite.Animation = Constants.AnimLaunching;
            }
        }
    }

    private void UpdateCrouch()
    {
        if (_standingSize != MarioSize.Big)
        {
            return;
        }
        if (_downPressed && !_crouching)
        {
            SetSize(MarioSize.Small);
            _crouching = true;
        }
        if (!_downPressed && _crouching)
        {
            var bigShape = CollisionBySize[(int)MarioSize.Big];

            if (MeInAnArray.Count == 0)
            {
                MeInAnArray.Add(GetRid());
            }

            var query = new PhysicsShapeQueryParameters2D()
            {
                Shape = bigShape.Shape,
                Transform = bigShape.GlobalTransform,
                CollisionMask = this.CollisionMask,
                Exclude = MeInAnArray,
                CollideWithAreas = false
            };

            var willCollide = GetWorld2D().DirectSpaceState.CollideShape(query).Count > 0;
            if (!willCollide)
            {
                SetSize(MarioSize.Big);
                _crouching = false;
            }
        }
    }

    private static readonly Array<Rid> MeInAnArray = new();

    private void SetSize(MarioSize size)
    {
        for (var i = 0; i < CollisionBySize.Count; i++)
        {
            CollisionBySize[i].CallDeferred(CollisionShape2D.MethodName.SetDisabled, i != (int)size);
        }
        _hurtZone.SetSize(size);
        _deathZone.SetSize(size);
        _currentSize = size;

        if (size != MarioSize.Big && _crouching)
        {
            _crouching = false;
        }

        if (size == MarioSize.Big)
        {
            TestAndForceCrouch();
        }
    }

    private void TestAndForceCrouch()
    {
        var bigShape = CollisionBySize[(int)MarioSize.Big];
        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = bigShape.Shape,
            Transform = bigShape.GlobalTransform,
            CollisionMask = CollisionMask,
            CollideWithBodies = true,
            CollideWithAreas = false,
        };
        if (GetWorld2D().DirectSpaceState.IntersectShape(query).Count > 0)
        {
            SetSize(MarioSize.Small);
            _crouching = true;
        }
    }

    private void UpdateAnimation()
    {
        TrySwitchStatusSprite();
        _spriteRoot.Scale = CharacterDirection < 0 ? Constants.FlipX : Constants.DoNotFlipX;

        var hasSprite = _currentSprite is { } sprite;

        if (IsInSpecialAnimation())
        {
            if (hasSprite)
            {
                _currentSprite.SpeedScale = 1;
            }
            return;
        }
        var (anim, speed) = GetAnimationIdAndSpeed();
        if (hasSprite)
        {
            if (_currentSprite.Animation != anim)
            {
                _currentSprite.Animation = anim;
            }
            _currentSprite.SpeedScale = speed;
        }
    }

    private (StringName, float) GetAnimationIdAndSpeed()
    {
        StringName anim;
        var speed = 1F;
        if (_crouching && _optionalAnimations.Contains(Constants.AnimCrouching))
        {
            return (Constants.AnimCrouching, speed);
        }
        if (_isInAir)
        {
            if (_isInWater)
            {
                anim = Constants.AnimSwimming;
            }
            else if (_sprinting && _optionalAnimations.Contains(Constants.AnimLeaping))
            {
                anim = Constants.AnimLeaping;
            }
            else
            {
                anim = (YSpeed >= 0 && _optionalAnimations.Contains(Constants.AnimFalling))
                    ? Constants.AnimFalling
                    : Constants.AnimJumping;
            }
        }
        else
        {
            if (XSpeed > 0)
            {
                if (_optionalAnimations.Contains(Constants.AnimTurning) && _walking && _turning)
                {
                    anim = Constants.AnimTurning;
                }
                else
                {
                    anim = (_sprinting && _optionalAnimations.Contains(Constants.AnimRunning)) 
                        ? Constants.AnimRunning 
                        : Constants.AnimWalking;
                }
                speed = XSpeed / MaxSpeedWhenRunning;
            }
            else
            {
                anim = Constants.AnimStopped;
            }
        }
        return (anim, speed);
    }

    private bool IsInSpecialAnimation()
    {
        return _currentSprite != null && Constants.SpecialAnimations.Contains(_currentSprite.Animation);
    }

    private void TrySwitchStatusSprite()
    {
        if (_currentStatus != GlobalData.Status)
        {
            _currentStatus = GlobalData.Status;
            _currentSprite = SpriteNodes[_currentStatus];
            PostSwitchStatusSprite();
        }
        if (_currentSprite is Node sprite && sprite.GetParent() is null)
        {
            foreach (var child in PossibleSprites)
            {
                child.GetParent()?.CallDeferred(Node.MethodName.RemoveChild, child.AsNode());
            }
            (sprite is Node3D ? (Node)_sprite3DRoot : _spriteRoot).AddChild(sprite);
        }
    }

    private IEnumerable<IAnimatedSprite> PossibleSprites =>
        _spriteRoot.Children().Concat(_sprite3DRoot.Children()).OfType<IAnimatedSprite>();

    private void PostSwitchStatusSprite()
    {
        if (!_invulnerable)
        {
            _spriteRoot.Modulate = Colors.White;
        }

        _optionalAnimations.Clear();
        if (_currentSprite is AnimatedSprite2D sprite)
        {
            var frames = sprite.SpriteFrames;
            foreach (var name in Constants.OptionalAnimations)
            {
                if (frames.HasAnimation(name))
                {
                    _optionalAnimations.Add(name);
                }
            }
        }
        else
        {
            _optionalAnimations.AddAll(Constants.OptionalAnimations);
        }

        var is3D = _currentSprite is Node3D;
        _sprite3DRoot.Visible = is3D;
        _sprite3DRoot.ProcessMode = is3D ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;

        if (_spriteRoot.Material is ShaderMaterial sm)
        {
            sm.SetShaderParameter("outline_width", _currentSprite is Node3D ? 4F : 0F);
        }
        
        _standingSize = _currentStatus.Size;
        if (!(_standingSize == MarioSize.Big && _crouching))
        {
            SetSize(_standingSize);
        }
    }

    public override void _Input(InputEvent e)
    {
        base._Input(e);
        FetchInput(ref _jumpPressed, e, Constants.ActionJump);
        FetchInput(ref _runPressed, e, Constants.ActionRun);
        FetchInput(ref _upPressed, e, Constants.ActionMoveUp);
        FetchInput(ref _downPressed, e, Constants.ActionMoveDown);
        if (e.IsActionPressed(Constants.ActionFire))
        {
            _firePreInput = 0.2F;
            TryFire();
        }
    }

    protected override void Dispose(bool disposing)
    {
        foreach (var sprite in SpriteNodes.Values.Cast<Node>().Where(it => IsInstanceValid(it) && it.GetParent() == null))
        {
            sprite.QueueFree();
        }
        base.Dispose(disposing);
    }

    private static void FetchInput(ref bool pressed, InputEvent e, StringName action)
    {
        if (e.IsActionReleased(action))
        {
            pressed = false;
        }
        if (e.IsActionPressed(action))
        {
            pressed = true;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _spriteRoot, Constants.NpSpriteRoot);
        this.GetNode(out _sprite3DRoot, Constants.NpSprite3DRoot);
        this.GetNode(out _jumpSound, Constants.NpJumpSound);
        this.GetNode(out _swimSound, Constants.NpSwimSound);
        this.GetNode(out _hurtSound, Constants.NpHurtSound);
        this.GetNode(out _skidSound, Constants.NpSkidSound);
        this.GetNode(out _sprintSmokeTimer, Constants.NpSmkTimer);
        this.GetNode(out _skidSmokeTimer, Constants.NpSkdTimer);
        GameRule = this.GetRule();

        _runPressed = Input.IsActionPressed(Constants.ActionRun);
        _sprintSmokeTimer.Timeout += () => EmitSmoke(Constants.SprintSmoke);
        _skidSmokeTimer.Timeout += () => EmitSmoke(Constants.SkidSmoke);

        var statuses = StatusList.Count;
        for (var i = 0; i < statuses; i++)
        {
            var status = StatusList[i];
            var spriteMaybe = (i >= StatusSpriteNodeList.Count ? null : StatusSpriteNodeList[i]) as IAnimatedSprite;
            var sprite = spriteMaybe ?? status.AnimationNode?.Instantiate<IAnimatedSprite>();
            if (sprite is not null)
            {
                InstallStatusSprite(sprite);
            }
            SpriteNodes[status] = sprite;
        }
        
        RpgReady();
        Translate(new Vector2(0, -SafeMargin / 2));
    }

    private static void InstallStatusSprite(IAnimatedSprite sprite)
    {
        sprite.GetParent()?.RemoveChild(sprite.AsNode());
        sprite.AnimationFinished += _ =>
        {
            sprite.Animation = Constants.AnimStopped;
            sprite.Play();
        };
    }
    
    private void EmitSmoke(PackedScene smoke)
    {
        if (_isInAir || !this.TryGetParent(out Node parent)) return;
        smoke.Instantiate(out Node2D instance);
        
        parent.AddChild(instance);
        instance.GlobalPosition = GlobalPosition;
    }

    private CollisionShape2D CurrentCollisionShape
    {
        get => CollisionBySize[(int)_currentSize];
        set => CollisionBySize[(int)_currentSize] = value;
    }

    private Node2D _spriteRoot;
    private Node3D _sprite3DRoot;
    private AudioStreamPlayer _jumpSound;
    private AudioStreamPlayer _swimSound;
    private AudioStreamPlayer _hurtSound;
    private AudioStreamPlayer _skidSound;
    private MarioCollisionBySize _hurtZone;
    private MarioCollisionBySize _deathZone;
    private Timer _sprintSmokeTimer;
    private Timer _skidSmokeTimer;

    [CtfFlag(2)] private bool _crouching;
    [CtfFlag(12)] private bool _isInWater;
    private bool _isNearWaterSurface;
    [CtfFlag(28)] private bool _completedLevel;
    private bool _skidding;
    private float _firePreInput;

    private MarioStatus _currentStatus;
    private IAnimatedSprite _currentSprite;
    private Camera2D _camera;

    /// <summary>
    /// 不受是否处于下蹲状态影响
    /// </summary>
    private MarioSize _standingSize;
    
    /// <summary>
    /// 受下蹲影响
    /// </summary>
    private MarioSize _currentSize;

    private readonly HashSet<StringName> _optionalAnimations = new();
    private System.Collections.Generic.Dictionary<MarioStatus, IAnimatedSprite> SpriteNodes { get; } = new();
}
