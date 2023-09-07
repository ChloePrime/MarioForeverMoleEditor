using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Util;
using DotNext;
using Godot;

namespace ChloePrime.MarioForever.RPG;

/// <summary>
/// 一个非玩家角色，可以是敌人，也可以是友军，甚至可以对话
/// </summary>
[GlobalClass]
public partial class WalkableNpc : WalkableObjectBase, IMarioForeverNpc
{
    [Export] public MarioForeverNpcData NpcData { get; private set; }

    public override void _PhysicsProcess(double delta)
    {
        if (NpcData.DoNotMove) return;
        base._PhysicsProcess(delta);
    }
}