using BannerlordImageTool.Win.Helpers;
using Microsoft.UI.Xaml.Controls;

namespace BannerlordImageTool.Win.ViewModels.Global;

public class ToastViewModel : BindableBase
{
    private ToastVariant _variant = ToastVariant.Info;
    public ToastVariant Variant
    {
        get => _variant;
        set => SetProperty(ref _variant, value);
    }
    private string _title = "";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
    private string _message = "Toast!";
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }
    private bool _isOpen;
    public bool IsOpen
    {
        get => _isOpen;
        set => SetProperty(ref _isOpen, value);
    }
}
public enum ToastVariant
{
    Info,
    Warning,
    Error,
    Success,
    Progressing,
}

