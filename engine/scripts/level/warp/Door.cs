using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Player;
using Godot;

namespace ChloePrime.MarioForever.Level.Warp;

public partial class Door : WarpObject
{
    [Export] public AudioStream EnterSound { get; set; } = GD.Load<AudioStream>("res://engine/resources/level/SE_door_open.ogg");
    [Export] public AudioStream ExitSound { get; set; } = GD.Load<AudioStream>("res://engine/resources/level/SE_door_close.ogg");

    public async void MarioEnterDoor(Mario mario)
    {
        if (mario.PipeState is not MarioPipeState.NotInPipe)
        {
            return;
        }
        EnterSound?.Play();
        mario.PipeState = MarioPipeState.Entering;
        mario.GlobalPosition = GlobalPosition;
        mario.ZIndexBeforePipe = mario.ZIndex;
        mario.PipeForceAnimation = Mario.Constants.AnimPipeBack;
        await this.DelayAsync(0.001F);
        mario.RequireTeleport += () => OnMarioRequireTeleport(mario);
        ApplyTransitionType(mario);
        mario.BeginWarpTransitionIn();
    }
    
    private static readonly NodePath NpHitbox = "Hitbox";
    private Mario _mario;
    private Area2D _hitbox;
    
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _hitbox, NpHitbox);
        _hitbox.BodyEntered += OnHitboxBodyEntered;
        _hitbox.BodyExited += OnHitboxBodyExited;
    }

    private void OnHitboxBodyEntered(Node2D body)
    {
        if (body is Mario m) _mario = m;
    }
    
    private void OnHitboxBodyExited(Node2D body)
    {
        if (body == _mario) _mario = null;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_mario is { IsInAir: false } mario && Input.IsActionJustPressed(Mario.Constants.ActionMoveUp))
        {
            mario.GlobalPosition = GlobalPosition;
            mario.XSpeed = mario.YSpeed = 0;
            Callable.From<Mario>(MarioEnterDoor).CallDeferred(mario);
        }
    }

    private void OnMarioRequireTeleport(Mario mario)
    {
        var target = Target;
        if (mario.TryTeleportTo(target))
        {
            target!.OnMarioTeleportedToHere(mario);
        }
        else
        {
            mario.GlobalPosition = GlobalPosition;
            MarioExitDoor(mario);
        }
    }

    public override void OnMarioTeleportedToHere(Mario mario)
    {
        base.OnMarioTeleportedToHere(mario);
        mario.Visible = true;
        mario.ZIndex = mario.ZIndexBeforePipe;
        mario.PipeForceAnimation = Mario.Constants.AnimPipeFrontYeah;
    }

    protected override void BeginExitingProcedure(Mario mario)
    {
        base.BeginExitingProcedure(mario);
        ExitSound?.Play();
        Callable.From<Mario>(MarioExitDoor).CallDeferred(mario);
    }

    private void MarioExitDoor(Mario mario)
    {
        MarioExitWarp(mario);
        mario.ForceCancelCrouch();
    }
}
