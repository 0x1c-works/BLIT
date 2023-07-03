// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Pages.Settings.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.Settings
{
    public sealed partial class BannerSpriteScanFolderItem : UserControl
    {
        public BannerSpriteScanFolderViewModel ViewModel
        {
            get => GetValue(ViewModelProperty) as BannerSpriteScanFolderViewModel;
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(BannerSpriteScanFolderViewModel),
            typeof(BannerSpriteScanFolderItem),
            new PropertyMetadata(null));

        public BannerSpriteScanFolderItem()
        {
            this.InitializeComponent();
        }

        private void editPath_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Accept();
            }
            else if (e.Key == Windows.System.VirtualKey.Escape)
            {
                Discard();
            }
        }

        void Accept()
        {
            ViewModel.IsEditing = false;
            ViewModel.RelativePath = editPath.Text;
        }
        void Discard()
        {
            ViewModel.IsEditing = false;
            editPath.Text = ViewModel.RelativePath;
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            Accept();
        }

        private void btnDiscard_Click(object sender, RoutedEventArgs e)
        {
            Discard();
        }
    }
}
