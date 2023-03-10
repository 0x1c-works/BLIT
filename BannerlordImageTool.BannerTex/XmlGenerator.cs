using System.Xml.Serialization;

namespace BannerlordImageTool.BannerTex;

public class XmlGenerator
{
    public static void GenerateXml(BannerIconData bannerIconData, string outFilePath)
    {
        var serializer = new XmlSerializer(typeof(BannerIconData));
        var outDir = Path.GetDirectoryName(outFilePath);
        Directory.CreateDirectory(outDir);

        using var writer = new FileStream(outFilePath, FileMode.Create);
        serializer.Serialize(writer, bannerIconData);
    }
}

