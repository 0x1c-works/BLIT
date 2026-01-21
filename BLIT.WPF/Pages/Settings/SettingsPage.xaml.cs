using BLIT.WPF.Helpers;
using BLIT.WPF.Services;
using System.Windows;
using System.Windows.Controls;

namespace BLIT.WPF.Pages.Settings;

public partial class SettingsPage : Page {
    private readonly ISettingsService? _settings = AppServices.Get<ISettingsService>();

    public SettingsPage() {
        InitializeComponent();
        DataContext = _settings;
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (ThemeComboBox.SelectedIndex >= 0) {
            var theme = (ThemeHelper.Theme)ThemeComboBox.SelectedIndex;
            ThemeHelper.SetTheme(theme);
        }
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        // Language change requires app restart
        if (LanguageComboBox.SelectedIndex >= 0) {
            var lang = LanguageComboBox.SelectedIndex == 0 ? "en-US" : "zh-CN";
            I18n.SetLanguage(lang);
            
            MessageBox.Show(
                I18n.Current.GetString("DialogChangeLanguage.Content"),
                I18n.Current.GetString("DialogChangeLanguage.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
