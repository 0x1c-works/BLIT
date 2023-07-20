using BLIT.Banner;
using BLIT.Win.Helpers;
using CommunityToolkit.WinUI.Helpers;
using MessagePack;
using Windows.UI;

namespace BLIT.Win.Pages.BannerIcons.Models;

public class BannerColorEntry : BindableBase
{
    public delegate BannerColorEntry Factory(int id);

    BannerIconsProject _project;
    int _id;
    Color _color = Color.FromArgb(255, 255, 255, 255);
    bool _isForSigil = true;
    bool _isForBackground = true;

    public int ID
    {
        get => _id;
        set
        {
            value = _project.ValidateColorID(_id, value);
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

    public BannerColorEntry(BannerIconsProject project, int id)
    {
        _project = project;
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
    public static int Compare(BannerColorEntry x, BannerColorEntry y)
    {
        CommunityToolkit.WinUI.HsvColor hsv1 = x.Color.ToHsv();
        CommunityToolkit.WinUI.HsvColor hsv2 = y.Color.ToHsv();

        if (hsv1.H == 360) hsv1.H = 0;
        if (hsv2.H == 360) hsv2.H = 0;

        var deltaH = hsv1.H - hsv2.H;
        var deltaS = hsv1.S - hsv2.S;
        var deltaV = hsv1.V - hsv2.V;

        // for greyscale, sort from white to black
        if (hsv1.S == 0 && hsv2.S == 0)
        {
            return deltaV == 1 ? -1 : deltaV == 0 ? 1 : (deltaV > 0 ? -1 : 1);
        }
        // greyscale always is at the start
        if (hsv1.S == 0) return -1;
        if (hsv2.S == 0) return 1;

        // For normal colors, sort by H (inc) > S (desc) > V (desc)
        if (deltaH != 0) return deltaH > 0 ? 1 : -1;
        if (deltaS != 0) return deltaS > 0 ? -1 : 1;
        if (deltaV != 0) return deltaV > 0 ? -1 : 1;

        return 0;
    }
}

