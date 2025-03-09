using System.Threading.Tasks;
using ChloePrime.Godot.Util;
using Godot;

namespace ChloePrime.MarioForever.Tool;

public partial class TileImageResizer : ImageResizerBase
{
    [ExportGroup("Advanced")]
    [Export] public TileImageResizerFormat PrepareFormat { get; private set; }
    [Export] public TileImageResizerFormat ExportFormat { get; private set; }

    protected override async ValueTask<Image> CaptureImage()
    {
        var srcImage = SourceSprite.Texture.GetImage();
        // 这张图的分辨率是16x的
        var intermediateImage = Image.CreateEmpty(320, 240, false, Image.Format.Rgba8);
        
        // 把原始tile做成中间格式以方便完美衔接
        foreach (Vector4I remap in PrepareFormat.Entries)
        {
            var srcRegion = TileCoordToImageArea(16, 1, remap.X, remap.Y);
            var dstRegion = TileCoordToImageArea(16, 0, remap.Z, remap.W);
            intermediateImage.BlitRect(srcImage, srcRegion, dstRegion.Position);
        }

        SourceSprite.Texture = ImageTexture.CreateFromImage(intermediateImage);
        await this.DelayAsync(0.25F);
        var captured = await base.CaptureImage();
        var result = Image.CreateEmpty(236, 270, false, Image.Format.Rgba8);
        
        // 从放大后的中间格式中扣出合适的块做成放大后的Tile
        foreach (Vector4I remap in ExportFormat.Entries)
        {
            var srcRegion = TileCoordToImageArea(32, 0, remap.X, remap.Y);
            var dstRegion = TileCoordToImageArea(32, 2, remap.Z, remap.W);
            result.BlitRect(captured, srcRegion, dstRegion.Position);
        }
        
        return result;
    }

    private static Rect2I TileCoordToImageArea(int unitSize, int spacing, int tileX, int tileY)
    {
        var position = (unitSize + spacing) * new Vector2I(tileX, tileY);
        return new Rect2I(position, new Vector2I(unitSize, unitSize));
    }
}