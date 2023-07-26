using BLIT.Banner;
using BLIT.Helpers;
using BLIT.Services;
using MessagePack;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace BLIT.Win.Pages.BannerIcons.Models;
public class BannerIconEntry : ReactiveObject, IDisposable
{
    public delegate BannerIconEntry Factory(BannerGroupEntry groupVm, string texturePath);

    readonly CompositeDisposable _disposables = new();
    readonly BannerGroupEntry _groupViewModel;
    string _texturePath = ImageHelper.BAD_IMAGE_PATH;
    string _spritePath = ImageHelper.BAD_IMAGE_PATH;
    int _cellIndex;
    readonly BannerSettings _settings;

    public string TexturePath
    {
        get => _texturePath;
        set
        {
            var newPath = ImageHelper.IsValidImage(value) ? Path.GetFullPath(value) : ImageHelper.BAD_IMAGE_PATH;
            if (newPath == _texturePath)
            {
                ReloadTexture();
            }
            else
            {
                this.RaiseAndSetIfChanged(ref _texturePath, newPath);
            }
        }
    }
    public string SpritePath
    {
        get => _spritePath;
        set
        {
            var newPath = ImageHelper.IsValidImage(value) ? Path.GetFullPath(value) : ImageHelper.BAD_IMAGE_PATH;
            if (newPath == _spritePath)
            {
                ReloadSprite();
            }
            else
            {
                this.RaiseAndSetIfChanged(ref _spritePath, newPath);
            }
        }
    }
    public int CellIndex
    {
        get => _cellIndex;
        set
        {
            if (value == _cellIndex)
            {
                return;
            }
            this.RaiseAndSetIfChanged(ref _cellIndex, value);
        }
    }
    [ObservableAsProperty] public int AtlasIndex { get; }
    [ObservableAsProperty] public string AtlasName { get; } = string.Empty;
    [ObservableAsProperty] public int ID { get; }

    [ObservableAsProperty] public bool IsValid { get; }

    public BannerIconEntry(BannerGroupEntry groupVm, string texturePath, BannerSettings settings)
    {
        _groupViewModel = groupVm;
        _texturePath = texturePath;
        _settings = settings;
        _settings = settings;

        IConnectableObservable<int> cellIndexChanges = this.WhenAnyValue(x => CellIndex).Publish();
        IConnectableObservable<int> groupIDChanges = _groupViewModel.WhenAnyValue(x => x.GroupID).Publish();

        cellIndexChanges.Select(x => x / (TextureMerger.ROWS * TextureMerger.COLS))
                        .ToPropertyEx(this, x => x.AtlasIndex)
                        .DisposeWith(_disposables);
        cellIndexChanges.CombineLatest(groupIDChanges).Select((x) => BannerUtils.GetIconID(x.Second, x.First))
                        .ToPropertyEx(this, x => x.ID)
                        .DisposeWith(_disposables);

        IConnectableObservable<int> atlasIndexChanges = this.WhenAnyValue(x => x.AtlasIndex).Publish();
        atlasIndexChanges.CombineLatest(groupIDChanges).Select(x => BannerUtils.GetAtlasName(x.Second, x.First))
                         .ToPropertyEx(this, x => AtlasName)
                         .DisposeWith(_disposables);

        Observable.CombineLatest(atlasIndexChanges,
                                 this.WhenAnyValue(x => x.TexturePath),
                                 (atlasIndex, texturePath) => new { atlasIndex, texturePath })
                  .Select((x) => ImageHelper.IsValidImage(x.texturePath) && x.atlasIndex >= 0)
                  .ToPropertyEx(this, x => x.IsValid)
                  .DisposeWith(_disposables);
    }

    public void ReloadSprite()
    {
        if (string.IsNullOrEmpty(SpritePath)) return;
        var oldPath = SpritePath;
        SpritePath = oldPath + "1";
        SpritePath = oldPath;
    }
    public void ReloadTexture()
    {
        if (string.IsNullOrEmpty(TexturePath)) return;
        var oldPath = TexturePath;
        TexturePath = oldPath + "1";
        TexturePath = oldPath;
    }

    public BannerIcon ToBannerIcon()
    {
        return new BannerIcon() {
            ID = ID,
            MaterialName = AtlasName,
            TextureIndex = CellIndex,
            Comment = Path.GetFileNameWithoutExtension(TexturePath),
        };
    }
    public IconSprite ToIconSprite()
    {
        return new(_groupViewModel.GroupID, ID, _spritePath);
    }

    public void AutoScanSprite()
    {
        if (string.IsNullOrEmpty(TexturePath))
        {
            return;
        }

        var dir = Path.GetDirectoryName(TexturePath);
        var filename = Path.GetFileName(TexturePath);
        foreach (var relPath in _settings.SpriteScanPaths)
        {
            var tryPath = Path.Join(dir, relPath, filename);
            if (File.Exists(tryPath))
            {
                SpritePath = tryPath;
                return;
            }
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    [MessagePackObject]
    public class SaveData
    {
        [Key(0)]
        public string TexturePath = string.Empty;
        [Key(1)]
        public string SpritePath = string.Empty;
        [Key(2)]
        public int CellIndex;

        public SaveData(BannerIconEntry vm)
        {
            TexturePath = vm.TexturePath;
            SpritePath = vm.SpritePath;
            CellIndex = vm.CellIndex;
        }
        public SaveData() { }

        public BannerIconEntry Load(BannerGroupEntry groupVM, Factory factory)
        {
            BannerIconEntry vm = factory(groupVM, TexturePath);
            vm.SpritePath = SpritePath;
            vm.CellIndex = CellIndex;
            return vm;
        }
    }
}
