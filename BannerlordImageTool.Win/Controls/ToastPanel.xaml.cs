using BannerlordImageTool.Win.ViewModels.Global;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Controls;

public sealed partial class ToastPanel : UserControl
{
    public ToastPanel()
    {
        this.InitializeComponent();
        BindAllTestButtons();

    }
    private void rootCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateContainerPosition();
    }
    private void container_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateContainerPosition();
    }
    private void UpdateContainerPosition()
    {
        Canvas.SetLeft(container, rootCanvas.ActualWidth - container.ActualWidth);
        Canvas.SetTop(container, rootCanvas.ActualHeight - container.ActualHeight);
    }
    private void AddToast()
    {
        var toast = new Toast() {
            Message = $"Heyhey {DateTime.Now.ToLongTimeString()}",
            IsOpen = true,
        };
        container.Children.Add(toast);
    }

    #region Tester
    private void BindAllTestButtons()
    {
        testButtons.Children.Where(c => c is Button).Cast<Button>().ToList().ForEach(b => b.Click += TestButton_Click);
    }
    private void TestButton_Click(object sender, RoutedEventArgs e)
    {
        var variantName = (sender as Button).Name.Replace("btnTest", "");
        if (Enum.TryParse<ToastVariant>(variantName, out var variant))
        {
            AddTestToast(variant);
        }
    }
    private void AddTestToast(ToastVariant variant)
    {
        var toast = new Toast() {
            Title = "Test Notification",
            Message = $"Heyhey {Enum.GetName(variant)} > {DateTime.Now.ToLongTimeString()}",
            Variant = variant,
            IsOpen = true,
        };
        container.Children.Add(toast);
    }
    #endregion
}
