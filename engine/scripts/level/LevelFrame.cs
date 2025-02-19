namespace ChloePrime.MarioForever.Level;

public partial class LevelFrame : LevelManager
{
    public override void _Ready()
    {
        Main.Init();
        base._Ready();
    }
}