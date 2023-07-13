using System.Diagnostics;
using System.IO;
using Vanara.PInvoke;

namespace BLIT.Win.Helpers;

/// <summary>
/// A struct that represents a file type.
/// Note that the extension should not contain the asterisk (*) or the dot (.).
/// </summary>
/// <param name="DisplayName"></param>
/// <param name="Extension"></param>
public record struct FileType(string DisplayName, string Extension)
{
    public string Extension { get; init; } = Extension.StartsWith(".") ? Extension[1..] : Extension;
    public Shell32.COMDLG_FILTERSPEC ToFilterSpec()
    {
        return new Shell32.COMDLG_FILTERSPEC() { pszName = DisplayName, pszSpec = $"*.{Extension}" };
    }
}
public static class CommonFileTypes
{
    // App
    public static readonly FileType BannerIconsProject = new("Banner Icons Project", "bip");

    // Images
    public static readonly FileType Png = new("PNG Images", "png");
}
public static class FileHelpers
{
    static bool IsDirectory(string path)
    {
        return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
    }
    static string GetDirectory(string path)
    {
        return IsDirectory(path) ? path : Path.GetDirectoryName(path);
    }
    public static void OpenFolderInExplorer(string path)
    {
        Process.Start("explorer.exe", GetDirectory(path));
    }
    public static void EnsureDirectory(ref string path)
    {
        path = Directory.CreateDirectory(path).FullName;
    }
}
