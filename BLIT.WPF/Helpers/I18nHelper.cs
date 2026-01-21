using System;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Markup;

namespace BLIT.WPF.Helpers;

public class I18n {
    private static I18n? _current;
    public static I18n Current => _current ??= new I18n();

    private readonly ResourceManager _resManager;

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

    public static void SetLanguage(string languageCode) {
        var culture = new CultureInfo(languageCode);
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        _current = new I18n(); // Recreate to use new culture
    }
}

/// <summary>
/// XAML Markup Extension for localized strings.
/// Usage: {local:I18n KeyName}
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
        return I18n.Current.GetString(Key);
    }
}
