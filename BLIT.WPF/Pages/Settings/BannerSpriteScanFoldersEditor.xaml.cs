using BLIT.WPF.Helpers;
using BLIT.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
public partial class BannerSpriteScanFoldersEditor {
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
        
        // Autofocus to TextBox after edit mode is enabled
        Dispatcher.BeginInvoke(FocusEditTextBox);
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
            BtnAccept_Click(sender, e);
            e.Handled = true;
        } else if (e.Key == Key.Escape) {
            BtnCancel_Click(sender, e);
            e.Handled = true;
        }
    }

    /// <summary>
    ///     Autofocus TextBox when it's loaded (when entering edit mode)
    /// </summary>
    private void EditPath_Loaded(object sender, RoutedEventArgs e) {
        if (sender is TextBox textBox) {
            textBox.Focus();
            textBox.SelectAll();
        }
    }

    /// <summary>
    ///     Handle double-click on list item to enter edit mode
    /// </summary>
    private void ListViewScanFolders_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
        if (ListViewScanFolders.SelectedItem is not ScanFolderItem item) {
            return;
        }

        // Only allow double-click on the display mode (TextBlock area)
        // Check if the click is not on the edit grid buttons
        var originalSource = e.OriginalSource as DependencyObject;
        if (e.OriginalSource is Button || (originalSource != null && FindAncestor<Button>(originalSource) != null)) {
            return;
        }

        // Enable edit mode
        _previousPathBackup = new ScanFolderItem(item.RelativePath);
        item.IsEditing = true;

        // Autofocus to TextBox after edit mode is enabled
        Dispatcher.BeginInvoke(FocusEditTextBox);

        e.Handled = true;
    }

    /// <summary>
    ///     Helper method to find an ancestor element of a specific type
    /// </summary>
    private T? FindAncestor<T>(DependencyObject child) where T : DependencyObject {
        var parent = VisualTreeHelper.GetParent(child);

        if (parent == null) {
            return null;
        }

        if (parent is T ancestor) {
            return ancestor;
        }

        return FindAncestor<T>(parent);
    }

    /// <summary>
    ///     Exit edit mode for a specific item with validation
    ///     If the item has invalid input, cancel the edit; otherwise accept it
    /// </summary>
    private void ExitEditModeForItem(ScanFolderItem item) {
        if (!item.IsEditing) {
            return;
        }

        // Validate the path
        if (!IsValidRelativePath(item.RelativePath)) {
            // Invalid input, perform cancel operation
            if (string.IsNullOrEmpty(_previousPathBackup?.RelativePath)) {
                // New item, remove it
                if (_folders != null) {
                    _folders.Remove(item);
                }
            } else {
                // Existing item, restore previous value
                item.RelativePath = _previousPathBackup?.RelativePath ?? "";
            }
        } else {
            // Check for duplicates (case-insensitive)
            var normalizedPath = item.RelativePath.Replace('\\', '/').ToLower();
            var isDuplicate = _folders?.Any(f =>
                f != item && f.RelativePath.Replace('\\', '/').ToLower() == normalizedPath
            ) ?? false;

            if (isDuplicate) {
                // Duplicate found, restore previous value
                if (string.IsNullOrEmpty(_previousPathBackup?.RelativePath)) {
                    // New item with duplicate, remove it
                    if (_folders != null) {
                        _folders.Remove(item);
                    }
                } else {
                    // Existing item with duplicate, restore previous value
                    item.RelativePath = _previousPathBackup?.RelativePath ?? "";
                }
            }
            // Valid input, accept it
        }

        // Clear error and exit edit mode
        item.ErrorMessage = "";
        item.IsEditing = false;
        SaveFolders();
    }

    /// <summary>
    ///     Focus the TextBox in the currently selected item's edit mode
    /// </summary>
    private void FocusEditTextBox() {
        if (ListViewScanFolders.SelectedItem is not ScanFolderItem item || !item.IsEditing) {
            return;
        }

        // Get the container for the selected item
        var container = ListViewScanFolders.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
        if (container == null) {
            return;
        }

        // Find the TextBox named "EditPath" in the container
        var textBox = FindVisualChild<TextBox>(container, "EditPath");
        if (textBox != null) {
            textBox.Focus();
            textBox.SelectAll();
        }
    }

    /// <summary>
    ///     Helper method to find a child element by name in the visual tree
    /// </summary>
    private T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement {
        var childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++) {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild && typedChild.Name == name) {
                return typedChild;
            }

            var result = FindVisualChild<T>(child, name);
            if (result != null) {
                return result;
            }
        }

        return null;
    }

    private void ListViewScanFolders_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        // Handle exiting edit mode for deselected items
        foreach (var deselectedItem in e.RemovedItems.OfType<ScanFolderItem>()) {
            if (deselectedItem.IsEditing) {
                ExitEditModeForItem(deselectedItem);
            }
        }

        // Update button states for newly selected item
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