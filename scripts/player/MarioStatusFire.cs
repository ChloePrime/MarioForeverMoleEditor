using ChloePrime.MarioForever.Enemy;
using Godot;
using MarioForeverMoleEditor.scripts.util;

namespace ChloePrime.MarioForever.Player;

public partial class MarioStatusFire : MarioStatus
{
    [Export]
    public PackedScene Fireball { get; set; } = GD.Load<PackedScene>("res://resources/mario/fireball_mario.tscn");

    [Export]
    public AudioStream FireSound { get; set; } = GD.Load<AudioStream>("res://resources/shared/SE_fireball.wav");

    [Export] public int MaxFireballs { get; set; } = 2;
    
    public static readonly StringName FireId = "fire";
    public override StringName GetId() => FireId;

    public override bool Fire(Mario mario)
    {
        base.Fire(mario);
        if (_fireballCount >= MaxFireballs)
        {
            return false;
        }
        // 播放音效
        if (FireSound is { } se)
        {
            mario.GetTree().Root.PlaySound(se);
        }
        // 发射火球
        var fireball = Fireball.Instantiate();
        if (mario.GetParent() is not { } environment)
        {
            return false;
        }
        environment.AddChild(fireball);
        if (fireball is Node2D fireball2D)
        {
            fireball2D.GlobalPosition = mario.Muzzle.GlobalPosition;
        }
        if (fireball is GravityObjectBase ball)
        {
            ball.XDirection = mario.XDirection;
        }
        if (fireball is Fireball fireball2)
        {
            fireball2.Shooter = mario;
        }
        // 火球统计
        _fireballCount++;
        fireball.TreeExited += () => _fireballCount--;
        return true;
    }

    private int _fireballCount;
}