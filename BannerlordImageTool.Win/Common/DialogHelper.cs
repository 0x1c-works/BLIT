using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace BannerlordImageTool.Win.Common;

public class DialogHelper
{
    public static Task<ContentDialogResult> ShowDangerConfirmDialog(UIElement sender, string title, string content)
    {
        var dialog = GetBaseDialog(sender);
        dialog.Title = title;
        dialog.PrimaryButtonStyle = Application.Current.Resources["DangerButton"] as Style;
        dialog.PrimaryButtonText = I18n.Current.GetString("Yes");
        dialog.SecondaryButtonText = I18n.Current.GetString("No");
        dialog.DefaultButton = ContentDialogButton.Secondary;
        dialog.Content = new TextBlock() { Text = content };
        return dialog.ShowAsync().AsTask();
    }

    public static ContentDialog GetBaseDialog(UIElement sender)
    {
        return new ContentDialog() {
            XamlRoot = sender.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
        };
    }
}
