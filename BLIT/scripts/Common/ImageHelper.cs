using System.IO;

namespace BLIT.scripts.Common;
public static class ImageHelper
{
    public static readonly string BAD_IMAGE_PATH = "res://assets/placeholder.png";
    public static bool IsValidImage(string path)
    {
        return !string.IsNullOrWhiteSpace(path) && path != BAD_IMAGE_PATH && File.Exists(path);
    }

}
