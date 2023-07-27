using BLIT.ViewModels.Banner.Data;
using ReactiveUI;
using Serilog;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls.Primitives;

namespace BLIT.Views.Banner;

/// <summary>
/// Interaction logic for BannerView.xaml
/// </summary>
public partial class BannerView
{
    public BannerView()
    {
        InitializeComponent();

        this.WhenActivated((disposables) => {
            this.OneWayBind(ViewModel, x => x.Project.Groups, x => x.listBoxGroups.ItemsSource).DisposeWith(disposables);
            this.OneWayBind(ViewModel, x => x.HasSelectedGroup, x => x.toolBtnDeleteGroup.IsEnabled).DisposeWith(disposables);
            listBoxGroups.Events().SelectionChanged
                .Select(_ => listBoxGroups.SelectedItem as BannerGroupEntry)
                .Do(g => {
                    Log.Debug("selection changed {g}", g);
                })
                .BindTo(ViewModel, x => x.SelectedGroup)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, x => x.AddGroup, x => x.toolBtnAddGroup).DisposeWith(disposables);
            this.BindCommand(ViewModel, x => x.DeleteGroup, x => x.toolBtnDeleteGroup).DisposeWith(disposables);
        });
    }
}
