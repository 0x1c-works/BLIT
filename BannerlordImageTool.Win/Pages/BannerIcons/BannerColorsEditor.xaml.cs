// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using BannerlordImageTool.Win.ViewModels.BannerIcons;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BannerlordImageTool.Win.Pages.BannerIcons;

public sealed partial class BannerColorsEditor : UserControl
{
    ObservableCollection<ColorViewModel> Colors { get; } = new(){
            new ColorViewModel(){ID=123,Color=Color.FromArgb(255,255,0,0)},
            new ColorViewModel(){ID=234,Color=Color.FromArgb(255,0,255,0)},
        };
    public BannerColorsEditor()
    {
        this.InitializeComponent();
    }
}
