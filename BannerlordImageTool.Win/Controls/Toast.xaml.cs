using BannerlordImageTool.Win.Helpers;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

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
        set
        {
            if (!value) EndTimeout();
            else if (value != ViewModel.IsOpen) StartTimeout();
            ViewModel.IsOpen = value;
        }
    }

    public static readonly DependencyProperty TimeoutSecondsProperty = DependencyProperty.Register(
        nameof(TimeoutSeconds),
        typeof(double),
        typeof(Toast),
        new PropertyMetadata(0.0));

    private CancellationTokenSource _cancelTimeout = new();
    public double TimeoutSeconds
    {
        get => (double)GetValue(TimeoutSecondsProperty);
        set
        {
            SetValue(TimeoutSecondsProperty, value);
            StartTimeout();
        }
    }

    public Toast()
    {
        this.InitializeComponent();
    }

    private void StartTimeout()
    {
        EndTimeout();
        if (!_cancelTimeout.TryReset())
        {
            _cancelTimeout = new CancellationTokenSource();
        }
        if (TimeoutSeconds > 0)
        {
            var timeRemaining = TimeoutSeconds;
            var total = TimeoutSeconds;
            Task.Delay(TimeSpan.FromSeconds(TimeoutSeconds), _cancelTimeout.Token).ContinueWith(t => {
                if (!IsOpen) throw new InvalidOperationException("already closed");
                IsOpen = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
            Task.Run(async () => {
                if (timeRemaining < 0) return;
                var prevTime = DateTime.Now;
                while (!_cancelTimeout.Token.IsCancellationRequested)
                {
                    try
                    {
                        var progress = Math.Max(0, timeRemaining / total * 100);
                        await DispatcherQueue.EnqueueAsync(() => ViewModel.Progress = progress);
                        await Task.Delay(1);
                        timeRemaining -= (DateTime.Now - prevTime).TotalSeconds;
                        prevTime= DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        break;
                    }
                }
            });
        }
    }
    private void EndTimeout()
    {
        _cancelTimeout.Cancel();
    }
}
public class ToastViewModel : BindableBase
{
    private ToastVariant _variant = ToastVariant.Info;
    public ToastVariant Variant
    {
        get => _variant;
        set
        {
            SetProperty(ref _variant, value);
            OnPropertyChanged(nameof(ProgressBarVisibility));
        }
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
    public double _progress = -1;
    public double Progress
    {
        get => _progress;
        set
        {
            SetProperty(ref _progress, value);
            OnPropertyChanged(nameof(ProgressBarVisibility));
        }
    }
    public Visibility ProgressBarVisibility
    {
        get => Progress >= 0 || Variant == ToastVariant.Progressing ? Visibility.Visible : Visibility.Collapsed;
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
public class ToastProgressBarIndeterminateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is not double || (double)value < 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}
