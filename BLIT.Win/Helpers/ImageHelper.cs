using System.IO;
using Windows.Storage;

namespace BLIT.Win.Helpers;
public static class ImageHelper {
    public static readonly string BAD_IMAGE_PATH = Path.GetFullPath("/Assets/empty-asset.png", ApplicationData.Current.LocalFolder.Path);
    public static bool IsValidImage(string path) {
        return !string.IsNullOrWhiteSpace(path) && path != BAD_IMAGE_PATH && File.Exists(path);
    }

}
