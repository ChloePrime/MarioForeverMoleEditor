namespace ChloePrime.MarioForever.RPG;

public interface IMarioForeverNpc
{
    public MarioForeverNpcData NpcData { get; }

    public float HitPoint
    {
        get => NpcData.HitPoint;
        set => AlterHitPoint(value - HitPoint);
    }

    public void AlterHitPoint(float amount) => NpcData.HitPoint += amount;
}