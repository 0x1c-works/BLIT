using BannerlordImageTool.Win.Common;
using System.Collections.Generic;
using Windows.UI;

namespace BannerlordImageTool.Win.ViewModels.BannerIcons;

public class ColorsEditorViewModel : BindableBase
{
}

public class ColorViewModel : BindableBase
{
    private int _id;
    private Color _color;
    private bool _isForSigil = true;
    private bool _isForBackground = true;

    public int ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    public Color Color
    {
        get => _color;
        set => SetProperty(ref _color, value);
    }
    public bool IsForSigil
    {
        get => _isForSigil;
        set => SetProperty(ref _isForSigil, value);
    }
    public bool IsForBackground
    {
        get => _isForBackground;
        set => SetProperty(ref _isForBackground, value);
    }

}
