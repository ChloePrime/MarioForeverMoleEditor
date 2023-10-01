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
    public static float GetIdealHeight(this MarioSize size) => IdealHeightTable[(int)size];
    private static readonly float[] IdealWidthTable = { 32, 32, 16 };
    private static readonly float[] IdealHeightTable = { 32, 64, 16 };
}