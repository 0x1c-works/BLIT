using BLIT.WPF.Helpers;
using BLIT.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BLIT.WPF.Pages.Settings;

/// <summary>
///     Represents a single scan folder entry with edit state management
/// </summary>
public partial class ScanFolderItem : ObservableObject {
    private string _errorMessage = "";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsViewing))]
    private bool _isEditing;

    private string _relativePath = "";

    public ScanFolderItem() {
        IsEditing = true; // New items start in edit mode
    }

    public ScanFolderItem(string path) {
        RelativePath = path;
        IsEditing = false;
    }

    public string RelativePath {
        get => _relativePath;
        set => SetProperty(ref _relativePath, value);
    }

    public bool IsViewing => !IsEditing;

    public string ErrorMessage {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
}

/// <summary>
///     Editor control for managing banner sprite scan folders
/// </summary>
public partial class BannerSpriteScanFoldersEditor : UserControl {
    private readonly ISettingsService? _settings = AppServices.Get<ISettingsService>();
    private ObservableCollection<ScanFolderItem>? _folders;
    private ScanFolderItem? _previousPathBackup;

    public BannerSpriteScanFoldersEditor() {
        InitializeComponent();
    }

    public ObservableCollection<ScanFolderItem>? ItemsSource {
        get => _folders;
        set {
            if (_folders != value) {
                _folders = value;
                ListViewScanFolders.ItemsSource = _folders;
            }
        }
    }

    /// <summary>
    ///     Load folders from settings
    /// </summary>
    public void LoadFolders(IEnumerable<string> folders) {
        _folders = new ObservableCollection<ScanFolderItem>(
            folders.Select(f => new ScanFolderItem(f))
        );
        ListViewScanFolders.ItemsSource = _folders;
    }

    /// <summary>
    ///     Validate if a path is a legal relative path
    /// </summary>
    private bool IsValidRelativePath(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            return false;
        }

        // Normalize path separators
        path = path.Replace('\\', '/');

        // Check for invalid characters
        var invalidChars = new[] { '|', '*', '?', '"', '<', '>' };
        if (path.Any(c => invalidChars.Contains(c))) {
            return false;
        }

        return true;
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs e) {
        if (_folders == null) {
            return;
        }

        var newItem = new ScanFolderItem();
        _folders.Add(newItem);
        ListViewScanFolders.SelectedItem = newItem;
        ListViewScanFolders.ScrollIntoView(newItem);
    }

    private void BtnEdit_Click(object sender, RoutedEventArgs e) {
        if (ListViewScanFolders.SelectedItem is not ScanFolderItem item) {
            return;
        }

        _previousPathBackup = new ScanFolderItem(item.RelativePath);
        item.IsEditing = true;
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e) {
        if (_folders == null || ListViewScanFolders.SelectedItem is not ScanFolderItem item) {
            return;
        }

        _folders.Remove(item);
        SaveFolders();

        // Update button states
        BtnEdit.IsEnabled = _folders.Count > 0;
        BtnDelete.IsEnabled = _folders.Count > 0;
    }

    private void BtnAccept_Click(object sender, RoutedEventArgs e) {
        if (ListViewScanFolders.SelectedItem is not ScanFolderItem item) {
            return;
        }

        // Validate the path
        if (!IsValidRelativePath(item.RelativePath)) {
            item.ErrorMessage = I18n.Current.GetString("TextInvalidPath.Text");
            return;
        }

        // Check for duplicates (case-insensitive)
        var normalizedPath = item.RelativePath.Replace('\\', '/').ToLower();
        var isDuplicate = _folders?.Any(f =>
            f != item && f.RelativePath.Replace('\\', '/').ToLower() == normalizedPath
        ) ?? false;

        if (isDuplicate) {
            item.ErrorMessage = "Folder already added";
            return;
        }

        // Clear error and exit edit mode
        item.ErrorMessage = "";
        item.IsEditing = false;
        SaveFolders();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) {
        if (ListViewScanFolders.SelectedItem is not ScanFolderItem item) {
            return;
        }

        // If it's a new item with empty path, remove it
        if (string.IsNullOrEmpty(_previousPathBackup?.RelativePath)) {
            if (_folders != null) {
                _folders.Remove(item);
            }
        } else {
            // Restore previous value
            item.RelativePath = _previousPathBackup?.RelativePath ?? "";
            item.IsEditing = false;
        }

        item.ErrorMessage = "";
        SaveFolders();
    }

    private void EditPath_KeyDown(object sender, KeyEventArgs e) {
        if (e.Key == Key.Enter) {
            BtnAccept_Click(null, null);
            e.Handled = true;
        } else if (e.Key == Key.Escape) {
            BtnCancel_Click(null, null);
            e.Handled = true;
        }
    }

    private void ListViewScanFolders_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        var hasSelection = ListViewScanFolders.SelectedItem != null;
        BtnEdit.IsEnabled = hasSelection;
        BtnDelete.IsEnabled = hasSelection;
    }

    /// <summary>
    ///     Save folders back to settings
    /// </summary>
    private void SaveFolders() {
        if (_settings?.Banner == null || _folders == null) {
            return;
        }

        List<string> paths = _folders
            .Where(f => !f.IsEditing && !string.IsNullOrWhiteSpace(f.RelativePath))
            .Select(f => f.RelativePath)
            .ToList();

        _settings.Banner.SaveSpriteScanFolders(paths);
    }
}