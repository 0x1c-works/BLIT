using ImageMagick;
using System.Text;
using System.Xml;

namespace BannerlordImageTool.Banner;

public record IconSprite(int GroupID, int IconID, string RawPath, bool AlwaysLoad = true);

public class SpriteOrganizer
{
    static readonly string SPRITE_SUB_FOLDER = Path.Join("GUI", "SpriteParts");

    public static async Task CollectToSpriteParts(string outDir, IEnumerable<IconSprite> icons)
    {
        if (!string.IsNullOrEmpty(outDir))
        {
            outDir = Directory.CreateDirectory(outDir).FullName;
        }

        await Task.WhenAll(icons.Select(icon => ResizeAndSave(outDir, icon)));
        GenerateConfigXML(outDir, icons);
    }
    public static void GenerateConfigXML(string outDir, IEnumerable<IconSprite> icons)
    {
        var doc = new XmlDocument();
        XmlElement root = doc.CreateElement("Config");
        doc.AppendChild(root);
        foreach (IconSprite? icon in icons.Where(icon => icon.AlwaysLoad).DistinctBy(icon => icon.GroupID))
        {
            XmlElement node = doc.CreateElement("SpriteCategory");
            node.SetAttribute("Name", GetAtlasID(icon.GroupID));
            node.AppendChild(doc.CreateElement("AlwaysLoad"));
            root.AppendChild(node);
        }
        using var writer = XmlWriter.Create(
            Path.Join(EnsureSpriteFolder(outDir), "Config.xml"),
            new XmlWriterSettings() {
                Encoding = Encoding.UTF8,
                Indent = true,
            });
        doc.WriteTo(writer);
    }

    static string EnsureSpriteFolder(string outDir)
    {
        var dir = Path.Join(outDir, SPRITE_SUB_FOLDER);
        return Directory.CreateDirectory(dir).FullName;
    }

    static string EnsureGroupFolder(string outDir, int groupID)
    {
        var dir = Path.Join(EnsureSpriteFolder(outDir), GetAtlasID(groupID));
        return Directory.CreateDirectory(dir).FullName;
    }

    static async Task ResizeAndSave(string outDir, IconSprite icon)
    {
        (var groupID, var iconID, var filePath, var _) = icon;
        var outPath = Path.Join(EnsureGroupFolder(outDir, groupID), $"{iconID}.png");
        using var img = new MagickImage(filePath);
        if (img.Width != 512 && img.Height != 512)
        {
            img.Resize(new MagickGeometry(512));
        }
        await img.WriteAsync(outPath);
    }

    static string GetAtlasID(int groupID)
    {
        return $"ui_{groupID}";
    }
}
