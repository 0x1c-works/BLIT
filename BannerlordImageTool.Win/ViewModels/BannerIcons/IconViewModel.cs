using BannerlordImageTool.BannerTex;
using BannerlordImageTool.Win.Common;
using System.ComponentModel;

namespace BannerlordImageTool.Win.ViewModels.BannerIcons;
public class IconViewModel : BindableBase
{
    private GroupViewModel _groupViewModel;
    private string _texturePath;
    private int _cellIndex;

    public string FilePath
    {
        get => _texturePath;
        set => SetProperty(ref _texturePath, value);
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
    public bool IsValid { get => !string.IsNullOrEmpty(FilePath) && AtlasIndex >= 0; }

    public IconViewModel(GroupViewModel groupVm, string filePath)
    {
        _groupViewModel = groupVm;
        _texturePath = filePath;

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
        return new BannerIcon() { ID = ID, MaterialName = AtlasName, TextureIndex = CellIndex };
    }
}
