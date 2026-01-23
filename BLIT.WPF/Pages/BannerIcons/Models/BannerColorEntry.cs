using BLIT.Banner;
using BLIT.WPF.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using MessagePack;
using System.Windows.Media;

namespace BLIT.WPF.Pages.BannerIcons.Models;

public partial class BannerColorEntry : ObservableObject {
    public delegate BannerColorEntry Factory(int id);

    private BannerIconsProject _project;
    private int _id;
    private Color _color = Color.FromArgb(255, 255, 255, 255);
    private bool _isForSigil = true;
    private bool _isForBackground = true;

    public int ID {
        get => _id;
        set {
            value = _project.ValidateColorID(_id, value);
            SetProperty(ref _id, value);
            OnPropertyChanged(nameof(CanExport));
        }
    }
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanExport))]
    [NotifyPropertyChangedFor(nameof(R))]
    [NotifyPropertyChangedFor(nameof(G))]
    [NotifyPropertyChangedFor(nameof(B))]
    [NotifyPropertyChangedFor(nameof(HexColor))]
    private Color color = Color.FromArgb(255, 255, 255, 255);
    
    [ObservableProperty]
    private bool isForSigil = true;
    
    [ObservableProperty]
    private bool isForBackground = true;

    public bool CanExport => ID >= 0 && Color.A > 0;

    // Individual RGB component properties for XAML binding
    public byte R {
        get => _color.R;
        set {
            if (_color.R != value) {
                Color = Color.FromArgb(_color.A, value, _color.G, _color.B);
                OnPropertyChanged(nameof(R));
            }
        }
    }

    public byte G {
        get => _color.G;
        set {
            if (_color.G != value) {
                Color = Color.FromArgb(_color.A, _color.R, value, _color.B);
                OnPropertyChanged(nameof(G));
            }
        }
    }

    public byte B {
        get => _color.B;
        set {
            if (_color.B != value) {
                Color = Color.FromArgb(_color.A, _color.R, _color.G, value);
                OnPropertyChanged(nameof(B));
            }
        }
    }

    // Hex color property for text input binding
    public string HexColor {
        get => $"#{_color.R:X2}{_color.G:X2}{_color.B:X2}";
        set {
            if (string.IsNullOrWhiteSpace(value)) return;

            var hex = value.Trim();
            if (hex.StartsWith("#")) hex = hex.Substring(1);

            if (hex.Length == 6) {
                try {
                    byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                    byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                    byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                    Color = Color.FromArgb(255, r, g, b);
                    OnPropertyChanged(nameof(HexColor));
                } catch {
                    // Invalid hex format, ignore
                }
            }
        }
    }

    public BannerColorEntry(BannerIconsProject project, int id) {
        _project = project;
        ID = id;
    }

    public BannerColor ToBannerColor() {
        return new BannerColor {
            ID = ID,
            Hex = ColorToHex(Color),
            PlayerCanChooseForSigil = IsForSigil,
            PlayerCanChooseForBackground = IsForBackground,
        };
    }

    private static string ColorToHex(Color color) {
        return $"0xff{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    [MessagePackObject]
    public class SaveData {
        [Key(0)]
        public int ID;
        [Key(1)]
        [MessagePackFormatter(typeof(WPFColorFormatter))]
        public Color Color;
        [Key(2)]
        public bool IsForSigil;
        [Key(3)]
        public bool IsForBackground;

        public SaveData(BannerColorEntry vm) {
            ID = vm.ID;
            Color = vm.Color;
            IsForSigil = vm.IsForSigil;
            IsForBackground = vm.IsForBackground;
        }
        public SaveData() { }
        public BannerColorEntry Load(Factory factory) {
            BannerColorEntry vm = factory(ID);
            vm.Color = Color;
            vm.IsForSigil = IsForSigil;
            vm.IsForBackground = IsForBackground;
            return vm;
        }
    }
    public static int Compare(BannerColorEntry x, BannerColorEntry y) {
        // Convert RGB to HSV manually for WPF
        var hsv1 = RgbToHsv(x.Color);
        var hsv2 = RgbToHsv(y.Color);

        if (hsv1.H == 360) hsv1.H = 0;
        if (hsv2.H == 360) hsv2.H = 0;

        var deltaH = hsv1.H - hsv2.H;
        var deltaS = hsv1.S - hsv2.S;
        var deltaV = hsv1.V - hsv2.V;

        // for greyscale, sort from white to black
        if (hsv1.S == 0 && hsv2.S == 0) {
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

    private static (double H, double S, double V) RgbToHsv(Color color) {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        double max = System.Math.Max(r, System.Math.Max(g, b));
        double min = System.Math.Min(r, System.Math.Min(g, b));
        double delta = max - min;

        double h = 0;
        if (delta != 0) {
            if (max == r) {
                h = 60 * (((g - b) / delta) % 6);
            } else if (max == g) {
                h = 60 * (((b - r) / delta) + 2);
            } else {
                h = 60 * (((r - g) / delta) + 4);
            }
        }
        if (h < 0) h += 360;

        double s = max == 0 ? 0 : delta / max;
        double v = max;

        return (h, s, v);
    }
}

