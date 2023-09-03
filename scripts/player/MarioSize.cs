namespace ChloePrime.MarioForever.Player;

public enum MarioSize
{
    Small,
    Big,
    Mini
}

public static class MarioSizeEx
{
    public static float GetIdealWidth(this MarioSize size) => IdealWidthTable[(int)size];
    private static readonly float[] IdealWidthTable = { 32, 32, 16 };
}