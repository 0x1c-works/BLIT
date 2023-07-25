using BLIT.Banner;
using MessagePack;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace BLIT.Services;

[MessagePackObject]
public class BannerSettings : ReactiveObject, IDisposable
{
    public const int MIN_CUSTOM_GROUP_ID = 7;
    public const int MAX_CUSTOM_GROUP_ID = 99999;
    public const int MIN_CUSTOM_COLOR_ID = 194;
    public const int MAX_CUSTOM_COLOR_ID = 999999;

    CompositeDisposable _disposables = new CompositeDisposable();

    string[] _spriteScanPaths = new string[0];
    [Key(0)]
    public string[] SpriteScanPaths
    {
        get => _spriteScanPaths;
        set
        {
            var cleanData = value.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();
            this.RaiseAndSetIfChanged(ref _spriteScanPaths, cleanData);
        }
    }
    [Key(1)]
    [Reactive]
    public OutputResolution TextureOutputResolution { get; set; }

    [Key(2)]
    [Reactive]
    public int CustomGroupStartID { get; set; } = 7;
    [Key(3)]
    [Reactive]
    public int CustomColorStartID { get; set; } = 194;

    public BannerSettings()
    {
        // trigger saving when any of the properties change in 1s
        Observable.Merge(new[] {
            this.WhenAnyValue(x=>x.SpriteScanPaths).Select((_)=>Unit.Default),
            this.WhenAnyValue(x=>x.TextureOutputResolution).Select((_)=>Unit.Default),
            this.WhenAnyValue(x=>x.CustomGroupStartID).Select((_)=>Unit.Default),
            this.WhenAnyValue(x=>x.CustomColorStartID).Select((_)=>Unit.Default),
        })
        .Throttle(TimeSpan.FromMilliseconds(1000))
        .Subscribe((_) => Save())
        .DisposeWith(_disposables);
    }

    public void Save()
    {
        var data = MessagePackSerializer.Serialize(this);
        Log.Debug("Saving banner settings: {Data}", MessagePackSerializer.ConvertToJson(data));
        var savedSettings = Convert.ToBase64String(data);
        Preferences.Default["BannerSettings"] = savedSettings;
        Preferences.Default.Save();
    }
    public static BannerSettings Load()
    {
        var savedSettings = Preferences.Default["BannerSettings"] as string;
        if (string.IsNullOrEmpty(savedSettings)) return new BannerSettings();
        var data = Convert.FromBase64String(savedSettings);
        Log.Debug("Loaded stored banner settings: {Data}", MessagePackSerializer.ConvertToJson(data));
        return MessagePackSerializer.Deserialize<BannerSettings>(data);
    }

    public void Dispose()
    {
        _disposables?.Dispose();
        // Final flush
        Save();
    }
}
