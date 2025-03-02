using System.Threading.Tasks;
using ChloePrime.Godot.Util;
using Godot;

namespace ChloePrime.MarioForever.Tool;

public partial class ImageResizerBase : Sprite2D
{
    [Export] public TextEdit NameInput { get; private set; }
    [Export] public AudioStream SuccessSound { get; private set; }
    
    public override async void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsPressed() && @event is InputEventKey { PhysicalKeycode: Key.F2 })
        {
            SaveImage(await CaptureImage());
        }
    }

    protected virtual ValueTask<Image> CaptureImage()
    {
        return ValueTask.FromResult(Texture.GetImage());
    }

    private void SaveImage(Image image)
    {
        var result = image.SavePng($"./{NameInput?.Text}_2x.png");
        if (result is Error.Ok)
        {
            SuccessSound?.Play();
        }
    }
}
