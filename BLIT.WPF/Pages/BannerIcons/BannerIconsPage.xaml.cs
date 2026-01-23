using BLIT.WPF.Helpers;
using BLIT.WPF.Pages.BannerIcons.Models;
using BLIT.WPF.Services;
using Sentry;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BLIT.WPF.Pages.BannerIcons;

/// <summary>
/// Banner Icons 编辑页面
/// </summary>
public partial class BannerIconsPage : Page, INotifyPropertyChanged {
    private static readonly Guid GUID_EXPORT_DIALOG = new("0c5f39f0-1a31-4d85-a9ee-7ad0cfd690b6");
    private static readonly Guid GUID_PROJECT_DIALOG = new("f86d402a-33de-4f62-8c2b-c5e75428c018");
    
    private readonly ISettingsService? _settings = AppServices.Get<ISettingsService>();
    private readonly IFileDialogService? _fileDialog = AppServices.Get<IFileDialogService>();
    private readonly IProjectService<BannerIconsProject>? _project = AppServices.Get<IProjectService<BannerIconsProject>>();
    private readonly ILoadingService? _loading = AppServices.Get<ILoadingService>();
    private readonly INotificationService? _notification = AppServices.Get<INotificationService>();

    public BannerIconsProject? ViewModel => _project?.Current;
    public BannerGroupEntry? SelectedGroup => listViewGroups.SelectedItem as BannerGroupEntry;
    public bool HasSelectedGroup => SelectedGroup != null;
    public bool ShowEmptyHint => !HasSelectedGroup;

    public event PropertyChangedEventHandler? PropertyChanged;

    public BannerIconsPage() {
        System.Diagnostics.Debug.WriteLine("BannerIconsPage: Constructor called");
        InitializeComponent();
        
        DataContext = this;
        
        System.Diagnostics.Debug.WriteLine($"BannerIconsPage: Services resolved - _project={_project != null}, _settings={_settings != null}");
        
        if (_project != null) {
            _project.PropertyChanged += OnProjectPropertyChanged;
        }
        
        Loaded += OnPageLoaded;
        
        // 设置输出分辨率默认值
        if (ViewModel != null) {
            var resolutionName = ViewModel.OutputResolutionName;
            if (resolutionName == "2K") {
                comboOutputResolution.SelectedIndex = 0;
            } else if (resolutionName == "4K") {
                comboOutputResolution.SelectedIndex = 1;
            }
        }
        
        System.Diagnostics.Debug.WriteLine($"BannerIconsPage: Initialization complete, ViewModel={ViewModel != null}");
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e) {
        System.Diagnostics.Debug.WriteLine("BannerIconsPage: Page loaded");
        
        // TODO: 显示警告信息并支持跳转
        // // 检查精灵扫描文件夹配置
        // if (_settings?.Banner.SpriteScanFolders.Count == 0) {
        //     _notification?.Notify(new Notification(
        //         ToastVariant.Warning,
        //         Message: I18n.Current.GetString("WarningNoSpriteScanFolders/Message"),
        //         TimeoutSeconds: 30,
        //         Action: new(
        //             I18n.Current.GetString("ButtonToSettings/Content"),
        //             (s, e) => {
        //                 SentrySdk.AddBreadcrumb("open settings", category: "ui.help");
        //                 (Application.Current.MainWindow as MainWindow)?.NavigateToSettings();
        //             })
        //         ));
        // }
    }

