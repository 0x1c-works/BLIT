using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;

namespace BLIT.ViewModels.Banner;

public class BannerSpriteScanPathViewModel : ReactiveObject
{
    [Reactive] public string Path { get; set; } = "";
    [ObservableAsProperty] public string DisplayPath { get; } = "";
    [Reactive] public bool IsEditing { get; set; }
    [ObservableAsProperty] public Visibility DisplayVisibility { get; }
    [ObservableAsProperty] public Visibility EditorVisibility { get; }

    public IObservable<bool> EditStateChanged;

    public ReactiveCommand<Unit, Unit> StartEdit { get; }
    public ReactiveCommand<bool, Unit> QuitEdit { get; }
    public ReactiveCommand<Unit, Unit> Delete { get; }

    public BannerSpriteScanPathViewModel(Action<BannerSpriteScanPathViewModel> onChanged, Action<BannerSpriteScanPathViewModel> onDelete)
    {
        this.WhenAnyValue(x => x.IsEditing)
            .Select(x => x ? Visibility.Collapsed : Visibility.Visible)
            .ToPropertyEx(this, x => x.DisplayVisibility);
        this.WhenAnyValue(x => x.DisplayVisibility)
            .Select(x => x == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible)
            .ToPropertyEx(this, x => x.EditorVisibility);
        this.WhenAnyValue(x => x.Path)
           .Select(x => string.IsNullOrEmpty(x) ? "(Empty)" : x)
           .ToPropertyEx(this, x => x.DisplayPath);
        EditStateChanged = this.WhenAnyValue(x => x.IsEditing);
        StartEdit = ReactiveCommand.Create(() => {
            IsEditing = true;
        });
        Delete = ReactiveCommand.Create(() => {
            onDelete(this);
        });
        QuitEdit = ReactiveCommand.Create<bool, Unit>((changed) => {
            IsEditing = false;
            if (changed) { onChanged(this); }
            return Unit.Default;
        });
    }
}
