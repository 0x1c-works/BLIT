using BannerlordImageTool.Win.Helpers;
using Microsoft.UI.Xaml;

namespace BannerlordImageTool.Win.ViewModels.Settings;

public class BannerSpriteScanFolderViewModel : BindableBase
{
    private string _relativePath = "";
    public string RelativePath
    {
        get => _relativePath;
        set => SetProperty(ref _relativePath, value);
    }
    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            SetProperty(ref _isEditing, value);
            OnPropertyChanged(nameof(EditVisibility));
            OnPropertyChanged(nameof(LabelVisibility));
        }
    }

    public Visibility EditVisibility { get => IsEditing ? Visibility.Visible : Visibility.Collapsed; }
    public Visibility LabelVisibility { get => IsEditing ? Visibility.Collapsed : Visibility.Visible; }

    public BannerSpriteScanFolderViewModel(string relativePath)
    {
        RelativePath = relativePath;
    }
    public BannerSpriteScanFolderViewModel()
    {
        IsEditing = true;
    }
}
