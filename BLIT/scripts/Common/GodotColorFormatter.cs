using Godot;
using MessagePack;
using MessagePack.Formatters;

namespace BLIT.scripts.Common;
public class GodotColorFormatter : IMessagePackFormatter<Color> {
    public Color Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
        var a = ToFloat(reader.ReadByte());
        var r = ToFloat(reader.ReadByte());
        var g = ToFloat(reader.ReadByte());
        var b = ToFloat(reader.ReadByte());
        return new Color(r, g, b, a);
    }

    public void Serialize(ref MessagePackWriter writer, Color value, MessagePackSerializerOptions options) {
        writer.Write(ToInt(value.A));
        writer.Write(ToInt(value.R));
        writer.Write(ToInt(value.G));
        writer.Write(ToInt(value.B));
    }

    private static int ToInt(float value) {
        return (int)(value * 255);
    }

    private static float ToFloat(int value) {
        return value / 255f;
    }
}

