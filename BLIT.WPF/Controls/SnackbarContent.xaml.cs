using System.Windows;
using System.Windows.Controls;

namespace BLIT.WPF.Controls;

public partial class SnackbarContent : UserControl {
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message),
        typeof(ContentControl),
        typeof(SnackbarContent),
        new PropertyMetadata(default(ContentControl)));

    public static readonly DependencyProperty ActionProperty = DependencyProperty.Register(
        nameof(Action),
        typeof(ContentControl),
        typeof(SnackbarContent),
        new PropertyMetadata(default(ContentControl)));

    public SnackbarContent() {
        InitializeComponent();
        DataContext = this;
    }

    public ContentControl? Message {
        get => GetValue(MessageProperty) as ContentControl;
        set => SetValue(MessageProperty, value);
    }

    public ContentControl? Action {
        get => GetValue(ActionProperty) as ContentControl;
        set => SetValue(ActionProperty, value);
    }
}