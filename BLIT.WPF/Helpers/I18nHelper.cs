using System.Globalization;
using System.IO;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace BLIT.WPF.Helpers;

public class I18n {
    private static I18n? _current;
    public static I18n Current => _current ??= new I18n();

    private readonly ResourceManager _resManager;
    
    // Supported language codes in order of preference
    private static readonly string[] SupportedLanguages = { "zh-CN", "en-US" };

    internal I18n() {
        _resManager = new ResourceManager("BLIT.WPF.Properties.Resources", typeof(I18n).Assembly);
    }

    public string GetString(string id) {
        try {
            var value = _resManager.GetString(id, CultureInfo.CurrentUICulture);
            return value ?? id;
        } catch {
            return id;
        }
    }

    /// <summary>
    /// Initializes language on app startup based on saved preference or system language
    /// </summary>
    public static void InitializeLanguage() {
        var savedLanguage = GetSavedLanguage();
        
        if (!string.IsNullOrEmpty(savedLanguage) && savedLanguage != "en-US") {
            // Use saved language if available
            System.Diagnostics.Debug.WriteLine($"[I18n] Using saved language: {savedLanguage}");
            SetLanguageInternal(savedLanguage);
        } else {
            // Try to get system language and find closest match
            var systemLanguageCode = CultureInfo.InstalledUICulture.Name;
            System.Diagnostics.Debug.WriteLine($"[I18n] System language: {systemLanguageCode}");
            
            var systemLanguage = GetClosestSupportedLanguage(systemLanguageCode);
            System.Diagnostics.Debug.WriteLine($"[I18n] Matched language: {systemLanguage}");
            
            if (systemLanguage != "en-US") {
                SetLanguageInternal(systemLanguage);
                SaveLanguagePreference(systemLanguage);
            }
        }
    }

    public static void SetLanguage(string languageCode) {
        SetLanguageInternal(languageCode);
        SaveLanguagePreference(languageCode);
        NotifyLanguageChanged();
    }

    /// <summary>
    /// Internal method to set the language without saving
    /// </summary>
    private static void SetLanguageInternal(string languageCode) {
        var culture = new CultureInfo(languageCode);
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        _current = new I18n(); // Recreate to use new culture
    }

    /// <summary>
    /// Finds the closest supported language based on the system language
    /// For example, zh-TW (Traditional Chinese) will be matched to zh-CN (Simplified Chinese)
    /// </summary>
    private static string GetClosestSupportedLanguage(string systemLanguage) {
        // Exact match first
        if (SupportedLanguages.Contains(systemLanguage, StringComparer.OrdinalIgnoreCase)) {
            return systemLanguage;
        }

        // Try to match language family (e.g., "zh" for any Chinese variant)
        var systemLanguageFamily = systemLanguage.Split('-')[0].ToLower();
        foreach (var supported in SupportedLanguages) {
            var supportedFamily = supported.Split('-')[0].ToLower();
            if (systemLanguageFamily == supportedFamily) {
                return supported;
            }
        }

        // No match found, fallback to en-US
        return "en-US";
    }

    /// <summary>
    /// Gets the saved language preference
    /// </summary>
    public static string GetSavedLanguage() {
        try {
            var settingsPath = GetLanguageSettingsFilePath();
            if (File.Exists(settingsPath)) {
                var saved = File.ReadAllText(settingsPath).Trim();
                // Validate that the saved language is in our supported languages
                if (SupportedLanguages.Contains(saved, StringComparer.OrdinalIgnoreCase)) {
                    return saved;
                }
            }
        } catch {
            // Ignore errors
        }
        return "en-US";
    }

    /// <summary>
    /// Saves the language preference to file
    /// </summary>
    private static void SaveLanguagePreference(string languageCode) {
        try {
            var settingsPath = GetLanguageSettingsFilePath();
            var dir = Path.GetDirectoryName(settingsPath);
            if (!string.IsNullOrEmpty(dir)) {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(settingsPath, languageCode);
        } catch {
            // Ignore errors
        }
    }

    /// <summary>
    /// Notifies all windows that the language has changed so they can update bindings
    /// </summary>
    private static void NotifyLanguageChanged() {
        // Update all windows using I18n markup extensions
        foreach (Window window in Application.Current?.Windows ?? new WindowCollection()) {
            if (window != null) {
                UpdateWindowLanguage(window);
            }
        }
    }

    /// <summary>
    /// Recursively updates all I18n bindings in a window
    /// </summary>
    private static void UpdateWindowLanguage(DependencyObject obj) {
        try {
            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < childrenCount; i++) {
                try {
                    var child = VisualTreeHelper.GetChild(obj, i);
                    if (child is FrameworkElement element) {
                        // Get all properties that have bindings and check if they use LocalizationConverter
                        var localValueEnumerator = element.GetLocalValueEnumerator();
                        var bindingsToRefresh = new List<DependencyProperty>();
                        
                        while (localValueEnumerator.MoveNext()) {
                            var prop = localValueEnumerator.Current.Property;
                            var binding = BindingOperations.GetBinding(element, prop);
                            if (binding != null && binding.Converter is LocalizationConverter) {
                                bindingsToRefresh.Add(prop);
                            }
                        }
                        
                        // Refresh bindings that use LocalizationConverter
                        foreach (var prop in bindingsToRefresh) {
                            RefreshBinding(element, prop);
                        }
                        
                        // Also try to refresh common text properties
                        RefreshBinding(element, TextBlock.TextProperty);
                        RefreshBinding(element, ContentControl.ContentProperty);
                    }
                    
                    // Recursively update children
                    if (child != null) {
                        UpdateWindowLanguage(child);
                    }
                } catch {
                    // Ignore errors in individual children
                }
            }
        } catch {
            // Ignore errors
        }
    }

    private static void RefreshBinding(DependencyObject obj, DependencyProperty property) {
        try {
            var binding = BindingOperations.GetBinding(obj, property);
            if (binding != null) {
                BindingOperations.ClearBinding(obj, property);
                BindingOperations.SetBinding(obj, property, binding);
            }
        } catch {
            // Ignore binding errors
        }
    }

    private static string GetLanguageSettingsFilePath() {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "BLIT.WPF", "language");
    }
}

/// <summary>
/// XAML Markup Extension for localized strings with binding support.
/// Usage: {helpers:I18n KeyName}
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
public class I18nExtension : MarkupExtension {
    public string Key { get; set; } = string.Empty;

    public I18nExtension() { }

    public I18nExtension(string key) {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
        if (string.IsNullOrEmpty(Key)) {
            return string.Empty;
        }
        
        // Create a binding with LocalizationConverter to enable dynamic updates
        var targetProvider = serviceProvider?.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        if (targetProvider?.TargetObject is DependencyObject targetObject && targetProvider?.TargetProperty is DependencyProperty) {
            var binding = new Binding {
                Source = I18n.Current,
                Path = new PropertyPath(nameof(I18n.Current)),
                Mode = BindingMode.OneWay,
                Converter = new LocalizationConverter(Key),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            return binding.ProvideValue(serviceProvider);
        }

        // Fallback to direct value if binding cannot be created
        return I18n.Current.GetString(Key);
    }
}

/// <summary>
/// Converter that provides localized strings and updates when language changes
/// </summary>
public class LocalizationConverter : IValueConverter {
    private readonly string _key;

    public LocalizationConverter(string key) {
        _key = key;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return I18n.Current.GetString(_key);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
