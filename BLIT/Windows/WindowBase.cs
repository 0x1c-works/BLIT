using MahApps.Metro.Controls;
using ReactiveUI;
using System.Windows;

namespace BLIT.Windows;

public abstract class WindowBase<TViewModel> : MetroWindow, IViewFor<TViewModel> where TViewModel : class
{
    /// <summary>
    /// The view model dependency property.
    /// </summary>
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
                                    "ViewModel",
                                    typeof(TViewModel),
                                    typeof(ReactiveWindow<TViewModel>),
                                    new PropertyMetadata(null));

    /// <summary>
    /// Gets the binding root view model.
    /// </summary>
    public TViewModel? BindingRoot => ViewModel;

    /// <inheritdoc/>
    public TViewModel? ViewModel
    {
        get => (TViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TViewModel?)value;
    }
}
