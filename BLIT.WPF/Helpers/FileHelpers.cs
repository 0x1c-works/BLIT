using System.Diagnostics;
using System.IO;
using Vanara.PInvoke;

namespace BLIT.WPF.Helpers;

/// <summary>
///     A struct that represents a file type.
///     Note that the extension should not contain the asterisk (*) or the dot (.).
/// </summary>
/// <param name="DisplayName"></param>
/// <param name="Extension"></param>
public record struct FileType(string DisplayName, string Extension) {
    public string Extension { get; init; } = Extension.StartsWith(".") ? Extension[1..] : Extension;

    public Shell32.COMDLG_FILTERSPEC ToFilterSpec() {
        return new Shell32.COMDLG_FILTERSPEC { pszName = DisplayName, pszSpec = $"*.{Extension}" };
    }
}

public static class CommonFileTypes {
    // App
    public static readonly FileType BannerIconsProject = new("Banner Icons Project", "bip");

    // Images
    public static readonly FileType Png = new("PNG Images", "png");
}

public static class FileHelpers {
    private static bool IsDirectory(string path) {
        return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
    }

    private static string GetDirectory(string path) {
        return IsDirectory(path) ? path : Path.GetDirectoryName(path) ?? path;
    }

    public static void OpenFolderInExplorer(string path) {
        Process.Start(new ProcessStartInfo { FileName = GetDirectory(path), UseShellExecute = true });
    }

    public static void EnsureDirectory(ref string path) {
        path = Directory.CreateDirectory(path).FullName;
    }

    public static void OpenUrl(string url) {
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }
}