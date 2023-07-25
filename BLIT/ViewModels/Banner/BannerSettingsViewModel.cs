using BLIT.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace BLIT.ViewModels.Banner;

public class BannerSettingsViewModel : ReactiveObject, IDisposable
{
    CompositeDisposable _disposables = new CompositeDisposable();
    public ObservableCollection<BannerSpriteScanPathViewModel> SpriteScanPaths { get; } = new();
    [Reactive] public BannerSpriteScanPathViewModel? SelectedSpriteScanPath { get; set; }
    [ObservableAsProperty] public bool HasSelectedSpriteScanPath { get; }

    public int CustomGroupStartID { get => _settings.CustomGroupStartID; set => _settings.CustomGroupStartID = value; }
    public int CustomColorStartID { get => _settings.CustomColorStartID; set => _settings.CustomColorStartID = value; }

    public ReactiveCommand<Unit, Unit> AddSpriteScanPath { get; }
    public ReactiveCommand<Unit, Unit> EditSpriteScanPath { get; }
    public ReactiveCommand<Unit, Unit> DeleteSpriteScanPath { get; }

    BannerSettings _settings;

    public BannerSettingsViewModel(BannerSettings settings)
    {
        _settings = settings;
        SpriteScanPaths.AddRange(_settings.SpriteScanPaths.Select(path => CreateNewPathVm(path)));

        SpriteScanPaths.CollectionChanged += SpriteScanPaths_CollectionChanged;
        this.WhenAnyValue(x => x.SelectedSpriteScanPath)
            .Select(x => x != null)
            .ToPropertyEx(this, x => x.HasSelectedSpriteScanPath)
            .DisposeWith(_disposables);

        AddSpriteScanPath = ReactiveCommand.Create(() => {
            SpriteScanPaths.Add(CreateNewPathVm(string.Empty));
        });
        EditSpriteScanPath = ReactiveCommand.Create(() => {
            if (SelectedSpriteScanPath != null)
            {
                SelectedSpriteScanPath.IsEditing = true;
            }
        });
        DeleteSpriteScanPath = ReactiveCommand.Create(() => {
            if (SelectedSpriteScanPath != null)
            {
                SpriteScanPaths.Remove(SelectedSpriteScanPath);
            }
        });

    }

    void SpriteScanPaths_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        SyncSpriteScanPaths();
    }

    BannerSpriteScanPathViewModel CreateNewPathVm(string path)
    {
        return new BannerSpriteScanPathViewModel(OnPathChanged, DeletePath) { Path = path };
    }
    void DeletePath(BannerSpriteScanPathViewModel path)
    {
        SpriteScanPaths.Remove(path);
    }
    void OnPathChanged(BannerSpriteScanPathViewModel path)
    {
        SyncSpriteScanPaths();
    }
    void SyncSpriteScanPaths()
    {
        _settings.SpriteScanPaths = SpriteScanPaths.Select(pathVm => pathVm.Path).ToArray();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
