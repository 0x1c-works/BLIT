using Vanara.PInvoke;

namespace BannerlordImageTool.Win.Common;

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
