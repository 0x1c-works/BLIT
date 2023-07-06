using BannerlordImageTool.Banner;
using BannerlordImageTool.Win.Helpers;
using MessagePack;
using Windows.UI;

namespace BannerlordImageTool.Win.Pages.BannerIcons.ViewModels;

public class BannerColorEntry : BindableBase
{
    public delegate BannerColorEntry Factory(int id);

    int _id;
    Color _color;
    bool _isForSigil = true;
    bool _isForBackground = true;

    public int ID
    {
        get => _id;
        set
        {
            SetProperty(ref _id, value);
            OnPropertyChanged(nameof(CanExport));
        }
    }
    public Color Color
    {
        get => _color;
        set
        {
            SetProperty(ref _color, value);
            OnPropertyChanged(nameof(CanExport));
        }
    }
    public bool IsForSigil
    {
        get => _isForSigil;
        set => SetProperty(ref _isForSigil, value);
    }
    public bool IsForBackground
    {
        get => _isForBackground;
        set => SetProperty(ref _isForBackground, value);
    }

    public bool CanExport => ID >= 0 && Color.A > 0;

    public BannerColorEntry(int id)
    {
        ID = id;
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
        return $"0xff{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    [MessagePackObject]
    public class SaveData
    {
        [Key(0)]
        public int ID;
        [Key(1)]
        [MessagePackFormatter(typeof(WinUIColorFormatter))]
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

