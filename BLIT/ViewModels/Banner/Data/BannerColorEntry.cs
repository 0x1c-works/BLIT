using BLIT.Banner;
using BLIT.Helpers;
using BLIT.Services;
using MessagePack;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media;

using ColorConverter = ColorHelper.ColorConverter;

namespace BLIT.ViewModels.Banner.Data;

public class BannerColorEntry : ReactiveObject, IDisposable
{
    public delegate BannerColorEntry Factory(int id);

    readonly CompositeDisposable _disposables = new();
    BannerIconsProject _project;
    int _id = BannerSettings.MIN_CUSTOM_COLOR_ID;

    public int ID
    {
        get => _id;
        set
        {
            value = _project.ValidateColorID(_id, value);
            this.RaiseAndSetIfChanged(ref _id, value);
        }
    }
    [Reactive] public Color Color { get; set; } = Color.FromArgb(255, 255, 255, 255);
    [Reactive] public bool IsForSigil { get; set; } = true;
    [Reactive] public bool IsForBackground { get; set; } = true;

    [ObservableAsProperty] public bool CanExport { get; }

    public BannerColorEntry(BannerIconsProject project, int id)
    {
        _project = project;
        ID = id;
        Observable.CombineLatest(this.WhenAnyValue(x => x.ID),
                                 this.WhenAnyValue(x => x.Color),
                                 (id, color) => id >= 0 && color.A > 0)
            .ToPropertyEx(this, x => x.CanExport)
            .DisposeWith(_disposables);
    }

    public BannerColor ToBannerColor()
    {
        return new BannerColor {
            ID = ID,
            Hex = ColorToHex(Color),
            PlayerCanChooseForSigil = IsForSigil,
            PlayerCanChooseForBackground = IsForBackground,
        };
    }

    static string ColorToHex(Color color)
    {
        return ColorConverter.RgbToHex(color.ToRGB()).ToString();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    [MessagePackObject]
    public class SaveData
    {
        [Key(0)]
        public int ID;
        [Key(1)]
        [MessagePackFormatter(typeof(ColorMessagePackFormatter))]
        public Color Color;
        [Key(2)]
        public bool IsForSigil;
        [Key(3)]
        public bool IsForBackground;

        public SaveData(BannerColorEntry vm)
        {
            ID = vm.ID;
            Color = vm.Color;
            IsForSigil = vm.IsForSigil;
            IsForBackground = vm.IsForBackground;
        }
        public SaveData() { }
        public BannerColorEntry Load(Factory factory)
        {
            BannerColorEntry vm = factory(ID);
            vm.Color = Color;
            vm.IsForSigil = IsForSigil;
            vm.IsForBackground = IsForBackground;
            return vm;
        }
    }
}

