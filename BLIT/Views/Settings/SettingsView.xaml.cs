using BLIT.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;

namespace BLIT.Views.Settings;

public abstract class SettingsViewBase : ReactiveUserControl<SettingsViewModel> { }
/// <summary>
/// Interaction logic for SettingsView.xaml
/// </summary>
public partial class SettingsView : SettingsViewBase
{
    public SettingsView()
    {
        InitializeComponent();

        this.WhenActivated((disposables) => {
            this.OneWayBind(ViewModel, x => x.SupportedLanguages, x => x.cboLanguage.ItemsSource).DisposeWith(disposables);
            this.OneWayBind(ViewModel, x => x.AppVersion, x => x.txtVersion.Text).DisposeWith(disposables);
            this.BindCommand(ViewModel, x => x.OpenLogFolder, x => x.btnOpenLogFolder).DisposeWith(disposables);
        });
    }
}
