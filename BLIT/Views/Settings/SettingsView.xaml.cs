using ReactiveUI;
using System.Reactive.Disposables;

namespace BLIT.Views.Settings;

/// <summary>
/// Interaction logic for SettingsView.xaml
/// </summary>
public partial class SettingsView
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
