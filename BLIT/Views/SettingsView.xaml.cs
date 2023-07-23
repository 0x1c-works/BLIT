using BLIT.ViewModels;
using ReactiveUI;

namespace BLIT.Views;
public abstract class SettingsViewBase : ReactiveUserControl<SettingsViewModel> { }
/// <summary>
/// Interaction logic for SettingsView.xaml
/// </summary>
public partial class SettingsView : SettingsViewBase
{
    public SettingsView()
    {
        InitializeComponent();
    }
}
