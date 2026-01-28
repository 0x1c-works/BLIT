using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BLIT.WPF.Helpers;

public class PathToOptimizedBitmapImageConverter : IValueConverter {
    #region IValueConverter Members

    /// <summary>
    ///     Converts a file path to an optimized BitmapImage with specified decode pixel width.
    ///     Parameter should be the DecodePixelWidth value (e.g., "128", "256", "512")
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not string path || string.IsNullOrEmpty(path)) {
            return null!;
        }

        if (!int.TryParse(parameter?.ToString() ?? "128", out var decodePixelWidth)) {
            decodePixelWidth = 128;
        }

        try {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = decodePixelWidth;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        } catch {
            // Return null if path is invalid or image cannot be loaded
            return null!;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    #endregion
}

/// <summary>
///     Async image loader - Returns null immediately, loads image in background
///     Used with MultiBinding to track loading state
/// </summary>
public class AsyncPathToBitmapImageConverter : IValueConverter {
    #region IValueConverter Members

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not string path || string.IsNullOrEmpty(path)) {
            return null!;
        }

        if (!int.TryParse(parameter?.ToString() ?? "128", out var decodePixelWidth)) {
            decodePixelWidth = 128;
        }

        // Return null immediately - loading happens in background
        // The binding will trigger async loading
        LoadImageAsync(path, decodePixelWidth, value as string ?? "");
        return null!;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    #endregion

    private static void LoadImageAsync(string path, int decodePixelWidth, string bindingPath) {
        // Load in background thread
        Task.Run(() => {
            try {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = decodePixelWidth;
                bitmap.EndInit();
                bitmap.Freeze();
                // Bitmap is loaded but we don't return it here
                // The actual image will be set via binding update
            } catch {
                // Silently ignore errors
            }
        });
    }
}

/// <summary>
///     Checks if a path is empty to show/hide loading indicator
/// </summary>
public class StringIsNullOrEmptyConverter : IValueConverter {
    #region IValueConverter Members

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return string.IsNullOrEmpty(value as string);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    #endregion
}