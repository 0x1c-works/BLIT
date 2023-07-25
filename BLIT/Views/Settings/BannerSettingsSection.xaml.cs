using BLIT.ViewModels.Banner;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace BLIT.Views.Settings;

/// <summary>
/// Interaction logic for BannerSettingsSection.xaml
/// </summary>
public partial class BannerSettingsSection
{
    public BannerSettingsSection()
    {
        InitializeComponent();
        ViewModel = App.MustGet<BannerSettingsViewModel>();

        IObservable<EventPattern<object>> e1 = Observable.FromEventPattern(numGroupStartID, nameof(numGroupStartID.ValueChanged));

        this.WhenActivated((disposables) => {
            this.OneWayBind(ViewModel, x => x.SpriteScanPaths, x => x.listScanPaths.ItemsSource).DisposeWith(disposables);
            this.Bind(ViewModel,
                      x => x.CustomGroupStartID,
                      x => x.numGroupStartID.Value)
                .DisposeWith(disposables);
            this.Bind(ViewModel,
                      x => x.CustomColorStartID,
                      x => x.numColorStartID.Value)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, x => x.AddSpriteScanPath, x => x.btnAdd).DisposeWith(disposables);

            listScanPaths.Events().SelectionChanged.Subscribe((x) => {
                ViewModel.SelectedSpriteScanPath = listScanPaths.SelectedItem as BannerSpriteScanPathViewModel;
            }).DisposeWith(disposables);
        });
    }
}
