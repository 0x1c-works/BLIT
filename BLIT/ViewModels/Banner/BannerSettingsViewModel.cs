using BLIT.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace BLIT.ViewModels.Banner;

public class BannerSettingsViewModel : ReactiveObject
{
    public ObservableCollection<BannerSpriteScanPathViewModel> SpriteScanPaths { get; } = new();
    [Reactive] public BannerSpriteScanPathViewModel? SelectedSpriteScanPath { get; set; }
    [ObservableAsProperty] public bool HasSelectedSpriteScanPath { get; }

    public ReactiveCommand<Unit, Unit> AddSpriteScanPath { get; }
    public ReactiveCommand<Unit, Unit> EditSpriteScanPath { get; }
    public ReactiveCommand<Unit, Unit> DeleteSpriteScanPath { get; }

    BannerSettings _settings;

    public BannerSettingsViewModel(BannerSettings settings)
    {
        _settings = settings;
        SpriteScanPaths.AddRange(_settings.SpriteScanPaths.Select(path => CreateNewPathVm(path)));

        this.WhenAnyValue(x => x.SelectedSpriteScanPath)
            .Select(x => x != null)
            .ToPropertyEx(this, x => x.HasSelectedSpriteScanPath);

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
        _settings.SyncSpriteScanPaths(SpriteScanPaths.Select(pathVm => pathVm.Path));
    }
}
