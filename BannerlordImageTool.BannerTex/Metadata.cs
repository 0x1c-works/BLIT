using System.Xml.Serialization;

namespace BannerlordImageTool.BannerTex;

[XmlRoot("base")]
public class BannerIconData
{
    const string XML_FILE_NAME = "banner_icons.xml";

    [XmlElement]
    public List<BannerIconGroup> IconGroups = new();
    [XmlArrayItem("Color")]
    public List<BannerColor> Colors = new();

    public void SaveToXml(string outDir)
    {
        var serializer = new XmlSerializer(typeof(BannerIconData));
        if (!string.IsNullOrEmpty(outDir))
        {
            Directory.CreateDirectory(outDir);
        }

        using var writer = new FileStream(Path.Join(outDir, XML_FILE_NAME), FileMode.Create);
        serializer.Serialize(writer, this);
    }
}

public class BannerIconGroup
{
    [XmlAttribute("id")]
    public int ID;
    [XmlAttribute("name")]
    public string Name = "";
    [XmlAttribute("is_pattern")]
    public bool IsPattern;

    [XmlArrayItem("Icon")]
    public List<BannerIcon> Icons = new();
}

public struct BannerIcon
{
    [XmlAttribute("id")] public int ID;
    [XmlAttribute("material_name")] public string MaterialName;
    [XmlAttribute("texture_index")] public int TextureIndex;
}

public record BannerColor
{
    [XmlAttribute("id")] public int ID;
    [XmlAttribute("hex")] public string Hex = "0xFFFFFFFF";
    [XmlAttribute("player_can_choose_for_sigil")] public bool PlayerCanChooseForSigil = true;
    [XmlAttribute("player_can_choose_for_background")] public bool PlayerCanChooseForBackground = true;
}
