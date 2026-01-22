using System.Windows;
using System.Windows.Controls;

namespace BLIT.WPF.Controls;

public partial class PageContainer : UserControl {
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(PageContainer),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content),
        typeof(object),
        typeof(PageContainer),
        new PropertyMetadata(null));

    public string Title {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public new object Content {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public PageContainer() {
        InitializeComponent();
    }
}
