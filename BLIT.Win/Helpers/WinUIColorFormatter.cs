using MessagePack;
using MessagePack.Formatters;
using Windows.UI;

namespace BLIT.Win.Helpers;

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
