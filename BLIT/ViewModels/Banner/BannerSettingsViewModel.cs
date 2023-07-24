using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
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

    public BannerSettingsViewModel()
    {
        this.WhenAnyValue(x => x.SelectedSpriteScanPath)
            .Select(x => x != null)
            .ToPropertyEx(this, x => x.HasSelectedSpriteScanPath);

        AddSpriteScanPath = ReactiveCommand.Create(() => {
            var pathVm = new BannerSpriteScanPathViewModel(DeletePath);
            SpriteScanPaths.Add(pathVm);
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

    void DeletePath(BannerSpriteScanPathViewModel path)
    {
        SpriteScanPaths.Remove(path);
    }
}
