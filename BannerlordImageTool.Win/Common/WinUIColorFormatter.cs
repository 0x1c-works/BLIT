using CommunityToolkit.WinUI.Helpers;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace BannerlordImageTool.Win.Common;

public class WinUIColorFormatter : IMessagePackFormatter<Color>
{
    public Color Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var a = reader.ReadByte();
        var r = reader.ReadByte();
        var g = reader.ReadByte();
        var b = reader.ReadByte();
        return Color.FromArgb(a, r, g, b);
    }

    public void Serialize(ref MessagePackWriter writer, Color value, MessagePackSerializerOptions options)
    {
        writer.Write(value.A);
        writer.Write(value.R);
        writer.Write(value.G);
        writer.Write(value.B);
    }
}
