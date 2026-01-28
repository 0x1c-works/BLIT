using BLIT.Banner;
using MessagePack;
using Serilog;
using System;
using System.Collections.Generic;
using Windows.Storage;

namespace BLIT.Win.Settings;

[MessagePackObject]
public class BannerSettings {
    private int _customColorStartID = 194;

    private int _customGroupStartID = 7;

    [Key(0)] public List<string> SpriteScanFolders { get; set; } = new();

    [Key(1)] public OutputResolution TextureOutputResolution { get; set; }

    [Key(2)]
    public int CustomGroupStartID {
        get => _customGroupStartID;
        set {
            if (_customGroupStartID == value) {
                return;
            }

            _customGroupStartID = value;
            Save();
        }
    }

    [Key(3)]
    public int CustomColorStartID {
        get => _customColorStartID;
        set {
            if (_customColorStartID == value) {
                return;
            }

            _customColorStartID = value;
            Save();
        }
    }

    public void SaveSpriteScanFolders(IEnumerable<string> scanFolders) {
        SpriteScanFolders = new List<string>(scanFolders);
        Save();
    }

    public void Save() {
        var data = MessagePackSerializer.Serialize(this);
        Log.Debug("Saving banner settings: {Data}", MessagePackSerializer.ConvertToJson(data));
        var savedSettings = Convert.ToBase64String(data);
        ApplicationData.Current.LocalSettings.Values["BannerSettings"] = savedSettings;
    }

    public static BannerSettings Load() {
        var savedSettings = ApplicationData.Current.LocalSettings.Values["BannerSettings"] as string;
        if (string.IsNullOrEmpty(savedSettings)) {
            return new BannerSettings();
        }

        var data = Convert.FromBase64String(savedSettings);
        Log.Debug("Loaded stored banner settings: {Data}", MessagePackSerializer.ConvertToJson(data));
        return MessagePackSerializer.Deserialize<BannerSettings>(data);
    }
}