// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Theming;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : ThemedWindow
{
    ViewModel Model { get; } = new ViewModel();

    public MainWindow()
    {
        this.InitializeComponent();
        SetBackdrop(BackdropType.Mica);
    }

    private async void btnSelectBLFolder_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        BindHwnd(picker);
        var folder = await picker.PickSingleFolderAsync();
        if (folder is not null)
        {
            Model.RootFolder = folder;
        }
    }

    private void BindHwnd(object target)
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(target, hwnd);
    }

    public class ViewModel : INotifyPropertyChanged
    {
        private StorageFolder _rootFolder;
        public StorageFolder RootFolder
        {
            get => _rootFolder;
            set
            {
                if (_rootFolder?.Path == value?.Path) return;
                _rootFolder = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            PropertyChanged?.Invoke(this, new(prop));
        }
    }
}
