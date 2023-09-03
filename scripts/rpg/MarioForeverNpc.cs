using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.RPG;

/// <summary>
/// 一个非玩家角色，可以是敌人，也可以是友军，甚至可以对话
/// </summary>
[GlobalClass]
public partial class MarioForeverNpc : WalkableObjectBase
{
    [Export] public float DamageLo { get; set; } = 2;
    [Export] public float DamageHi { get; set; } = 40;
    [Export] public bool Friendly { get; set; }
    [Export] public bool DoNotMove { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        if (DoNotMove) return;
        base._PhysicsProcess(delta);
    }
}