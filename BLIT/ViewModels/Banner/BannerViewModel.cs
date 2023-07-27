using BLIT.Services;
using BLIT.ViewModels.Banner.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;

namespace BLIT.ViewModels.Banner;

public class BannerViewModel : ReactiveObject, IRoutableViewModel, IDisposable
{
    readonly CompositeDisposable _disposables = new();
    public string? UrlPathSegment => "banner";
    public IScreen HostScreen { get; }

    IProjectService<BannerIconsProject> _projectService;
    public BannerIconsProject Project => _projectService.Current!;
    [Reactive] public BannerGroupEntry? SelectedGroup { get; set; }
    IConnectableObservable<bool> _hasSelectedGroup;
    [ObservableAsProperty] public bool HasSelectedGroup { get; }
    [Reactive] public BannerIconEntry? SelectedIcon { get; set; }
    IConnectableObservable<bool> _hasSelectedIcon;
    [ObservableAsProperty] public bool HasSelectedIcon { get; }

    #region Visibilities
    [ObservableAsProperty] public Visibility GroupEditorVisibility { get; } = Visibility.Collapsed;
    [ObservableAsProperty] public Visibility IconDetailsVisibility { get; } = Visibility.Collapsed;
    [ObservableAsProperty] public Visibility GroupEditorPlaceholderVisibility { get; } = Visibility.Visible;
    [ObservableAsProperty] public Visibility IconDetailsPlaceholderVisibility { get; } = Visibility.Collapsed;
    #endregion


    #region Commands
    //public ReactiveCommand<Unit, Unit> NewProject { get; }
    //public ReactiveCommand<Unit, Unit> OpenProject { get; }
    //public ReactiveCommand<bool, Unit> SaveProject { get; }
    //public ReactiveCommand<Unit, Unit> ExportAll { get; }
    //public ReactiveCommand<Unit, Unit> ExportXML { get; }
    public ReactiveCommand<Unit, Unit> AddGroup { get; }
    public ReactiveCommand<Unit, Unit> DeleteGroup { get; }
    //public ReactiveCommand<Unit, Unit> ImportTextures { get; }
    //public ReactiveCommand<Unit, Unit> DeleteTextures { get; }
    //public ReactiveCommand<Unit, Unit> ExportGroup { get; }
    //public ReactiveCommand<Unit, Unit> SelectSprite { get; }
    //public ReactiveCommand<Unit, Unit> ReimportSprite { get; }
    //public ReactiveCommand<Unit, Unit> SelectTexture { get; }
    //public ReactiveCommand<Unit, Unit> ReimportTexture { get; }

    #endregion

    public BannerViewModel(IProjectService<BannerIconsProject> projectService, IScreen? screen = null)
    {
        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
        _projectService = projectService;

        _hasSelectedGroup = this.WhenAnyValue(x => x.SelectedGroup)
            .Select(x => x != null)
            .StartWith(false)
            .Replay(1);
        _hasSelectedGroup.Connect().DisposeWith(_disposables);
        _hasSelectedGroup
            .Do(x => Log.Debug("has selected group: {x}", x))
            .ToPropertyEx(this, x => x.HasSelectedGroup)
            .DisposeWith(_disposables);

        _hasSelectedIcon = this.WhenAnyValue(x => x.SelectedIcon)
            .Do(x => Log.Debug("selected icon: {x}", x))
            .Select(x => x != null)
            .StartWith(false)
            .Replay(1);
        _hasSelectedIcon.Connect().DisposeWith(_disposables);
        _hasSelectedIcon
            .Do(x => Log.Debug("has selected icon: {x}", x))
            .ToPropertyEx(this, x => x.HasSelectedIcon)
            .DisposeWith(_disposables);

        var selectionState = Observable.CombineLatest(_hasSelectedGroup,
                                                      _hasSelectedIcon,
                                                      (group, icon) => new { group, icon })
            .Do(x => Log.Debug("selection state: {x}", x))
            .Replay(1);
        selectionState.Connect().DisposeWith(_disposables);
        selectionState.Select((x) => x.group ? Visibility.Visible : Visibility.Collapsed)
            .ToPropertyEx(this, x => x.GroupEditorVisibility)
            .DisposeWith(_disposables);
        selectionState.Select((x) => !x.group ? Visibility.Visible : Visibility.Collapsed)
            .ToPropertyEx(this, x => x.GroupEditorPlaceholderVisibility)
            .DisposeWith(_disposables);
        selectionState.Select((x) => x.group && x.icon ? Visibility.Visible : Visibility.Collapsed)
            .ToPropertyEx(this, x => x.IconDetailsVisibility)
            .DisposeWith(_disposables);
        selectionState.Select((x) => x.group && !x.icon ? Visibility.Visible : Visibility.Collapsed)
            .ToPropertyEx(this, x => x.IconDetailsPlaceholderVisibility)
            .DisposeWith(_disposables);

        AddGroup = ReactiveCommand.Create(() => { Project.AddGroup(); });
        DeleteGroup = ReactiveCommand.Create<Unit, Unit>(group => {
            if (SelectedGroup != null)
            {
                Project.DeleteGroup(SelectedGroup);
            }
            return Unit.Default;
        }, _hasSelectedGroup);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
