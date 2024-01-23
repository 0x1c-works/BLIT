using BLIT.Banner;
using BLIT.scripts.Common;
using Godot;
using MessagePack;
using System.Collections.Generic;

namespace BLIT.scripts.Models.BannerIcons;

public class BannerColorEntry : BindableBase {
    public delegate BannerColorEntry Factory(int id);

    private BannerIconsProject _project;
    private int _id;
    private Color _color = Colors.White;
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
    public Color Color {
        get => _color;
        set {
            SetProperty(ref _color, value);
            OnPropertyChanged(nameof(CanExport));
        }
    }
    public bool IsForSigil {
        get => _isForSigil;
        set => SetProperty(ref _isForSigil, value);
    }
    public bool IsForBackground {
        get => _isForBackground;
        set => SetProperty(ref _isForBackground, value);
    }

    public bool CanExport => ID >= 0 && Color.A > 0;

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
        [MessagePackFormatter(typeof(GodotColorFormatter))]
        public Color Color;
        [Key(2)]
        public bool IsForSigil;
        [Key(3)]
        public bool IsForBackground;

        public SaveData(BannerColorEntry model) {
            ID = model.ID;
            Color = model.Color;
            IsForSigil = model.IsForSigil;
            IsForBackground = model.IsForBackground;
        }
        public SaveData() { }
        public BannerColorEntry Load(Factory factory) {
            BannerColorEntry model = factory(ID);
            model.Color = Color;
            model.IsForSigil = IsForSigil;
            model.IsForBackground = IsForBackground;
            return model;
        }
    }
    public static int Compare(BannerColorEntry x, BannerColorEntry y) {
        x.Color.ToHsv(out var h1, out var s1, out var v1);
        y.Color.ToHsv(out var h2, out var s2, out var v2);

        if (h1 == 360) h1 = 0;
        if (h2 == 360) h2 = 0;

        var deltaH = h1 - h2;
        var deltaS = s1 - s2;
        var deltaV = v1 - v2;

        // for greyscale, sort from white to black
        if (s1 == 0 && s2 == 0) {
            return deltaV == 1 ? -1 : deltaV == 0 ? 1 : (deltaV > 0 ? -1 : 1);
        }
        // greyscale always is at the start
        if (s1 == 0) return -1;
        if (s2 == 0) return 1;

        // For normal colors, sort by H (inc) > S (desc) > V (desc)
        if (deltaH != 0) return deltaH > 0 ? 1 : -1;
        if (deltaS != 0) return deltaS > 0 ? -1 : 1;
        if (deltaV != 0) return deltaV > 0 ? -1 : 1;

        return 0;
    }

    public class Comparer : IComparer<BannerColorEntry> {
        public int Compare(BannerColorEntry? x, BannerColorEntry? y) {
            return Compare(x, y);
        }
    }
}

