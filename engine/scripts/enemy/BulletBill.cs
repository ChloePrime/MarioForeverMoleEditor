using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

public partial class BulletBill : Node2D
{
    [Export] public float Speed { get; set; } = Units.Speed.CtfMovementToGd(26);
    [Export] public float XDirection { get; set; }

    public void LookAtMario()
    {
        if (!IsInsideTree())
        {
            return;
        }
        LookAtMario0();
        GetNode<AnimatedSprite2D>(NpSprite).FlipH = XDirection < 0;
    }

    public override async void _EnterTree()
    {
        base._EnterTree();
        Callable.From(LookAtMario).CallDeferred();

        await ToSignal(GetTree().CreateTimer(0.2), SceneTreeTimer.SignalName.Timeout);
        if (IsInstanceValid(this))
        {
            ZIndex += 2;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Translate(new Vector2(Speed * XDirection * (float)delta, 0));
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        var frame = this.GetFrame();
        var x = GlobalPosition.X;
        if (x <= frame.Position.X - 1920 || x > frame.End.X + 1920)
        {
            QueueFree();
        }
    }

    private void LookAtMario0()
    {
        if (XDirection != 0 || GetTree().GetPlayer() is not Node2D mario)
        {
            return;
        }
        XDirection = Mathf.Sign(ToLocal(mario.GlobalPosition).X);
    }

    private static readonly NodePath NpSprite = "Enemy Core/Sprite";
}