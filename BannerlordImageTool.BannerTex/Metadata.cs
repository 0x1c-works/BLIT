using System.Xml.Serialization;

namespace BannerlordImageTool.BannerTex;

[XmlRoot("base")]
public class BannerIconData
{
    [XmlElement]
    public List<BannerIconGroup> IconGroups = new();
    [XmlArrayItem("Color")]
    public List<BannerColor> Colors = new();
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

public record BannerIcon(
[XmlAttribute("id")] int ID,
[XmlAttribute("material_name")] string MaterialName,
[XmlAttribute("texture_index")] int TextureIndex);

public record BannerColor(
[XmlAttribute("id")] int ID,
[XmlAttribute("hex")] string Hex,
[XmlAttribute("player_can_choose_for_sigil")] bool PlayerCanChooseForSigil = true,
[XmlAttribute("player_can_choose_for_background")] bool PlayerCanChooseForBackground = true);
