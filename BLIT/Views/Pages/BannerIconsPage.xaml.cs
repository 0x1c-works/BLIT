using BLIT.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace BLIT.Views.Pages;
/// <summary>
/// Interaction logic for BannerIconsPage.xaml
/// </summary>
public partial class BannerIconsPage : INavigableView<BannerIconsProjectViewModel>
{
    public BannerIconsProjectViewModel ViewModel { get; }
    public BannerIconsPage(BannerIconsProjectViewModel vm)
    {
        ViewModel = vm;
        InitializeComponent();
    }

}
