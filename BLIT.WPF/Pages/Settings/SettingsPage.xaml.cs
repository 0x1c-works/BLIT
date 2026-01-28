using BLIT.WPF.Helpers;
using BLIT.WPF.Services;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace BLIT.WPF.Pages.Settings;

public partial class SettingsPage : Page {
    private readonly ISettingsService? _settings = AppServices.Get<ISettingsService>();
    private bool _isInitialized = false;

    public SettingsPage() {
        InitializeComponent();
        DataContext = _settings;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
        if (!_isInitialized) {
            LoadCurrentSettings();
            _isInitialized = true;
        }
    }

    private void LoadCurrentSettings() {
        // Load current theme
        ThemeComboBox.SelectedIndex = (int)ThemeHelper.CurrentTheme;
        
        // Load current language based on CurrentUICulture
        var currentLang = CultureInfo.CurrentUICulture.Name;
        if (currentLang.StartsWith("zh", StringComparison.OrdinalIgnoreCase)) {
            LanguageComboBox.SelectedIndex = 1; // 简体中文
        } else {
            LanguageComboBox.SelectedIndex = 0; // English
        }
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (ThemeComboBox.SelectedIndex >= 0 && _isInitialized) {
            var theme = (ThemeHelper.Theme)ThemeComboBox.SelectedIndex;
            ThemeHelper.SetTheme(theme);
        }
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (LanguageComboBox.SelectedIndex >= 0 && _isInitialized) {
            var selectedItem = LanguageComboBox.SelectedItem as ComboBoxItem;
            var lang = selectedItem?.Tag as string ?? "en-US";
            
            // Check if language actually changed
            if (lang != I18n.GetSavedLanguage()) {
                I18n.SetLanguage(lang);
            }
        }
    }
}
