using System.IO;
using System.Threading.Tasks;
using ChloePrime.Godot.Util;
using Godot;

namespace ChloePrime.MarioForever.Tool;

public partial class ImageResizerBase : Sprite2D
{
    [Export] public TextEdit NameInput { get; private set; }
    [Export] public AudioStream SuccessSound { get; private set; }
    
    [ExportGroup("Advanced")]
    [Export] public Sprite2D SourceSprite { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        GetWindow().FilesDropped += OnWindowFileDropped;
    }

    private async void OnWindowFileDropped(string[] files)
    {
        foreach (var file in files)
        {
            if (Image.LoadFromFile(file) is not { } image)
            {
                continue;
            }
            SourceSprite.Texture = ImageTexture.CreateFromImage(image);
            var fd = new FileInfo(file);
            SaveImage(await CaptureImage(), fd.DirectoryName + "/" + fd.Name.TrimSuffix(fd.Extension) + "_2x.png");
        }
    }
    
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

    protected void SaveImage(Image image, string path)
    {
        var result = image.SavePng(path);
        if (result is Error.Ok)
        {
            SuccessSound?.Play();
        }
    }

    private void SaveImage(Image image)
    {
        SaveImage(image, $"./{NameInput?.Text}_2x.png");
    }
}
