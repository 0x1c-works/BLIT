using ImageMagick;

namespace BannerlordImageTool.Sprite;

public class SpriteUnpacker
{
    public void UnpackSingle(string spriteSheet, string outputFile, SpriteRegion region)
    {
        if (string.IsNullOrEmpty(outputFile)) throw new ArgumentNullException("outputFile");
        var settings = new MagickReadSettings() { ExtractArea = region.ToGeometry() };
        using (var sprite = new MagickImage(spriteSheet, settings))
        {
            sprite.Write(outputFile);
        }
    }
}
public record SpriteRegion(int X, int Y, int Width, int Height)
{
    public static SpriteRegion FromString(string args)
    {
        var parts = args.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
        {
            throw new ArgumentException("invalid sprite region. should be in the format of x,y,width,height");
        }
        if (!int.TryParse(parts[0], out var x))
        {
            throw new ArgumentException($"invalid sprite x: {parts[0]}");
        }
        if (!int.TryParse(parts[1], out var y))
        {
            throw new ArgumentException($"invalid sprite y: {parts[1]}");
        }
        if (!int.TryParse(parts[2], out var w))
        {
            throw new ArgumentException($"invalid sprite w: {parts[2]}");
        }
        if (!int.TryParse(parts[3], out var h))
        {
            throw new ArgumentException($"invalid sprite h: {parts[3]}");
        }
        return new SpriteRegion(x, y, w, h);
    }
    public MagickGeometry ToGeometry() => new MagickGeometry(X, Y, Width, Height);
}