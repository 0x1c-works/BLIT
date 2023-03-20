using MessagePack;
using System;
using System.Collections.Generic;
using Windows.Storage;

namespace BannerlordImageTool.Win.Settings;

[MessagePackObject]
public class BannerSettings
{
    [Key(0)]
    public List<string> SpriteScanFolders { get; set; } = new();
    [Key(1)]
    public Banner.OutputResolution TextureOutputResolution { get; set; }

    public void SaveSpriteScanFolders(IEnumerable<string> scanFolders)
    {
        SpriteScanFolders = new(scanFolders);
        Save();
    }

    public void Save()
    {
        var data = MessagePackSerializer.Serialize(this);
        var stored = Convert.ToBase64String(data);
        ApplicationData.Current.LocalSettings.Values["BannerSettings"] = stored;
    }
    public static BannerSettings Load()
    {
        var stored = ApplicationData.Current.LocalSettings.Values["BannerSettings"] as string;
        var data = Convert.FromBase64String(stored);
        return string.IsNullOrEmpty(stored)
            ? new BannerSettings()
            : MessagePackSerializer.Deserialize<BannerSettings>(data);
    }
}
