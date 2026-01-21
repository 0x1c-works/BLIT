using BLIT.WPF.Services;
using System.Windows;
using System.Windows.Controls;

namespace BLIT.WPF.Controls;

public partial class LoadingOverlay : UserControl {
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message),
        typeof(string),
        typeof(LoadingOverlay),
        new PropertyMetadata("Loading..."));

    public string Message {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public bool IsLoading {
        get => RootGrid.Visibility == Visibility.Visible;
        set => RootGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    public LoadingOverlay() {
        InitializeComponent();
        Loaded += (s, e) => {
            AppServices.Get<ILoadingService>()?.RegisterControl(this);
        };
    }
}
