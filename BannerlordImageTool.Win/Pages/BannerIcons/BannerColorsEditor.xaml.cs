// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Helpers;
using BannerlordImageTool.Win.Pages.BannerIcons.ViewModels;
using BannerlordImageTool.Win.Services;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CP = CommunityToolkit.WinUI.UI.Controls.ColorPicker;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.BannerIcons;

public sealed partial class BannerColorsEditor : UserControl, INotifyPropertyChanged
{
    public BannerIconsPageViewModel PageViewModel
    {
        get => GetValue(DataViewModelProperty) as BannerIconsPageViewModel;
        set
        {
            SetValue(DataViewModelProperty, value);
            PropertyChanged?.Invoke(this, new(nameof(PageViewModel)));
        }
    }
    public static readonly DependencyProperty DataViewModelProperty = DependencyProperty.Register(
        nameof(PageViewModel), typeof(BannerIconsPageViewModel), typeof(BannerColorsEditor), new PropertyMetadata(null));

    readonly EditorViewModel editorViewModel;

    public event PropertyChangedEventHandler PropertyChanged;

    public BannerColorsEditor()
    {
        InitializeComponent();
        editorViewModel = new EditorViewModel(dataGrid);
    }

    async void btnChangeColor_Click(object sender, RoutedEventArgs e)
    {
        var tag = (sender as Button)?.Tag;
        if (tag is null || tag is not BannerColorViewModel vm)
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

    void menuItemAdd_Click(object sender, RoutedEventArgs e)
    {
        AddNewColor();
    }

    async void menuItemDelete_Click(object sender, RoutedEventArgs e)
    {
        await DeleteSelectedColors();
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
        PageViewModel.AddColor();
    }

    async Task DeleteSelectedColors()
    {
        IEnumerable<BannerColorViewModel> selection = editorViewModel.Selection;
        if (!selection.Any())
        {
            return;
        }

        if (await AppServices.Get<IConfirmDialogService>().ShowDanger(
            this,
            I18n.Current.GetString("DialogDeleteColor/Title"),
            string.Format(I18n.Current.GetString("DialogDeleteColor/Content"), selection.Count()))
            != ContentDialogResult.Primary)
        {
            return;
        }

        PageViewModel.DeleteColors(selection);
    }

    class EditorViewModel : BindableBase
    {
        readonly DataGrid _dataGrid;

        public IEnumerable<BannerColorViewModel> Selection => _dataGrid.SelectedItems.Cast<BannerColorViewModel>().Where(m => m is not null);
        public bool HasSelection => Selection.Any();

        public EditorViewModel(DataGrid dataGrid)
        {
            _dataGrid = dataGrid;
            _dataGrid.SelectionChanged += _dataGrid_SelectionChanged;
        }

        void _dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasSelection));
        }
    }
}
