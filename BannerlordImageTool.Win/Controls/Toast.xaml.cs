using BannerlordImageTool.Win.Helpers;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Serilog;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Controls;

public sealed partial class Toast : UserControl
{
    const double TIMER_INTERVAL = 0.01;
    ToastViewModel ViewModel { get; } = new ToastViewModel();
    PeriodicTimer _countdownTimer = new(TimeSpan.FromSeconds(TIMER_INTERVAL));

    public event Action<Toast> OnClosed;

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
            if (!value)
            {
                StopTimeout();
                if (value != ViewModel.IsOpen) OnClosed?.Invoke(this);
            }
            else if (value != ViewModel.IsOpen)
            {
                StartTimeout();
            }
            ViewModel.IsOpen = value;
        }
    }
    public bool IsClosable
    {
        get => ViewModel.IsClosable;
        set => ViewModel.IsClosable = value;
    }
    public Button ActionButton
    {
        get => ViewModel.ActionButton;
        set => ViewModel.ActionButton = value;
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
        infoBar.Closed += (s, e) => IsOpen = false;
    }

    private void StartTimeout()
    {
        StopTimeout();
        if (!_cancelTimeout.TryReset())
        {
            _cancelTimeout = new CancellationTokenSource();
        }
        if (TimeoutSeconds > 0)
        {
            var cancelToken = _cancelTimeout.Token;
            // close the toast after timeout
            Task.Delay(TimeSpan.FromSeconds(TimeoutSeconds), cancelToken)
                .ContinueWith(t => {
                    if (!IsOpen) throw new InvalidOperationException("already closed");
                    IsOpen = false;
                },
                cancellationToken: cancelToken,
                continuationOptions: TaskContinuationOptions.NotOnCanceled,
                scheduler: TaskScheduler.FromCurrentSynchronizationContext());

            // update the toast's progress bar during the countdown
            var timeRemaining = TimeoutSeconds;
            var total = TimeoutSeconds;
            Task.Run(async () => {
                if (timeRemaining < 0) return;
                var UpdateProgress = new Func<double, Task>(async (t) => {
                    var progress = t / total * 100;
                    await DispatcherQueue.EnqueueAsync(() => ViewModel.Progress = progress);
                });
                await UpdateProgress(timeRemaining);
                var prevTime = DateTime.Now;
                while (!cancelToken.IsCancellationRequested && await _countdownTimer.WaitForNextTickAsync(cancelToken))
                {
                    try
                    {
                        var newTime = DateTime.Now;
                        timeRemaining -= (newTime - prevTime).TotalSeconds;
                        prevTime = newTime;
                        await UpdateProgress(timeRemaining);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Toast countdown error: {Exception}", ex);
                        break;
                    }
                }
            }, cancelToken);
        }
    }
    private void StopTimeout()
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
    private Button _actionButton;
    public Button ActionButton
    {
        get => _actionButton;
        set => SetProperty(ref _actionButton, value);
    }
    private bool _isClosable = true;
    public bool IsClosable
    {
        get => _isClosable;
        set => SetProperty(ref _isClosable, value);
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
