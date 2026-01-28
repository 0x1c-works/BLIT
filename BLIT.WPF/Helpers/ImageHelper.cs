using System.IO;

namespace BLIT.WPF.Helpers;

public static class ImageHelper {
    public static readonly string BAD_IMAGE_PATH =
        Path.GetFullPath("Assets/empty-asset.png", AppDomain.CurrentDomain.BaseDirectory);

    public static bool IsValidImage(string? path) {
        return !string.IsNullOrWhiteSpace(path) && path != BAD_IMAGE_PATH && File.Exists(path);
    }
}