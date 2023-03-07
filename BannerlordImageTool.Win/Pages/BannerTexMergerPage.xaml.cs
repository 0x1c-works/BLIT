// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BannerlordImageTool.Win.Common;
using BannerlordImageTool.Win.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BannerTexMergerPage : Page
{
    public BannerTexMergerViewModel ViewModel { get; } = new();
    public BannerTexMergerPage()
    {
        this.InitializeComponent();
    }

    
    void ResolutionOption_Click(object sender, RoutedEventArgs e)
    {
        if(sender is not MenuFlyoutItem item)
        {
            return;
        }
        ViewModel.OutputResolution = item.Tag as string;
    }
}

public class BannerTexMergerViewModel : BindableBase
{
    public record CellTexture(string filePath);

    private int _groupID;

    public int GroupID
    {
        get => _groupID;
        set => SetProperty(ref _groupID, value);
    }
    public string OutputResolution
    {
        get
        {
            switch (GlobalSettings.Current.BannerTexOutputResolution)
            {
                case BannerTex.OutputResolution.Res2K: return "2K";
                case BannerTex.OutputResolution.Res4K: return "4K";
                default: return I18n.Current.GetString("PleaseSelect");
            }
        }
        set
        {
            if(Enum.TryParse<BannerTex.OutputResolution>(value,out var enumValue))
            {
                GlobalSettings.Current.BannerTexOutputResolution = enumValue;
            }
            else
            {
                GlobalSettings.Current.BannerTexOutputResolution = BannerTex.OutputResolution.INVALID;
            }
            OnPropertyChanged();
        }
    }

    public ObservableCollection<CellTexture> CellTextures { get; } = new();

    internal BannerTexMergerViewModel()
    {
    }

}
