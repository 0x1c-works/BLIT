using System.Xml.Serialization;

namespace BannerlordImageTool.BannerTex;

public class BannerIconData
{
    const string XML_FILE_NAME = "banner_icons.xml";

    [XmlElement("BannerIconGroup")]
    public List<BannerIconGroup> IconGroups = new();
    [XmlArrayItem("Color")]
    public List<BannerColor> BannerColors = new();

    public void SaveToXml(string outDir)
    {
        var serializer = new XmlSerializer(typeof(XmlDoc));
        if (!string.IsNullOrEmpty(outDir))
        {
            Directory.CreateDirectory(outDir);
        }

        using var writer = new FileStream(Path.Join(outDir, XML_FILE_NAME), FileMode.Create);
        serializer.Serialize(writer, new XmlDoc { BannerIconData = this });
    }
    [XmlRoot("base")]
    public class XmlDoc
    {
        public BannerIconData BannerIconData = new();
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

    [XmlElement("Icon")]
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
