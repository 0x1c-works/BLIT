using BLIT.Banner;
using BLIT.scripts.Common;
using BLIT.scripts.Services;
using MessagePack;
using System.ComponentModel;
using System.IO;

namespace BLIT.scripts.Models.BannerIcons;
public class BannerIconEntry : BindableBase
{
    public delegate BannerIconEntry Factory(BannerGroupEntry groupVm, string texturePath);

    readonly BannerGroupEntry _groupViewModel;
    string _texturePath = string.Empty;
    string _spritePath = string.Empty;
    int _cellIndex;
    readonly ISettingsService _settings;

    public string TexturePath
    {
        get => _texturePath;
        set
        {
            var newPath = ImageHelper.IsValidImage(value) ? Path.GetFullPath(value) : ImageHelper.BAD_IMAGE_PATH;
            if (newPath == _texturePath)
            {
                ReloadTexture();
            }
            else
            {
                SetProperty(ref _texturePath, newPath);
            }
        }
    }
    public string SpritePath
    {
        get => _spritePath;
        set
        {
            var newPath = ImageHelper.IsValidImage(value) ? Path.GetFullPath(value) : ImageHelper.BAD_IMAGE_PATH;
            if (newPath == _spritePath)
            {
                ReloadSprite();
            }
            else
            {
                SetProperty(ref _spritePath, newPath);
            }
        }
    }
    public int CellIndex
    {
        get => _cellIndex;
        set
        {
            if (value == _cellIndex)
            {
                return;
            }

            SetProperty(ref _cellIndex, value);
            OnPropertyChanged(nameof(ID));
            OnPropertyChanged(nameof(AtlasName));
        }
    }
    public int AtlasIndex => CellIndex / (TextureMerger.ROWS * TextureMerger.COLS);

    public string AtlasName => BannerUtils.GetAtlasName(_groupViewModel.GroupID, AtlasIndex);
    public int ID => BannerUtils.GetIconID(_groupViewModel.GroupID, CellIndex);

    public bool IsValid => ImageHelper.IsValidImage(TexturePath) && AtlasIndex >= 0;

    public BannerIconEntry(BannerGroupEntry groupVm, string texturePath, ISettingsService settings)
    {
        _groupViewModel = groupVm;
        _texturePath = texturePath;
        _settings = settings;
        _settings = settings;

        _groupViewModel.PropertyChanged += _viewModel_PropertyChanged;
    }

    void _viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BannerGroupEntry.GroupName))
        {
            OnPropertyChanged(nameof(AtlasName));
            OnPropertyChanged(nameof(ID));
        }
    }

    public void ReloadSprite()
    {
        if (string.IsNullOrEmpty(SpritePath)) return;
        var oldPath = SpritePath;
        SpritePath = oldPath + "1";
        SpritePath = oldPath;
    }
    public void ReloadTexture()
    {
        if (string.IsNullOrEmpty(TexturePath)) return;
        var oldPath = TexturePath;
        TexturePath = oldPath + "1";
        TexturePath = oldPath;
    }

    public BannerIcon ToBannerIcon()
    {
        return new BannerIcon() {
            ID = ID,
            MaterialName = AtlasName,
            TextureIndex = CellIndex,
            Comment = Path.GetFileNameWithoutExtension(TexturePath),
        };
    }
    public IconSprite ToIconSprite()
    {
        return new(_groupViewModel.GroupID, ID, _spritePath);
    }

    public void AutoScanSprite()
    {
        if (string.IsNullOrEmpty(TexturePath))
        {
            return;
        }

        var dir = Path.GetDirectoryName(TexturePath);
        var filename = Path.GetFileName(TexturePath);
        foreach (var relPath in _settings.Banner.SpriteScanFolders)
        {
            var tryPath = Path.Join(dir, relPath, filename);
            if (File.Exists(tryPath))
            {
                SpritePath = tryPath;
                return;
            }
        }
    }

    [MessagePackObject]
    public class SaveData
    {
        [Key(0)]
        public string TexturePath;
        [Key(1)]
        public string SpritePath;
        [Key(2)]
        public int CellIndex;

        public SaveData(BannerIconEntry vm)
        {
            TexturePath = vm.TexturePath;
            SpritePath = vm.SpritePath;
            CellIndex = vm.CellIndex;
        }
        public SaveData() { }

        public BannerIconEntry Load(BannerGroupEntry groupVM, Factory factory)
        {
            BannerIconEntry vm = factory(groupVM, TexturePath);
            vm.SpritePath = SpritePath;
            vm.CellIndex = CellIndex;
            return vm;
        }
    }
}
