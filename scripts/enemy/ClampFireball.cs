namespace ChloePrime.MarioForever.Enemy;

public partial class ClampFireball : Fireball
{
    protected ClampFireball()
    {
    }
    
    public GameRule GameRule { get; set; }

    public override void _PhysicsProcess(double deltaD)
    {
        var delta = (float)deltaD;
        YSpeed += Gravity * delta;
        Velocity = VelocityVector;
        MoveAndSlide();
        _ProcessCollision(delta);
    }

    protected override void _ProcessCollision(float delta)
    {
        var rule = GameRule ??= this.GetRule();
        if (rule.ClampFireballExplodeOnWallHit && GetSlideCollisionCount() > 0)
        {
            Explode(ExplodeFlags.WithDefaultSound);
        }
    }
}