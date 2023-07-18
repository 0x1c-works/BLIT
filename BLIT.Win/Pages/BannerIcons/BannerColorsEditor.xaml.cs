// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BLIT.Win.Helpers;
using BLIT.Win.Pages.BannerIcons.Models;
using BLIT.Win.Services;
using Microsoft.AppCenter.Analytics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CP = CommunityToolkit.WinUI.UI.Controls.ColorPicker;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BLIT.Win.Pages.BannerIcons;

public sealed partial class BannerColorsEditor : UserControl
{
    const int TITLE_MAX_COUNT = 3;
    public BannerIconsProject ViewModel
    {
        get => GetValue(ViewModelProperty) as BannerIconsProject;
        set
        {
            SetValue(ViewModelProperty, value);
            Bindings.Update();
        }
    }
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel), typeof(BannerIconsProject), typeof(BannerColorsEditor), new PropertyMetadata(null));

    public IEnumerable<BannerColorEntry> SelectedColors { get => listViewColors.SelectedItems.Cast<BannerColorEntry>(); }
    public BannerColorEntry FirstSelectedColor { get => SelectedColors.FirstOrDefault(); }
    public bool HasSelectedColor { get => SelectedColors.Any(); }
    public bool IsSingleSelected { get => listViewColors.SelectedItems.Count == 1; }
    public bool IsMultipleSelection { get => SelectedColors.Count() > 1; }
    public bool IsForSigil
    {
        get => GetMultiSelectionFlag(c => c.IsForSigil);
        set => SetMultiSelectionFlag((c, v) => c.IsForSigil = v, value);
    }
    public bool IsForBackground
    {
        get => GetMultiSelectionFlag(c => c.IsForBackground);
        set => SetMultiSelectionFlag((c, v) => c.IsForBackground = v, value);
    }
    public string SelectedColorIDs
    {
        get => string.Join(", ", SelectedColors.Take(TITLE_MAX_COUNT).Select(c => c.ID));
    }
    public string MoreColorsText
    {
        get
        {
            var moreCount = SelectedColors.Count() - TITLE_MAX_COUNT;
            return moreCount > 0 ? string.Format(I18n.Current.GetString("AndMore"), moreCount) : string.Empty;
        }
    }

    public BannerColorsEditor()
    {
        InitializeComponent();
    }

    async void btnChangeColor_Click(object sender, RoutedEventArgs e)
    {
        var tag = (sender as Button)?.Tag;
        if (tag is null || tag is not BannerColorEntry vm)
        {
            return;
        }

        ContentDialog dialog = AppServices.Get<IConfirmDialogService>().Create(this);
        dialog.Title = I18n.Current.GetString("DialogSelectColor/Title");
        dialog.PrimaryButtonText = I18n.Current.GetString("ButtonOK/Content");
        dialog.SecondaryButtonText = I18n.Current.GetString("ButtonCancel/Content");
        dialog.DefaultButton = ContentDialogButton.Primary;
        var colorPicker = new CP() { Color = vm.Color };
        dialog.Content = colorPicker;
        ContentDialogResult result = await dialog.ShowAsync().AsTask();
        if (result == ContentDialogResult.Primary)
        {
            vm.Color = colorPicker.Color;
        }
    }
    void btnAdd_Click(object sender, RoutedEventArgs e)
    {
        AddNewColor();
    }

    async void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        await DeleteSelectedColors();
    }

    void AddNewColor()
    {
        ViewModel.AddColor();
        listViewColors.SelectedIndex = listViewColors.SelectedItems.Count - 1;
    }

    async Task DeleteSelectedColors()
    {
        if (!SelectedColors.Any())
        {
            return;
        }

        if (await AppServices.Get<IConfirmDialogService>().ShowDanger(
            this,
            I18n.Current.GetString("DialogDeleteColor/Title"),
            string.Format(I18n.Current.GetString("DialogDeleteColor/Content"), SelectedColors.Count()))
            != ContentDialogResult.Primary)
        {
            return;
        }

        var index = listViewColors.SelectedIndex;
        ViewModel.DeleteColors(SelectedColors);
        var count = listViewColors.SelectedItems.Count;
        listViewColors.SelectedIndex = count > 0 ? Math.Min(index, count - 1) : -1;
    }

    void listViewColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Bindings.Update();
    }

    bool GetMultiSelectionFlag(Func<BannerColorEntry, bool> getter)
    {
        if (!HasSelectedColor) return false;
        return SelectedColors.All(c => getter?.Invoke(c) ?? false);
    }
    void SetMultiSelectionFlag(Action<BannerColorEntry, bool> setter, bool value)
    {
        foreach (BannerColorEntry item in SelectedColors)
        {
            setter?.Invoke(item, value);
        }
    }

    void lnkColorWarning_Click(object sender, RoutedEventArgs e)
    {
        Analytics.TrackEvent("Visit help", new Dictionary<string, string> {
            {"source", "color warning" }
        });
    }
}
