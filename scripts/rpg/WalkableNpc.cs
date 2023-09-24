using ChloePrime.MarioForever.Enemy;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.RPG;

/// <summary>
/// 一个非玩家角色，可以是敌人，也可以是友军，甚至可以对话
/// </summary>
[GlobalClass]
public partial class WalkableNpc : WalkableObjectBase, IMarioForeverNpc
{
    [Export] public MarioForeverNpcData NpcData { get; private set; }

    public override bool CanMove => !NpcData.DoNotMove && base.CanMove;
    public override bool AutoDestroy => !NpcData.DoNotMove && !NpcData.Friendly;

    public override void _Ready()
    {
        base._Ready();
        NpcData = NpcData.ForceLocalToScene();
    }
}