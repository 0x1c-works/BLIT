// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Settings;
using BannerlordImageTool.Win.ViewModels.Settings;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        SettingsViewModel ViewModel { get; } = new();

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        public GlobalSettings Settings { get => GlobalSettings.Current; }

        private void btnOpenLogFolder_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Logging.Folder);
        }
    }

}
