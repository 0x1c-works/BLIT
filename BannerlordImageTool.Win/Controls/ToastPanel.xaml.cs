using BannerlordImageTool.Win.Services;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Controls;

public sealed partial class ToastPanel : UserControl
{
    Visibility DebugButtonVisibility { get; } = Visibility.Collapsed;
    public ToastPanel()
    {
        InitializeComponent();
        BindDebugAutoTestButtons();
        AppServices.Get<INotificationService>().OnNotify += NotificationService_OnNotify;
    }

    Toast NotificationService_OnNotify(Notification notification)
    {
        return AddToast(notification);
    }

    void rootCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateContainerPosition();
    }
    void container_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateContainerPosition();
    }
    void UpdateContainerPosition()
    {
        Canvas.SetLeft(container, rootCanvas.ActualWidth - container.ActualWidth);
        Canvas.SetTop(container, rootCanvas.ActualHeight - container.ActualHeight);
    }
    Toast AddToast(Notification notification)
    {
        Toast toast = notification.CreateToast();
        toast.OnClosed += (t) => {
            _ = container.Children.Remove(t);
        };
        container.Children.Add(toast);
        return toast;
    }

    #region Tester
    const string AUTO_TEST_BTN_PREFIX = "btnAutoTest";
    void BindDebugAutoTestButtons()
    {
        debugButtons.Children.Where(c => c is Button && (c as Button).Name.StartsWith(AUTO_TEST_BTN_PREFIX))
            .Cast<Button>()
            .ToList()
            .ForEach(b => b.Click += AutoTestButtonClick);
    }
    void AutoTestButtonClick(object sender, RoutedEventArgs e)
    {
        var variantName = (sender as Button).Name.Replace(AUTO_TEST_BTN_PREFIX, "");
        if (Enum.TryParse<ToastVariant>(variantName, out ToastVariant variant))
        {
            AddTestToast(variant);
        }
    }
    void AddTestToast(ToastVariant variant)
    {
        var toast = new Toast() {
            Title = "Test Notification",
            Message = $"Heyhey {Enum.GetName(variant)} > {DateTime.Now.ToLongTimeString()}",
            Variant = variant,
            IsOpen = true,
        };
        container.Children.Add(toast);
    }

    void btnTestTimeout_Click(object sender, RoutedEventArgs e)
    {
        var btn = new Button() { Content = "Refresh", };
        btn.Click += (s, e) => {
            Toast toast = (s as Button).FindAscendant<Toast>();
            if (toast != null)
            {
                toast.TimeoutSeconds = 2;
            }
        };
        var toast = new Toast() {
            Title = "Timeout Test",
            Message = "I'm gonna close in 2 seconds.",
            IsOpen = true,
            TimeoutSeconds = 2,
            ActionButton = btn,
        };

        container.Children.Add(toast);
    }

    void btnTestNoTimeout_Click(object sender, RoutedEventArgs e)
    {
        var toast = new Toast() {
            Title = "Timeout 0s Test",
            Message = "I'm not gonna close automatically.",
            IsOpen = true,
            TimeoutSeconds = 0,
        };
        container.Children.Add(toast);
    }
    #endregion
}

