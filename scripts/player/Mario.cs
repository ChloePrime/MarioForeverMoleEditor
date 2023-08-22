using System.Linq;
using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Util;
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
    [Export] public float MinSpeed { get; set; } = Units.Speed.CtfMovementToGd(8);
    [Export] public float AccelerationWhenWalking { get; set; } = Units.Acceleration.CtfMovementToGd(1);
    [Export] public float AccelerationWhenRunning { get; set; } = Units.Acceleration.CtfMovementToGd(1);
    [Export] public float AccelerationWhenTurning { get; set; } = Units.Acceleration.CtfMovementToGd(4);
    [Export] public bool AllowMoveOutOfScreen { get; set; }
    [Export] public float ScreenBorderPadding { get; set; } = 16;
    
    
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
    [Export] public bool FastRetry { get; set; }
    [Export] public Array<MarioStatus> StatusList { get; set; }
    [Export] public Array<Node> StatusSpriteNodeList { get; set; }

    [ExportGroup("")]
    [Export] public float InvulnerabilityFlashSpeed { get; set; } = 8;

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
        YSpeed = -strength;
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
        if (_invulnerable && _currentSprite != null)
        {
            _invulnerableFlashPhase = (_invulnerableFlashPhase + InvulnerabilityFlashSpeed * (float)delta) % 1;
            var alpha = Mathf.Cos(2 * Mathf.Pi * _invulnerableFlashPhase);
            _currentSprite.Modulate = new Color(Colors.White, alpha);
        }
        else if (_invulnerableFlashPhase != 0)
        {
            _invulnerableFlashPhase = 0;
            if (_currentSprite != null)
            {
                _currentSprite.Modulate = Colors.White;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_hurtStack > 0)
        {
            Hurt();
        }
        if (GlobalPosition.Y > this.GetFrame().End.Y + 48)
        {
            Kill();
            return;
        }
        PhysicsProcessX(delta);
        PhysicsProcessY(delta);
        UpdateCrouch();
        UpdateAnimation();
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
            CollisionBySize[i].Disabled = i != (int)size;
        }
        _hurtZone.SetSize(size);
        _deathZone.SetSize(size);
        _currentSize = size;

        if (size != MarioSize.Big && _crouching)
        {
            _crouching = false;
        }
    }

    private void UpdateAnimation()
    {
        TrySwitchStatusSprite();
        _spriteRoot.Scale = _xDirection < 0 ? Constants.FlipX : Constants.DoNotFlipX;
        
        var (anim, speed) = GetAnimationIdAndSpeed();
        if (_currentSprite.Animation != anim)
        {
            _currentSprite.Animation = anim;
        }
        _currentSprite.SpeedScale = speed;
    }

    private (StringName, float) GetAnimationIdAndSpeed()
    {
        StringName anim;
        var speed = 1F;
        if (_crouching && _hasCrouchingAnimation)
        {
            return (Constants.AnimCrouching, speed);
        }
        if (_isInAir)
        {
            anim = _isInWater
                ? Constants.AnimSwimming
                : ((YSpeed >= 0 && _hasFallingAnimation) ? Constants.AnimFalling : Constants.AnimJumping);
        }
        else
        {
            if (_xSpeed > 0)
            {
                anim = Constants.AnimWalking;
                speed = _xSpeed / MaxSpeedWhenRunning;
            }
            else
            {
                anim = Constants.AnimStopped;
            }
        }
        return (anim, speed);
    }

    private void TrySwitchStatusSprite()
    {
        if (_currentStatus != GlobalData.Status)
        {
            _currentStatus = GlobalData.Status;
            _currentSprite = SpriteNodes[_currentStatus];
            PostSwitchStatusSprite();
        }
        if (_currentSprite != null && _currentSprite.GetParent() == null)
        {
            foreach (var child in _spriteRoot.Children())
            {
                _spriteRoot.CallDeferred(Node.MethodName.RemoveChild, child);
            }
            _spriteRoot.AddChild(_currentSprite);
        }
    }

    private void PostSwitchStatusSprite()
    {
        _hasFallingAnimation = _currentSprite.SpriteFrames.HasAnimation(Constants.AnimFalling);
        _hasCrouchingAnimation = _currentSprite.SpriteFrames.HasAnimation(Constants.AnimCrouching);
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
    }

    protected override void Dispose(bool disposing)
    {
        foreach (var sprite in SpriteNodes.Values.Where(it => IsInstanceValid(it) && it.GetParent() == null))
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
        this.GetNode(out _hurtZone, Constants.NpHurtZone);
        this.GetNode(out _deathZone, Constants.NpDeathZone);
        this.GetNode(out _jumpSound, Constants.NpJumpSound);
        this.GetNode(out _swimSound, Constants.NpSwimSound);
        this.GetNode(out _hurtSound, Constants.NpHurtSound);

        _runPressed = Input.IsActionPressed(Constants.ActionRun);

        _hurtZone.BodyEntered += _ => _hurtStack++;
        _hurtZone.BodyExited += _ => _hurtStack--;
        _deathZone.BodyEntered += _ => Kill();

        var statuses = StatusList.Count;
        for (var i = 0; i < statuses; i++)
        {
            var status = StatusList[i];
            var spriteMaybe = (i >= StatusSpriteNodeList.Count ? null : StatusSpriteNodeList[i]) as AnimatedSprite2D;
            var sprite = spriteMaybe ?? status.AnimationNode.Instantiate<AnimatedSprite2D>();
            InstallStatusSprite(sprite);
            SpriteNodes[status] = sprite;
        }
    }

    private static void InstallStatusSprite(AnimatedSprite2D sprite)
    {
        sprite?.GetParent()?.RemoveChild(sprite);
    }

    private CollisionShape2D CurrentCollisionShape
    {
        get => CollisionBySize[(int)_currentSize];
        set => CollisionBySize[(int)_currentSize] = value;
    }

    private Node2D _spriteRoot;
    private AudioStreamPlayer _jumpSound;
    private AudioStreamPlayer _swimSound;
    private AudioStreamPlayer _hurtSound;
    private MarioCollisionBySize _hurtZone;
    private MarioCollisionBySize _deathZone;

    [CtfFlag(2)] private bool _crouching;
    [CtfFlag(12)] private bool _isInWater;
    private bool _isNearWaterSurface;
    [CtfFlag(28)] private bool _completedLevel;

    private MarioStatus _currentStatus;
    private AnimatedSprite2D _currentSprite;
    private Camera2D _camera;
    
    /// <summary>
    /// 不受是否处于下蹲状态影响
    /// </summary>
    private MarioSize _standingSize;
    
    /// <summary>
    /// 受下蹲影响
    /// </summary>
    private MarioSize _currentSize;
    private bool _hasFallingAnimation;
    private bool _hasCrouchingAnimation;
    private System.Collections.Generic.Dictionary<MarioStatus, AnimatedSprite2D> SpriteNodes { get; } = new();
}
