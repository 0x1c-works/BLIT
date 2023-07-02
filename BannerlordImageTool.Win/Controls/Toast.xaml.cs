using BannerlordImageTool.Win.ViewModels.Global;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Controls;

public sealed partial class Toast : UserControl
{
    ToastViewModel ViewModel { get; } = new ToastViewModel();

    public ToastVariant Variant
    {
        get => ViewModel.Variant;
        set => ViewModel.Variant = value;
    }
    public string Title
    {
        get => ViewModel.Title;
        set => ViewModel.Title = value;
    }
    public string Message
    {
        get => ViewModel.Message;
        set => ViewModel.Message = value;
    }
    public bool IsOpen
    {
        get => ViewModel.IsOpen;
        set => ViewModel.IsOpen = value;
    }
    public Toast()
    {
        this.InitializeComponent();
    }
}
public class ToastVariantIsIconVisibleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not ToastVariant) return true;
        return (ToastVariant)value != ToastVariant.Progressing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}
public class ToastVariantSeverityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not ToastVariant) return InfoBarSeverity.Informational;
        switch (value)
        {
            case ToastVariant.Warning:
                return InfoBarSeverity.Warning;
            case ToastVariant.Error:
                return InfoBarSeverity.Error;
            case ToastVariant.Success:
                return InfoBarSeverity.Success;
            default:
                return InfoBarSeverity.Informational;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
public class ToastVariantProgressBarVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is ToastVariant && (ToastVariant)value == ToastVariant.Progressing ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}