    private void OnProjectPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(_project.Current)) {
            OnPropertyChanged(nameof(ViewModel));
            OnPropertyChanged(nameof(HasSelectedGroup));
        }
    }

    private void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // 输出分辨率选择
    private void comboOutputResolution_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (ViewModel == null || comboOutputResolution.SelectedItem == null) return;
        
        var item = comboOutputResolution.SelectedItem as ComboBoxItem;
        var tag = item?.Tag as string;
        if (tag != null) {
            ViewModel.OutputResolutionName = tag;
        }
    }

    // 导出所有
    private async void btnExportAll_Click(object sender, RoutedEventArgs e) {
        var outFolderPath = await SelectOutFolder();
        if (string.IsNullOrEmpty(outFolderPath)) return;

        await DoExportAsync(async () => {
            var outDir = await ViewModel!.ExportAll(outFolderPath);
            _notification?.Notify(new(
                ToastVariant.Success,
                Message: string.Format(I18n.Current.GetString("ExportSuccess"), outDir),
                Action: new(
                    I18n.Current.GetString("ButtonOpenFolder/Content"),
                    (s, e) => FileHelpers.OpenFolderInExplorer(outDir))));
        });
    }

    // 仅导出 XML
    private async void btnExportXML_Click(object sender, RoutedEventArgs e) {
        var outFolderPath = await SelectOutFolder();
        if (string.IsNullOrEmpty(outFolderPath)) return;

        await DoExportAsync(() => {
            var outDir = ViewModel!.ExportXML(outFolderPath);
            _notification?.Notify(new(
                ToastVariant.Success,
                Message: string.Format(I18n.Current.GetString("SaveXMLSuccess"), Path.Join(outDir, "banner_icons.xml")),
                Action: new(
                    I18n.Current.GetString("ButtonOpenFolder/Content"),
                    (s, e) => FileHelpers.OpenFolderInExplorer(outDir ?? ""))));
            return Task.CompletedTask;
        });
    }

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

        _loading?.Show(I18n.Current.GetString("TextExporting/Text"));
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

    // 添加分组
    private void btnAddGroup_Click(object sender, RoutedEventArgs e) {
        if (ViewModel == null) return;
        
        ViewModel.AddGroup();
        listViewGroups.SelectedItem = ViewModel.Groups.LastOrDefault();
    }

    // 删除分组
    private async void btnDeleteGroup_Click(object sender, RoutedEventArgs e) {
        if (!HasSelectedGroup || ViewModel == null) {
            return;
        }

        var result = await _confirmDialog!.ShowDanger(
            I18n.Current.GetString("DialogDeleteBannerGroup/Title"),
            string.Format(I18n.Current.GetString("DialogDeleteBannerGroup/Content"), SelectedGroup!.GroupID));
        
        if (result != ContentDialogResult.Primary) {
            return;
        }
        
        var selectedIndex = listViewGroups.SelectedIndex;
        ViewModel.DeleteGroup(SelectedGroup);
        
        if (listViewGroups.Items.Count > 0) {
            listViewGroups.SelectedIndex = Math.Min(selectedIndex, listViewGroups.Items.Count - 1);
        } else {
            listViewGroups.SelectedItem = null;
        }
    }

    // 新建项目
    private async void btnNewProject_Click(object sender, RoutedEventArgs e) {
        if (_project == null) return;
        await _project.NewProject();
        OnPropertyChanged(nameof(ViewModel));
    }

    // 保存项目
    private void btnSaveProject_Click(object sender, RoutedEventArgs e) {
        Save(false);
    }

    // 打开项目
    private async void btnOpenProject_Click(object sender, RoutedEventArgs e) {
        if (_project == null || _fileDialog == null || _loading == null) return;
        
        var openedFilePath = await _fileDialog.OpenFile(GUID_PROJECT_DIALOG, new[] { CommonFileTypes.BannerIconsProject });
        if (string.IsNullOrEmpty(openedFilePath)) {
            return;
        }
        
        _loading.Show(I18n.Current.GetString("PleaseWait"));
        await _project.Load(openedFilePath);
        listViewGroups.SelectedItem = ViewModel?.Groups.FirstOrDefault();
        
        // 等待 UI 更新
        await Task.Delay(200);
        _loading.Hide();
        
        OnPropertyChanged(nameof(ViewModel));
    }

    // 另存为项目
    private void btnSaveProjectAs_Click(object sender, RoutedEventArgs e) {
        Save(true);
    }

    private async void Save(bool force) {
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

    // 分组列表选择变化
    private void listViewGroups_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        OnPropertyChanged(nameof(SelectedGroup));
        OnPropertyChanged(nameof(HasSelectedGroup));
        OnPropertyChanged(nameof(ShowEmptyHint));
    }
    
    private IConfirmDialogService? _confirmDialog = AppServices.Get<IConfirmDialogService>();
}
