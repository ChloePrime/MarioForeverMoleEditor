using System.IO;
using System.Threading.Tasks;
using ChloePrime.Godot.Util;
using Godot;

namespace ChloePrime.MarioForever.Tool;

public partial class SpriteImageResizer : ImageResizerBase
{
    public const int Margin = 8;
    
    [Export] public SubViewport ResultFrameBuffer { get; set; }
    [Export] public SubViewport PixelizedContent { get; set; }

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
            SaveImage(await CaptureImage(), fd.Directory + "/" + fd.Name.TrimSuffix(fd.Extension) + "_2x.png");
        }
    }

    protected override async ValueTask<Image> CaptureImage()
    {
        Vector2I size = (Vector2I) SourceSprite.Texture.GetSize() + new Vector2I(Margin, Margin);
        SourceSprite.Position = size / 2;
        PixelizedContent.Size2DOverride = size;
        PixelizedContent.Size = ResultFrameBuffer.Size = size * 2;
        await this.DelayAsync(0.25F);
        var captured = await base.CaptureImage();
        
        // 移除8px边距
        size *= 2;
        size -= new Vector2I(Margin, Margin);
        captured.Crop(size.X, size.Y);
        captured.Rotate180();
        size -= new Vector2I(Margin, Margin);
        captured.Crop(size.X, size.Y);
        captured.Rotate180();
        
        return captured;
    }
}
