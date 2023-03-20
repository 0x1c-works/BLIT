using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BannerlordImageTool.Win.Common;

public class DialogHelper
{
    public static Task<ContentDialogResult> ShowDangerConfirmDialog(UIElement sender, string title, string content)
    {
        var dialog = new ContentDialog() {
            XamlRoot = sender.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = title,
            PrimaryButtonStyle = Application.Current.Resources["DangerButton"] as Style,
            PrimaryButtonText = I18n.Current.GetString("Yes"),
            SecondaryButtonText = I18n.Current.GetString("No"),
            DefaultButton = ContentDialogButton.Secondary,
            Content = new TextBlock() {
                Text = content,
            }
        };
        return dialog.ShowAsync().AsTask();
    }
}
