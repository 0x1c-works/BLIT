using ColorHelper;
using MessagePack;
using MessagePack.Formatters;
using System.Windows.Media;
using ColorConverter = ColorHelper.ColorConverter;

namespace BLIT.Helpers;

public class ColorMessagePackFormatter : IMessagePackFormatter<Color>
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

public static class BLITColorHelper
{
    public static RGB ToRGB(this Color color) => new RGB(color.R, color.G, color.B);
    public static int Sort(Color x, Color y)
    {
        HSV hsv1 = ColorConverter.RgbToHsv(x.ToRGB());
        HSV hsv2 = ColorConverter.RgbToHsv(y.ToRGB());

        if (hsv1.H == 360) hsv1.H = 0;
        if (hsv2.H == 360) hsv2.H = 0;

        var deltaH = hsv1.H - hsv2.H;
        var deltaS = hsv1.S - hsv2.S;
        var deltaV = hsv1.V - hsv2.V;

        // for greyscale, sort from white to black
        if (hsv1.S == 0 && hsv2.S == 0)
        {
            return deltaV == 1 ? -1 : deltaV == 0 ? 1 : deltaV > 0 ? -1 : 1;
        }
        // greyscale always is at the start
        if (hsv1.S == 0) return -1;
        if (hsv2.S == 0) return 1;

        // For normal colors, sort by H (inc) > S (desc) > V (desc)
        if (deltaH != 0) return deltaH > 0 ? 1 : -1;
        if (deltaS != 0) return deltaS > 0 ? -1 : 1;
        if (deltaV != 0) return deltaV > 0 ? -1 : 1;

        return 0;
    }
}
