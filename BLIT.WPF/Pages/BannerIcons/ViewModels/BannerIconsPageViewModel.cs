using BLIT.WPF.Helpers;
using BLIT.WPF.Pages.BannerIcons.Models;
using BLIT.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BLIT.WPF.Pages.BannerIcons.ViewModels;

public partial class BannerIconsPageViewModel : ObservableObject {
    // 注入的服务
    private readonly IProjectService<BannerIconsProject>? _project = 
        AppServices.Get<IProjectService<BannerIconsProject>>();
    private readonly IFileDialogService? _fileDialog = 
        AppServices.Get<IFileDialogService>();
    private readonly ILoadingService? _loading = 
        AppServices.Get<ILoadingService>();
    private readonly INotificationService? _notification = 
        AppServices.Get<INotificationService>();

    private static readonly Guid GUID_EXPORT_DIALOG = new("0c5f39f0-1a31-4d85-a9ee-7ad0cfd690b6");
    private static readonly Guid GUID_PROJECT_DIALOG = new("f86d402a-33de-4f62-8c2b-c5e75428c018");

    public BannerIconsPageViewModel() {
        // 初始化时设置 ViewModel，以便订阅事件
        ViewModel = _project?.Current;
    }

    // UI 状态属性
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedGroup))]
    [NotifyPropertyChangedFor(nameof(ShowEmptyHint))]
    private BannerGroupEntry? selectedGroup;

    partial void OnSelectedGroupChanged(BannerGroupEntry? value) {
        DeleteGroupCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanDeleteGroup));
    }

    // 数据模型引用
    private BannerIconsProject? _viewModel;
    public BannerIconsProject? ViewModel {
        get => _viewModel;
        private set {
            if (_viewModel != null) {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }
            _viewModel = value;
            if (_viewModel != null) {
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
                // 立即更新所有命令状态，因为可能在订阅前属性已经改变
                SaveProjectCommand.NotifyCanExecuteChanged();
                SaveProjectAsCommand.NotifyCanExecuteChanged();
                OpenProjectCommand.NotifyCanExecuteChanged();
                AddGroupCommand.NotifyCanExecuteChanged();
                DeleteGroupCommand.NotifyCanExecuteChanged();
                ExportAllCommand.NotifyCanExecuteChanged();
                ExportXMLCommand.NotifyCanExecuteChanged();
            }
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSaveProject));
            OnPropertyChanged(nameof(CanAddGroup));
            OnPropertyChanged(nameof(CanDeleteGroup));
            OnPropertyChanged(nameof(CanExport));
        }
    }

    // 计算属性
    public bool HasSelectedGroup => SelectedGroup != null;
    public bool ShowEmptyHint => !HasSelectedGroup;

    // ============ RelayCommand 们 ============
    
    [RelayCommand]
    public async Task NewProject() {
        if (_project == null) return;
        await _project.NewProject();
        ViewModel = _project.Current;
    }

    [RelayCommand(CanExecute = nameof(CanSaveProject))]
    public async Task SaveProject() {
        await Save(false);
    }

    [RelayCommand(CanExecute = nameof(CanSaveProject))]
    public async Task SaveProjectAs() {
        await Save(true);
    }

    [RelayCommand]
    public async Task OpenProject() {
        if (_project == null || _fileDialog == null || _loading == null) return;

        var openedFilePath = await _fileDialog.OpenFile(GUID_PROJECT_DIALOG, new[] { CommonFileTypes.BannerIconsProject });
        if (string.IsNullOrEmpty(openedFilePath)) {
            return;
        }

        _loading.Show(I18n.Current.GetString("PleaseWait"));
        await _project.Load(openedFilePath);
        
        // 重新设置 ViewModel 以确保新项目的事件处理器被订阅
        // Load() 内部会创建新的 Current，所以必须在这里重新赋值才能触发 setter
        ViewModel = _project.Current;
        SelectedGroup = ViewModel?.Groups.FirstOrDefault();

        // 等待 UI 更新
        await Task.Delay(200);
        _loading.Hide();
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    public async Task ExportAll() {
        var outFolderPath = await SelectOutFolder();
        if (string.IsNullOrEmpty(outFolderPath)) return;

        await DoExportAsync(async () => {
            var outDir = await ViewModel!.ExportAll(outFolderPath);
            _notification?.Notify(new(
                ToastVariant.Success,
                Message: string.Format(I18n.Current.GetString("ExportSuccess"), outDir),
                Action: new(
                    I18n.Current.GetString("ButtonOpenFolder.Content"),
                    (s, e) => FileHelpers.OpenFolderInExplorer(outDir))));
        });
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    public async Task ExportXML() {
        var outFolderPath = await SelectOutFolder();
        if (string.IsNullOrEmpty(outFolderPath)) return;

        await DoExportAsync(() => {
            var outDir = ViewModel!.ExportXML(outFolderPath);
            _notification?.Notify(new(
                ToastVariant.Success,
                Message: string.Format(I18n.Current.GetString("SaveXMLSuccess"), Path.Join(outDir, "banner_icons.xml")),
                Action: new(
                    I18n.Current.GetString("ButtonOpenFolder.Content"),
                    (s, e) => FileHelpers.OpenFolderInExplorer(outDir ?? ""))));
            return Task.CompletedTask;
        });
    }

    [RelayCommand(CanExecute = nameof(CanAddGroup))]
    public void AddGroup() {
        if (ViewModel == null) return;

        ViewModel.AddGroup();
        SelectedGroup = ViewModel.Groups.LastOrDefault();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteGroup))]
    public async Task DeleteGroup() {
        if (!HasSelectedGroup || ViewModel == null) {
            return;
        }

        var confirmDialog = AppServices.Get<IConfirmDialogService>();
        if (confirmDialog == null) return;

        var result = await confirmDialog.ShowDanger(
            I18n.Current.GetString("DialogDeleteBannerGroup.Title"),
            string.Format(I18n.Current.GetString("DialogDeleteBannerGroup.Content"), SelectedGroup!.GroupID));

        if (result != ContentDialogResult.Primary) {
            return;
        }

        var selectedIndex = ViewModel.Groups.IndexOf(SelectedGroup);
        ViewModel.DeleteGroup(SelectedGroup);

        if (ViewModel.Groups.Count > 0) {
            SelectedGroup = ViewModel.Groups[Math.Min(selectedIndex, ViewModel.Groups.Count - 1)];
        } else {
            SelectedGroup = null;
        }
    }

    // ============ CanExecute 方法 ============

    private bool CanSaveProject => ViewModel != null && !ViewModel.IsSavingOrLoading;
    private bool CanAddGroup => ViewModel != null && !ViewModel.IsSavingOrLoading;
    private bool CanDeleteGroup => HasSelectedGroup && ViewModel != null && !ViewModel.IsSavingOrLoading;
    private bool CanExport => ViewModel?.CanExport ?? false;

    // ============ 辅助方法 ============

    private async Task<string?> SelectOutFolder() {
        try {
            return await _fileDialog!.OpenFolder(GUID_EXPORT_DIALOG);
        } catch (FileNotFoundException ex) {
            _notification?.Notify(new(ToastVariant.Error,
                                     string.Format(I18n.Current.GetString("TargetPathNotFound"), ex.Message)));
        }
        return null;
    }

    private async Task DoExportAsync(Func<Task> work) {
        if (ViewModel == null || ViewModel.IsExporting) {
            return;
        }

        _loading?.Show(I18n.Current.GetString("TextExporting.Text"));
        try {
            ViewModel.IsExporting = true;
            await work();
        } catch (Exception ex) {
            _notification?.Notify(new(
                ToastVariant.Error,
                Message: ex.Message,
                Title: string.Format(
                    I18n.Current.GetString("ErrorWhen"),
                    I18n.Current.GetString("OperationExporting"))));
        } finally {
            _loading?.Hide();
            ViewModel.IsExporting = false;
        }
    }

    private async Task Save(bool force) {
        if (_project == null || _fileDialog == null || _loading == null) return;

        var filePath = _project.CurrentFile;
        if (force || string.IsNullOrEmpty(filePath)) {
            filePath = _fileDialog.SaveFile(GUID_PROJECT_DIALOG,
                                            new[] { CommonFileTypes.BannerIconsProject },
                                            "banner_icons",
                                            filePath);
        }

        if (string.IsNullOrEmpty(filePath)) {
            return;
        }

        _loading.Show(I18n.Current.GetString("PleaseWait"));
        await _project.Save(filePath);

        // 等待 UI 更新
        await Task.Delay(200);
        _loading.Hide();
    }

     // ============ 事件处理 ============

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
        // 当 ViewModel 的依赖属性改变时，通知相关命令和计算属性更新
        if (e.PropertyName == nameof(BannerIconsProject.IsSavingOrLoading)) {
            SaveProjectCommand.NotifyCanExecuteChanged();
            SaveProjectAsCommand.NotifyCanExecuteChanged();
            OpenProjectCommand.NotifyCanExecuteChanged();
            AddGroupCommand.NotifyCanExecuteChanged();
            DeleteGroupCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanSaveProject));
            OnPropertyChanged(nameof(CanAddGroup));
            OnPropertyChanged(nameof(CanDeleteGroup));
        } else if (e.PropertyName == nameof(BannerIconsProject.CanExport) || 
                   e.PropertyName == nameof(BannerIconsProject.IsExporting)) {
            ExportAllCommand.NotifyCanExecuteChanged();
            ExportXMLCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanExport));
        }
    }

    public void OnSelectionChanged(BannerGroupEntry? group) {
        SelectedGroup = group;
    }
}
