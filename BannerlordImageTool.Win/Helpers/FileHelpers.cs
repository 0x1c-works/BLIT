using System.Diagnostics;
using System.IO;
using Vanara.PInvoke;

namespace BannerlordImageTool.Win.Helpers;

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
    public static readonly FileType BannerIconsProject = new FileType("Banner Icons Project", "bip");

    // Images
    public static readonly FileType Png = new("PNG Images", "png");
}
public static class FileHelpers
{
    public static void OpenFolderInExplorer(string path)
    {
        if (!File.GetAttributes(path).HasFlag(FileAttributes.Directory))
        {
            path = Path.GetDirectoryName(path);
        }
        Process.Start("explorer.exe", path);
    }
}
