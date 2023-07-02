// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Helpers;
using BannerlordImageTool.Win.Services;
using BannerlordImageTool.Win.ViewModels.BannerIcons;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CP = CommunityToolkit.WinUI.UI.Controls.ColorPicker;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.BannerIcons;

public sealed partial class BannerColorsEditor : UserControl
{
    public DataViewModel DataViewModel
    {
        get => GetValue(DataViewModelProperty) as DataViewModel;
        set => SetValue(DataViewModelProperty, value);
    }
    public static readonly DependencyProperty DataViewModelProperty = DependencyProperty.Register(
        nameof(DataViewModel), typeof(DataViewModel), typeof(BannerColorsEditor), new PropertyMetadata(null));

    EditorViewModel editorViewModel;

    public BannerColorsEditor()
    {
        this.InitializeComponent();
        editorViewModel = new EditorViewModel(dataGrid);
    }

    private async void btnChangeColor_Click(object sender, RoutedEventArgs e)
    {
        var tag = (sender as Button)?.Tag;
        if (tag is null || tag is not ColorViewModel vm) return;
        var dialog = AppService.Get<IConfirmDialogService>().Create(this);
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

    private void menuItemAdd_Click(object sender, RoutedEventArgs e)
    {
        AddNewColor();
    }

    private async void menuItemDelete_Click(object sender, RoutedEventArgs e)
    {
        await DeleteSelectedColors();
    }

    private void btnAdd_Click(object sender, RoutedEventArgs e)
    {
        AddNewColor();
    }

    private async void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        await DeleteSelectedColors();
    }

    void AddNewColor()
    {
        DataViewModel.AddColor();
    }

    async Task DeleteSelectedColors()
    {
        var selection = editorViewModel.Selection;
        if (!selection.Any()) return;
        if (await AppService.Get<IConfirmDialogService>().ShowDanger(
            this,
            I18n.Current.GetString("DialogDeleteColor/Title"),
            string.Format(I18n.Current.GetString("DialogDeleteColor/Content"), selection.Count()))
            != ContentDialogResult.Primary)
        {
            return;
        }

        DataViewModel.DeleteColors(selection);
    }

    class EditorViewModel : BindableBase
    {
        private DataGrid _dataGrid;

        public IEnumerable<ColorViewModel> Selection
        {
            get => _dataGrid.SelectedItems.Cast<ColorViewModel>().Where(m => m is not null);
        }
        public bool HasSelection { get => Selection.Any(); }

        public EditorViewModel(DataGrid dataGrid)
        {
            _dataGrid = dataGrid;
            _dataGrid.SelectionChanged += _dataGrid_SelectionChanged;
        }

        private void _dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasSelection));
        }
    }
}
