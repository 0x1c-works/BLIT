using BannerlordImageTool.Win.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace BannerlordImageTool.Win.Services;

public interface IConfirmDialogService
{
    ContentDialog Create(UIElement sender);
    Task<ContentDialogResult> Show(UIElement sender, Action<ContentDialog> customizer);
    Task<ContentDialogResult> ShowDanger(UIElement sender, string title, string content);
}

public class ConfirmDialogService : IConfirmDialogService
{
    public ContentDialog Create(UIElement sender)
    {
        return new ContentDialog() {
            XamlRoot = sender.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
        };
    }
    public Task<ContentDialogResult> Show(UIElement sender, Action<ContentDialog> customizer)
    {
        var dialog = Create(sender);
        customizer(dialog);
        return dialog.ShowAsync().AsTask();
    }
    public Task<ContentDialogResult> ShowDanger(UIElement sender, string title, string content)
    {
        return Show(sender, (dialog) => {
            dialog.Title = title;
            dialog.PrimaryButtonStyle = Application.Current.Resources["DangerButton"] as Style;
            dialog.PrimaryButtonText = I18n.Current.GetString("Yes");
            dialog.SecondaryButtonText = I18n.Current.GetString("No");
            dialog.DefaultButton = ContentDialogButton.Secondary;
            dialog.Content = new TextBlock() { Text = content };
        });
    }
}
