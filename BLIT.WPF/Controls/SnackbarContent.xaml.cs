using System.Windows;
using System.Windows.Controls;

namespace BLIT.WPF.Controls;

public partial class SnackbarContent : UserControl {
    public SnackbarContent() {
        InitializeComponent();
        DataContext = this;
    }

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message),
        typeof(ContentControl),
        typeof(SnackbarContent),
        new PropertyMetadata(default(ContentControl)));

    public ContentControl? Message {
        get => GetValue(MessageProperty) as ContentControl;
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty ActionProperty = DependencyProperty.Register(
        nameof(Action),
        typeof(ContentControl),
        typeof(SnackbarContent),
        new PropertyMetadata(default(ContentControl)));

    public ContentControl? Action {
        get => GetValue(ActionProperty) as ContentControl;
        set => SetValue(ActionProperty, value);
    }
}