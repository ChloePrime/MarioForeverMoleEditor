using System.Linq;
using System.Text.RegularExpressions;
using Godot;

namespace ChloePrime.MarioForever.UI;

public partial class WorldName : Label
{
    [Export] public Theme ThemeForLongText { get; set; }

    public void SetText(string text)
    {
        Text = text;
        Theme = IsSimpleText(text) ? _originalTheme : ThemeForLongText;
    }

    public override void _Ready()
    {
        base._Ready();
        _originalTheme = Theme;
        SetText(Text);
    }

    private static bool IsSimpleText(string text)
    {
        return RegSimpleWorldName.IsMatch(text) || (text.Split("\n").All(s => s.Length < 8) && text.All(char.IsAscii));
    }
    
    private static readonly Regex RegSimpleWorldName = RegSimpleWorldNameFactory();
    private Theme _originalTheme;

    [GeneratedRegex("\\d*-\\d*")]
    private static partial Regex RegSimpleWorldNameFactory();
}