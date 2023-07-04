using MessagePack;
using Serilog;
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

    int _customGroupStartID = 10000;
    [Key(2)]
    public int CustomGroupStartID
    {
        get => _customGroupStartID;
        set
        {
            if (_customGroupStartID == value)
            {
                return;
            }

            _customGroupStartID = value;
            Save();
        }
    }
    int _customColorStartID = 500;
    [Key(3)]
    public int CustomColorStartID
    {
        get => _customColorStartID;
        set
        {
            if (_customColorStartID == value)
            {
                return;
            }

            _customColorStartID = value;
            Save();
        }
    }
    public void SaveSpriteScanFolders(IEnumerable<string> scanFolders)
    {
        SpriteScanFolders = new(scanFolders);
        Save();
    }

    public void Save()
    {
        var data = MessagePackSerializer.Serialize(this);
        Log.Debug("Saving banner settings: {Data}", MessagePackSerializer.ConvertToJson(data));
        var stored = Convert.ToBase64String(data);
        ApplicationData.Current.LocalSettings.Values["BannerSettings"] = stored;
    }
    public static BannerSettings Load()
    {
        var stored = ApplicationData.Current.LocalSettings.Values["BannerSettings"] as string;
        var data = Convert.FromBase64String(stored);
        Log.Debug("Loaded stored banner settings: {Data}", MessagePackSerializer.ConvertToJson(data));
        return string.IsNullOrEmpty(stored)
            ? new BannerSettings()
            : MessagePackSerializer.Deserialize<BannerSettings>(data);
    }
}
