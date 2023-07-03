using BannerlordImageTool.Banner;
using BannerlordImageTool.Win.Helpers;
using BannerlordImageTool.Win.Services;
using BannerlordImageTool.Win.Settings;
using MessagePack;
using System.ComponentModel;
using System.IO;

namespace BannerlordImageTool.Win.ViewModels.BannerIcons;
public class IconViewModel : BindableBase
{
    private readonly ISettingsService _settings = AppServices.Get<ISettingsService>();
    private GroupViewModel _groupViewModel;
    private string _texturePath;
    private string _spritePath;
    private int _cellIndex;

    public string TexturePath
    {
        get => _texturePath;
        set
        {
            var newPath = Path.GetFullPath(value);
            if (newPath == _texturePath) return;
            SetProperty(ref _texturePath, value);
        }
    }
    public string SpritePath
    {
        get => _spritePath ?? "";
        set
        {
            var newPath = Path.GetFullPath(value);
            if (newPath == _spritePath) return;
            SetProperty(ref _spritePath, newPath);
        }
    }
    public int CellIndex
    {
        get => _cellIndex;
        set
        {
            if (value == _cellIndex) return;
            SetProperty(ref _cellIndex, value);
            OnPropertyChanged(nameof(ID));
            OnPropertyChanged(nameof(AtlasName));
        }
    }
    public int AtlasIndex
    {
        get => CellIndex / (TextureMerger.ROWS * TextureMerger.COLS);
    }

    public string AtlasName
    {
        get => BannerUtils.GetAtlasName(_groupViewModel.GroupID, AtlasIndex);
    }
    public int ID
    {
        get => BannerUtils.GetIconID(_groupViewModel.GroupID, CellIndex);
    }

    public bool IsSelected { get; set; }
    public bool IsValid { get => !string.IsNullOrEmpty(TexturePath) && AtlasIndex >= 0; }

    public IconViewModel(GroupViewModel groupVm, string texturePath)
    {
        _groupViewModel = groupVm;
        _texturePath = texturePath;

        _groupViewModel.PropertyChanged += _viewModel_PropertyChanged;
    }

    private void _viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GroupViewModel.GroupName))
        {
            OnPropertyChanged(nameof(AtlasName));
            OnPropertyChanged(nameof(ID));
        }
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
        if (string.IsNullOrEmpty(TexturePath)) return;
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

        public SaveData(IconViewModel vm)
        {
            TexturePath = vm.TexturePath;
            SpritePath = vm.SpritePath;
            CellIndex = vm.CellIndex;
        }
        public SaveData() { }

        public IconViewModel Load(GroupViewModel groupVM)
        {
            return new IconViewModel(groupVM, TexturePath) {
                SpritePath = SpritePath,
                CellIndex = CellIndex,
            };
        }
    }
}
