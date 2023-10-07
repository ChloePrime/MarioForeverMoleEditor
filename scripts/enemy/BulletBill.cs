using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class BulletBill : Node2D
{
    [Export] public float Speed { get; set; } = Units.Speed.CtfMovementToGd(26);
    [Export] public float XDirection { get; set; }

    public void LookAtMario()
    {
        LookAtMario0();
        GetNode<AnimatedSprite2D>(NpSprite).FlipH = XDirection < 0;
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        CallDeferred(MethodName.LookAtMario);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Translate(new Vector2(Speed * XDirection * (float)delta, 0));
    }
    
    private void LookAtMario0()
    {
        if (XDirection != 0 || GetTree().GetFirstNodeInGroup(MaFo.Groups.Player) is not Node2D mario)
        {
            return;
        }
        XDirection = Mathf.Sign(ToLocal(mario.GlobalPosition).X);
    }

    private static readonly NodePath NpSprite = "Enemy Core/Sprite";
}