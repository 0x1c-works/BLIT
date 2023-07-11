using BannerlordImageTool.Win.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Controls;

public sealed partial class LoadingOverlay : UserControl
{
    public readonly static DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message),
        typeof(string),
        typeof(LoadingOverlay),
        new PropertyMetadata("Loading..."));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
    public bool IsLoading
    {
        get => loadingControl.IsLoading;
        set => loadingControl.IsLoading = value;
    }

    public LoadingOverlay()
    {
        this.InitializeComponent();
        AppServices.Get<ILoadingService>().RegisterControl(this);
    }
}
