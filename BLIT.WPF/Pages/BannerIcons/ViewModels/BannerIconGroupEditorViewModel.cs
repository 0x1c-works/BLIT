using BLIT.WPF.Helpers;
using BLIT.WPF.Pages.BannerIcons.Models;
using BLIT.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

namespace BLIT.WPF.Pages.BannerIcons.ViewModels;

public partial class BannerIconGroupEditorViewModel : ObservableObject {
    private static readonly Guid GUID_TEXTURE_DIALOG = new("8a8429ec-b674-40d8-82f0-ad42be0d6e8f");
    private static readonly Guid GUID_SPRITE_DIALOG = new("7fb7d0f4-e50d-4fa3-a890-ae0775bca3d8");

    private readonly IFileDialogService? _fileDialog = AppServices.Get<IFileDialogService>();
    
    // 记录当前选中的图标，用于管理事件订阅
    private BannerIconEntry? _currentSelectedIcon;

    // 分组数据引用 - 这个由父 ViewModel 的 SelectedGroup 提供
    [ObservableProperty]
    private BannerGroupEntry? groupData;

    // UI 状态属性 - 本地管理的选择状态
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FirstSelectedIcon))]
    [NotifyPropertyChangedFor(nameof(HasSelectedIcons))]
    [NotifyPropertyChangedFor(nameof(CanReimportSprite))]
    [NotifyPropertyChangedFor(nameof(CanReimportTexture))]
    private IEnumerable<BannerIconEntry> selectedIcons = [];

    public bool CanReimportSprite {
        get => FirstSelectedIcon != null && ImageHelper.IsValidImage(FirstSelectedIcon.SpritePath);
    }
    
    public bool CanReimportTexture {
        get => FirstSelectedIcon != null && ImageHelper.IsValidImage(FirstSelectedIcon.TexturePath);
    }

    // 计算属性
    public BannerIconEntry? FirstSelectedIcon => SelectedIcons.FirstOrDefault();
    public bool HasSelectedIcons => SelectedIcons.Any();

    // ============ RelayCommand 们 ============

    [RelayCommand]
    public async Task AddTextures() {
        if (_fileDialog == null || GroupData == null) return;

        var files = await _fileDialog.OpenFiles(GUID_TEXTURE_DIALOG, new[] { CommonFileTypes.Png });
        if (files == null || files.Count == 0) {
            return;
        }

        GroupData.AddIcons(files);
    }

    [RelayCommand(CanExecute = nameof(HasSelectedIcons))]
    public async Task DeleteTextures() {
        if (!HasSelectedIcons || GroupData == null) {
            return;
        }

        var confirmDialog = AppServices.Get<IConfirmDialogService>();
        if (confirmDialog == null) return;

        var result = await confirmDialog.ShowDanger(
            I18n.Current.GetString("DialogDeleteBannerIcon.Title"),
            string.Format(I18n.Current.GetString("DialogDeleteBannerIcon.Content"), SelectedIcons.Count()));

        if (result == ContentDialogResult.Primary) {
            GroupData.DeleteIcons(SelectedIcons);
        }
    }

    [RelayCommand(CanExecute = nameof(CanReimportSprite))]
    public async Task ChangeSprite() {
        if (FirstSelectedIcon == null) return;

        if (_fileDialog == null) return;

        var file = await _fileDialog.OpenFile(GUID_SPRITE_DIALOG, 
                                            FirstSelectedIcon.SpritePath,
                                            new[] { CommonFileTypes.Png });
        if (string.IsNullOrEmpty(file)) {
            return;
        }

        FirstSelectedIcon.SpritePath = file;
    }

    [RelayCommand(CanExecute = nameof(CanReimportTexture))]
    public async Task ChangeTexture() {
        if (FirstSelectedIcon == null) return;

        if (_fileDialog == null) return;

        var file = await _fileDialog.OpenFile(GUID_TEXTURE_DIALOG, 
                                            FirstSelectedIcon.TexturePath,
                                            new[] { CommonFileTypes.Png });
        if (string.IsNullOrEmpty(file)) {
            return;
        }

        FirstSelectedIcon.TexturePath = file;
    }

    [RelayCommand(CanExecute = nameof(CanReimportSprite))]
    public void ReimportSprite() {
        FirstSelectedIcon?.ReloadSprite();
    }

    [RelayCommand(CanExecute = nameof(CanReimportTexture))]
    public void ReimportTexture() {
        FirstSelectedIcon?.ReloadTexture();
    }

    // ============ 事件处理 ============

    partial void OnGroupDataChanged(BannerGroupEntry? oldValue, BannerGroupEntry? newValue) {
        // 当分组数据改变时，清空已选中的图标
        if (oldValue != newValue) {
            SelectedIcons = [];
            _currentSelectedIcon = null;
        }
    }

    private void OnSelectedIconPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(BannerIconEntry.SpritePath)||e.PropertyName == nameof(BannerIconEntry.TexturePath)) {
            UpdateCanReimportAssets();
        }
    }

     private void UpdateCanReimportAssets() {
        // 显式通知 RelayCommand 重新评估 CanExecute
        ChangeSpriteCommand.NotifyCanExecuteChanged();
        ReimportSpriteCommand.NotifyCanExecuteChanged();
        ChangeTextureCommand.NotifyCanExecuteChanged();
        ReimportTextureCommand.NotifyCanExecuteChanged();
    }

    public void OnSelectionChanged(IEnumerable<BannerIconEntry> icons) {
        // 取消旧图标的订阅
        if (_currentSelectedIcon != null) {
            _currentSelectedIcon.PropertyChanged -= OnSelectedIconPropertyChanged;
        }

        // 更新选中的图标集合
        SelectedIcons = icons;
        
        // 更新当前选中图标引用
        _currentSelectedIcon = FirstSelectedIcon;
        
        // 订阅新图标的属性变化
        if (_currentSelectedIcon != null) {
            _currentSelectedIcon.PropertyChanged += OnSelectedIconPropertyChanged;
        }

        // 更新 CanReimport 属性值
        UpdateCanReimportAssets();
    }
}
