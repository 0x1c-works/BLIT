using BLIT.WPF.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace BLIT.WPF.Controls;

public partial class LoadingOverlay : UserControl {
    // Cached Storyboards to prevent IDE cleanup and improve performance
    private Storyboard? _fadeInStoryboard;
    private Storyboard? _fadeOutStoryboard;

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message),
        typeof(string),
        typeof(LoadingOverlay),
        new PropertyMetadata("Loading..."));

    public static readonly DependencyProperty IsShowingProgressProperty = DependencyProperty.Register(
        nameof(IsShowingProgress),
        typeof(bool),
        typeof(LoadingOverlay),
        new PropertyMetadata(false));

    public static readonly DependencyProperty CurrentProgressProperty = DependencyProperty.Register(
        nameof(CurrentProgress),
        typeof(int),
        typeof(LoadingOverlay),
        new PropertyMetadata(0, OnProgressChanged));

    public static readonly DependencyProperty TotalProgressProperty = DependencyProperty.Register(
        nameof(TotalProgress),
        typeof(int),
        typeof(LoadingOverlay),
        new PropertyMetadata(0, OnProgressChanged));

    public static readonly DependencyProperty ProgressDisplayTextProperty = DependencyProperty.Register(
        nameof(ProgressDisplayText),
        typeof(string),
        typeof(LoadingOverlay),
        new PropertyMetadata("0/0"));

    public static readonly DependencyProperty ProgressPercentageProperty = DependencyProperty.Register(
        nameof(ProgressPercentage),
        typeof(double),
        typeof(LoadingOverlay),
        new PropertyMetadata(0.0));

    public static readonly DependencyProperty ProgressPercentageStringProperty = DependencyProperty.Register(
        nameof(ProgressPercentageString),
        typeof(string),
        typeof(LoadingOverlay),
        new PropertyMetadata("0%"));

    public LoadingOverlay() {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
        // Cache Storyboards from resources - this makes IDE recognize they're being used
        _fadeInStoryboard = (Storyboard?)Resources["FadeInStoryboard"];
        _fadeOutStoryboard = (Storyboard?)Resources["FadeOutStoryboard"];
        
        // Register with loading service
        AppServices.Get<ILoadingService>()?.RegisterControl(this);
    }

    public string Message {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public bool IsShowingProgress {
        get => (bool)GetValue(IsShowingProgressProperty);
        set => SetValue(IsShowingProgressProperty, value);
    }

    public int CurrentProgress {
        get => (int)GetValue(CurrentProgressProperty);
        set => SetValue(CurrentProgressProperty, value);
    }

    public int TotalProgress {
        get => (int)GetValue(TotalProgressProperty);
        set => SetValue(TotalProgressProperty, value);
    }

    public string ProgressDisplayText {
        get => (string)GetValue(ProgressDisplayTextProperty);
        set => SetValue(ProgressDisplayTextProperty, value);
    }

    public double ProgressPercentage {
        get => (double)GetValue(ProgressPercentageProperty);
        set => SetValue(ProgressPercentageProperty, value);
    }

    public string ProgressPercentageString {
        get => (string)GetValue(ProgressPercentageStringProperty);
        set => SetValue(ProgressPercentageStringProperty, value);
    }

    public bool IsLoading {
        get => RootGrid.Visibility == Visibility.Visible;
        set {
            if (value) {
                // Show with fade-in animation
                RootGrid.Visibility = Visibility.Visible;
                _fadeInStoryboard?.Begin();
            } else {
                // Hide with fade-out animation
                if (_fadeOutStoryboard != null) {
                    _fadeOutStoryboard.Completed += (s, e) => {
                        RootGrid.Visibility = Visibility.Collapsed;
                    };
                    _fadeOutStoryboard.Begin();
                } else {
                    RootGrid.Visibility = Visibility.Collapsed;
                }
            }
        }
    }

    private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is LoadingOverlay overlay) {
            overlay.UpdateProgressDisplay();
        }
    }

    private void UpdateProgressDisplay() {
        ProgressDisplayText = $"{CurrentProgress}/{TotalProgress}";
        var percentage = TotalProgress > 0 ? CurrentProgress * 100.0 / TotalProgress : 0;
        ProgressPercentage = percentage;
        ProgressPercentageString = $"{percentage:F0}%";
    }
}