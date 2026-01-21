using OpenCCNET;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BLIT.Banner;

public class BannerIconData {
    private const string XML_FILE_NAME = "banner_icons.xml";

    [XmlElement("BannerIconGroup")]
    public List<BannerIconGroup> IconGroups = new();
    [XmlArrayItem("Color")]
    public List<BannerColor> BannerColors = new();

    public void SaveToXml(string outDir) {
        var serializer = new XmlSerializer(typeof(XmlDoc));
        if (!string.IsNullOrEmpty(outDir)) {
            outDir = Directory.CreateDirectory(outDir).FullName;
        }

        // 调试：打印当前工作目录
        var currentDir = Directory.GetCurrentDirectory();
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var assemblyDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        System.Diagnostics.Debug.WriteLine($"Current Working Directory: {currentDir}");
        System.Diagnostics.Debug.WriteLine($"AppDomain BaseDirectory: {baseDir}");
        System.Diagnostics.Debug.WriteLine($"Assembly Directory: {assemblyDir}");

        ZhConverter.Initialize(
            dictionaryDirectory: Path.Combine(baseDir, "Dictionary"),
            jiebaResourceDirectory: Path.Combine(baseDir, "JiebaResource")
            );

        // 序列化到内存流
        using (var memoryStream = new MemoryStream()) {
            serializer.Serialize(memoryStream, new XmlDoc { BannerIconData = this });
            memoryStream.Position = 0;

            // 加载为 XDocument
            var doc = XDocument.Load(memoryStream);

            // 遍历所有 Icon 元素，添加 comment 并移除 comment attribute
            var iconElements = doc.Descendants("Icon").ToList();
            foreach (XElement? iconElement in iconElements) {
                XAttribute? commentAttr = iconElement.Attribute("comment");
                if (commentAttr != null && !string.IsNullOrEmpty(commentAttr.Value)) {
                    var oldValue = commentAttr.Value;
                    var finalValue = ZhConverter.HantToHans(oldValue);
                    if (finalValue != oldValue) {
                        finalValue = $"{finalValue}/{oldValue}";
                    }
                    // 在 Icon 元素前插入 comment
                    iconElement.AddBeforeSelf(new XComment(finalValue));
                    // 移除 comment attribute
                    commentAttr.Remove();
                }
            }

            // 保存到文件
            doc.Save(Path.Join(outDir, XML_FILE_NAME));
        }
    }
    [XmlRoot("base")]
    public class XmlDoc {
        public BannerIconData BannerIconData = new();
    }
}

public class BannerIconGroup {
    [XmlAttribute("id")]
    public int ID;
    [XmlAttribute("name")]
    public string Name = "";
    [XmlAttribute("is_pattern")]
    public bool IsPattern;

    [XmlElement("Icon")]
    public List<BannerIcon> Icons = new();
}

public struct BannerIcon {
    [XmlAttribute("id")] public int ID;
    [XmlAttribute("material_name")] public string MaterialName;
    [XmlAttribute("texture_index")] public int TextureIndex;
    [XmlAttribute("comment")] public string Comment;
}

public record BannerColor {
    [XmlAttribute("id")] public int ID;
    [XmlAttribute("hex")] public string Hex = "0xFFFFFFFF";
    [XmlAttribute("player_can_choose_for_sigil")] public bool PlayerCanChooseForSigil = true;
    [XmlAttribute("player_can_choose_for_background")] public bool PlayerCanChooseForBackground = true;
}
