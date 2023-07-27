using ReactiveUI;
using System.Reactive.Disposables;

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
            this.OneWayBind(ViewModel,
                            x => x.Project.Groups,
                            x => x.listBoxGroups.ItemsSource).DisposeWith(disposables);
            this.OneWayBind(ViewModel,
                            x => x.HasSelectedGroup,
                            x => x.toolBtnDeleteGroup.IsEnabled).DisposeWith(disposables);
            this.OneWayBind(ViewModel,
                            x => x.GroupEditorVisibility,
                            x => x.blockTextureGallery.Visibility).DisposeWith(disposables);
            this.OneWayBind(ViewModel,
                            x => x.GroupEditorVisibility,
                            x => x.splitterGroupEditor.Visibility).DisposeWith(disposables);
            this.OneWayBind(ViewModel,
                            x => x.IconDetailsVisibility,
                            x => x.blockIconDetails.Visibility).DisposeWith(disposables);
            this.OneWayBind(ViewModel,
                            x => x.GroupEditorPlaceholderVisibility,
                            x => x.blockGroupEditorPlaceholder.Visibility).DisposeWith(disposables);
            this.OneWayBind(ViewModel,
                            x => x.IconDetailsPlaceholderVisibility,
                            x => x.blockIconDetailsPlaceholder.Visibility).DisposeWith(disposables);


            this.WhenAnyValue(x => x.listBoxGroups.SelectedItem)
                .BindTo(ViewModel, x => x.SelectedGroup)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, x => x.AddGroup, x => x.toolBtnAddGroup).DisposeWith(disposables);
            this.BindCommand(ViewModel, x => x.DeleteGroup, x => x.toolBtnDeleteGroup).DisposeWith(disposables);
        });
    }
}
