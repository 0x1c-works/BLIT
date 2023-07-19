using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Common.Interfaces;

namespace BLIT.ViewModels;
public class BannerIconsProjectViewModel : ObservableObject, INavigationAware
{
    public string[] TextureResolutions = new[] { "2K", "4K" };
    public void OnNavigatedFrom()
    {
    }

    public void OnNavigatedTo()
    {
    }
}
