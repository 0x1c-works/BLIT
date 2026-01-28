using OpenCCNET;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BLIT.Banner;

public class BannerIconData {
    private const string XML_FILE_NAME = "banner_icons.xml";

    [XmlArrayItem("Color")] public List<BannerColor> BannerColors = new();

    [XmlElement("BannerIconGroup")] public List<BannerIconGroup> IconGroups = new();

    public void SaveToXml(string outDir) {
        var serializer = new XmlSerializer(typeof(XmlDoc));
        if (!string.IsNullOrEmpty(outDir)) {
            outDir = Directory.CreateDirectory(outDir).FullName;
        }

        // 调试：打印当前工作目录
        var currentDir = Directory.GetCurrentDirectory();
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        Debug.WriteLine($"Current Working Directory: {currentDir}");
        Debug.WriteLine($"AppDomain BaseDirectory: {baseDir}");
        Debug.WriteLine($"Assembly Directory: {assemblyDir}");

        ZhConverter.Initialize(
            Path.Combine(baseDir, "Dictionary"),
            Path.Combine(baseDir, "JiebaResource")
        );

        // 序列化到内存流
        using (var memoryStream = new MemoryStream()) {
            serializer.Serialize(memoryStream, new XmlDoc { BannerIconData = this });
            memoryStream.Position = 0;

            // 加载为 XDocument
            XDocument doc = XDocument.Load(memoryStream);

            // 遍历所有 Icon 元素，添加 comment 并移除 comment attribute
            List<XElement> iconElements = doc.Descendants("Icon").ToList();
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

    #region Nested type: XmlDoc

    [XmlRoot("base")]
    public class XmlDoc {
        public BannerIconData BannerIconData = new();
    }

    #endregion
}

public class BannerIconGroup {
    [XmlElement("Icon")] public List<BannerIcon> Icons = new();

    [XmlAttribute("id")] public int ID;

    [XmlAttribute("is_pattern")] public bool IsPattern;

    [XmlAttribute("name")] public string Name = "";
}

public struct BannerIcon {
    [XmlAttribute("id")] public int ID;
    [XmlAttribute("material_name")] public string MaterialName;
    [XmlAttribute("texture_index")] public int TextureIndex;
    [XmlAttribute("comment")] public string Comment;
}

public record BannerColor {
    [XmlAttribute("hex")] public string Hex = "0xFFFFFFFF";
    [XmlAttribute("id")] public int ID;

    [XmlAttribute("player_can_choose_for_background")]
    public bool PlayerCanChooseForBackground = true;

    [XmlAttribute("player_can_choose_for_sigil")]
    public bool PlayerCanChooseForSigil = true;
}