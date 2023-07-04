using BannerlordImageTool.Win.Helpers;
using Microsoft.UI.Xaml;

namespace BannerlordImageTool.Win.Pages.Settings.ViewModels;

public class BannerSpriteScanFolderViewModel : BindableBase
{
    string _relativePath = "";
    public string RelativePath
    {
        get => _relativePath;
        set => SetProperty(ref _relativePath, value);
    }
    bool _isEditing;
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

    public Visibility EditVisibility => IsEditing ? Visibility.Visible : Visibility.Collapsed;
    public Visibility LabelVisibility => IsEditing ? Visibility.Collapsed : Visibility.Visible;

    public BannerSpriteScanFolderViewModel(string relativePath)
    {
        RelativePath = relativePath;
    }
    public BannerSpriteScanFolderViewModel()
    {
        IsEditing = true;
    }
}
