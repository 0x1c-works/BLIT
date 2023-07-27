using BLIT.Services;
using BLIT.ViewModels.Banner.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

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

        _hasSelectedGroup = this.WhenAnyValue(x => x.SelectedGroup).Select(x => x != null).Publish();
        _hasSelectedGroup.Connect().DisposeWith(_disposables);

        _hasSelectedGroup.ToPropertyEx(this, x => x.HasSelectedGroup).DisposeWith(_disposables);


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
