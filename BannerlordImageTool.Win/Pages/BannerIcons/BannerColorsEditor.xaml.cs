// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using BannerlordImageTool.Win.ViewModels.BannerIcons;
using Windows.UI;
using BannerlordImageTool.Win.Common;
using CommunityToolkit.WinUI.UI.Controls;

using CP = CommunityToolkit.WinUI.UI.Controls.ColorPicker;
using System.Linq;
using Microsoft.UI.Xaml.Data;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.BannerIcons;

public sealed partial class BannerColorsEditor : UserControl
{
    ObservableCollection<ColorViewModel> Colors = new() {
            new ColorViewModel(){ID=123,Color=Color.FromArgb(255,255,0,0)},
            new ColorViewModel(){ID=234,Color=Color.FromArgb(255,0,255,0)},
    };

    public BannerColorsEditor()
    {
        this.InitializeComponent();
    }

    private async void btnChangeColor_Click(object sender, RoutedEventArgs e)
    {
        var tag = (sender as Button)?.Tag;
        if (tag is null || tag is not ColorViewModel vm) return;
        var dialog = DialogHelper.GetBaseDialog(this);
        dialog.Title = I18n.Current.GetString("DialogSelectColor/Title");
        dialog.PrimaryButtonText = I18n.Current.GetString("ButtonOK/Content");
        dialog.SecondaryButtonText = I18n.Current.GetString("ButtonCancel/Content");
        dialog.DefaultButton = ContentDialogButton.Primary;
        var colorPicker = new CP() { Color = vm.Color };
        dialog.Content = colorPicker;
        var result = await dialog.ShowAsync().AsTask();
        if (result == ContentDialogResult.Primary)
        {
            vm.Color = colorPicker.Color;
        }
    }
}
