using BLIT.Banner;
using BLIT.scripts.Common;
using BLIT.scripts.Services;
using MessagePack;
using System.ComponentModel;
using System.IO;

namespace BLIT.scripts.Models.BannerIcons;
public class BannerIconEntry : BindableBase {
    public delegate BannerIconEntry Factory(BannerGroupEntry group, string texturePath);

    private readonly BannerGroupEntry _group;
    private string _texturePath = string.Empty;
    private string _spritePath = string.Empty;
    private int _cellIndex;
    private readonly ISettingsService _settings;

    public BannerGroupEntry Group => _group;

    public string TexturePath {
        get => _texturePath;
        set {
            var newPath = ImageHelper.IsValidImage(value) ? Path.GetFullPath(value) : ImageHelper.BAD_IMAGE_PATH;
            if (newPath == _texturePath) {
                ReloadTexture();
            } else {
                SetProperty(ref _texturePath, newPath);
            }
        }
    }
    public string SpritePath {
        get => _spritePath;
        set {
            var newPath = ImageHelper.IsValidImage(value) ? Path.GetFullPath(value) : ImageHelper.BAD_IMAGE_PATH;
            if (newPath == _spritePath) {
                ReloadSprite();
            } else {
                SetProperty(ref _spritePath, newPath);
            }
        }
    }
    public int CellIndex {
        get => _cellIndex;
        set {
            if (value == _cellIndex) {
                return;
            }

            SetProperty(ref _cellIndex, value);
            OnPropertyChanged(nameof(ID));
            OnPropertyChanged(nameof(AtlasName));
        }
    }
    public int AtlasIndex => CellIndex / (TextureMerger.ROWS * TextureMerger.COLS);

    public string AtlasName => BannerUtils.GetAtlasName(_group.GroupID, AtlasIndex);
    public int ID => BannerUtils.GetIconID(_group.GroupID, CellIndex);

    public bool IsValid => ImageHelper.IsValidImage(TexturePath) && AtlasIndex >= 0;

    public BannerIconEntry(BannerGroupEntry group, string texturePath, ISettingsService settings) {
        _group = group;
        _texturePath = texturePath;
        _settings = settings;
        _settings = settings;

        _group.PropertyChanged += OnGroupPropertyChanged;
    }

    private void OnGroupPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(BannerGroupEntry.GroupName)) {
            OnPropertyChanged(nameof(AtlasName));
            OnPropertyChanged(nameof(ID));
        }
    }

    public void ReloadSprite() {
        OnPropertyChanged(nameof(SpritePath));
    }
    public void ReloadTexture() {
        OnPropertyChanged(nameof(SpritePath));
    }

    public BannerIcon ToBannerIcon() {
        return new BannerIcon() {
            ID = ID,
            MaterialName = AtlasName,
            TextureIndex = CellIndex,
            Comment = Path.GetFileNameWithoutExtension(TexturePath),
        };
    }
    public IconSprite ToIconSprite() {
        return new(_group.GroupID, ID, _spritePath);
    }

    public void AutoScanSprite() {
        if (string.IsNullOrEmpty(TexturePath)) {
            return;
        }

        var dir = Path.GetDirectoryName(TexturePath);
        var filename = Path.GetFileName(TexturePath);
        foreach (var relPath in _settings.Banner.SpriteScanFolders) {
            var tryPath = Path.Join(dir, relPath, filename);
            if (File.Exists(tryPath)) {
                SpritePath = tryPath;
                return;
            }
        }
    }

    [MessagePackObject]
    public class SaveData {
        [Key(0)]
        public string TexturePath = string.Empty;
        [Key(1)]
        public string SpritePath = string.Empty;
        [Key(2)]
        public int CellIndex;

        public SaveData(BannerIconEntry model) {
            TexturePath = model.TexturePath;
            SpritePath = model.SpritePath;
            CellIndex = model.CellIndex;
        }
        public SaveData() { }

        public BannerIconEntry Load(BannerGroupEntry group, Factory factory) {
            BannerIconEntry model = factory(group, TexturePath);
            model.SpritePath = SpritePath;
            model.CellIndex = CellIndex;
            return model;
        }
    }
}
