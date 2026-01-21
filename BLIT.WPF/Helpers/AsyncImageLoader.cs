using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace BLIT.WPF.Helpers;

/// <summary>
/// Attached behavior for async image loading with caching
/// </summary>
public static class AsyncImageLoader {
    private static readonly object CacheLock = new object();
    
    public static string GetAsyncSource(DependencyObject obj) {
        return (string)obj.GetValue(AsyncSourceProperty);
    }

    public static void SetAsyncSource(DependencyObject obj, string value) {
        obj.SetValue(AsyncSourceProperty, value);
    }

    public static readonly DependencyProperty AsyncSourceProperty =
        DependencyProperty.RegisterAttached(
            "AsyncSource",
            typeof(string),
            typeof(AsyncImageLoader),
            new PropertyMetadata(null, OnAsyncSourceChanged));

    public static int GetDecodePixelWidth(DependencyObject obj) {
        return (int)obj.GetValue(DecodePixelWidthProperty);
    }

    public static void SetDecodePixelWidth(DependencyObject obj, int value) {
        obj.SetValue(DecodePixelWidthProperty, value);
    }

    public static readonly DependencyProperty DecodePixelWidthProperty =
        DependencyProperty.RegisterAttached(
            "DecodePixelWidth",
            typeof(int),
            typeof(AsyncImageLoader),
            new PropertyMetadata(128));

    private static void OnAsyncSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is not Image image) return;

        string source = (string)e.NewValue;
        int decodePixelWidth = GetDecodePixelWidth(image);

        if (string.IsNullOrEmpty(source)) {
            image.Source = null;
            return;
        }

        // Load image asynchronously
        LoadImageAsync(image, source, decodePixelWidth);
    }

    private static void LoadImageAsync(Image image, string path, int decodePixelWidth) {
        Task.Run(() => {
            try {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = decodePixelWidth;
                bitmap.EndInit();
                bitmap.Freeze();

                // Update UI thread
                image.Dispatcher.BeginInvoke(() => {
                    image.Source = bitmap;
                });
            }
            catch {
                // Silently ignore errors - image stays empty
                image.Dispatcher.BeginInvoke(() => {
                    image.Source = null;
                });
            }
        });
    }
}
