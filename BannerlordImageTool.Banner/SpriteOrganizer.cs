using ImageMagick;

namespace BannerlordImageTool.Banner;

public record IconSprite(int GroupID, int IconID, string RawPath);

public class SpriteOrganizer
{
    static readonly string SPRITE_SUB_FOLDER = Path.Join("GUI", "SpriteParts");

    public static async Task CollectToSpriteParts(string outDir, IEnumerable<IconSprite> icons)
    {
        if (!string.IsNullOrEmpty(outDir))
        {
            Directory.CreateDirectory(outDir);
        }

        await Task.WhenAll(icons.Select(icon => ResizeAndSave(outDir, icon)));
    }

    static string EnsureOutFolder(string outDir, int groupID)
    {
        var dir = Path.Join(outDir, SPRITE_SUB_FOLDER, $"ui_{groupID}");
        Directory.CreateDirectory(dir);
        return dir;
    }

    async static Task ResizeAndSave(string outDir, IconSprite icon)
    {
        var (groupID, iconID, filePath) = icon;
        var outPath = Path.Join(EnsureOutFolder(outDir, groupID), $"{iconID}.png");
        using (var img = new MagickImage(filePath))
        {
            if (img.Width != 512 && img.Height != 512)
            {
                img.Resize(new MagickGeometry(512));
            }
            await img.WriteAsync(outPath);
        }
    }
}
