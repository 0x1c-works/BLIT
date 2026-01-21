using MessagePack;
using MessagePack.Formatters;
using System.Windows.Media;

namespace BLIT.WPF.Helpers;

public class WPFColorFormatter : IMessagePackFormatter<Color> {
    public Color Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
        var a = reader.ReadByte();
        var r = reader.ReadByte();
        var g = reader.ReadByte();
        var b = reader.ReadByte();
        return Color.FromArgb(a, r, g, b);
    }

    public void Serialize(ref MessagePackWriter writer, Color value, MessagePackSerializerOptions options) {
        writer.Write(value.A);
        writer.Write(value.R);
        writer.Write(value.G);
        writer.Write(value.B);
    }
}
