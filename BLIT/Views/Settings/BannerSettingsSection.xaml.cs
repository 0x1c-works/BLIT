using BLIT.ViewModels.Banner;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls.Primitives;

namespace BLIT.Views.Settings;

public abstract class BannerSettingsBase : ReactiveUserControl<BannerSettingsViewModel> { }
/// <summary>
/// Interaction logic for BannerSettingsSection.xaml
/// </summary>
public partial class BannerSettingsSection : BannerSettingsBase
{
    public BannerSettingsSection()
    {
        InitializeComponent();
        ViewModel = new BannerSettingsViewModel();

        this.WhenActivated((disposables) => {
            this.OneWayBind(ViewModel, x => x.SpriteScanPaths, x => x.listScanPaths.ItemsSource).DisposeWith(disposables);
            this.BindCommand(ViewModel, x => x.AddSpriteScanPath, x => x.btnAdd).DisposeWith(disposables);

            listScanPaths.Events().SelectionChanged.Subscribe((x) => {
                ViewModel.SelectedSpriteScanPath = listScanPaths.SelectedItem as BannerSpriteScanPathViewModel;
            }).DisposeWith(disposables);
        });
    }
}
