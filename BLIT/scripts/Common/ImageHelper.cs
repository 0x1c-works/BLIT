using Godot;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BLIT.scripts.Common;
public static class ImageHelper {
    public static readonly string BAD_IMAGE_PATH = "res://assets/placeholder.png";
    public static bool IsValidImage(string? path) {
        return !string.IsNullOrWhiteSpace(path) && path != BAD_IMAGE_PATH && File.Exists(path);
    }

    public static Task<ImageTexture?> LoadImage(string path, CancellationTokenSource? cancelSource) {
        cancelSource?.Cancel();
        cancelSource = new();
        return Task.Factory.StartNew(() => {
            if (!File.Exists(path)) return null;

            using var img = Image.LoadFromFile(path);
            return ImageTexture.CreateFromImage(img);
        }, cancelSource.Token);
    }

}
