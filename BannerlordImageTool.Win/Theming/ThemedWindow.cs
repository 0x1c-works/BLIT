using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using WinRT;

namespace BannerlordImageTool.Win.Theming;

/// <summary>
/// The base class of all app windows to apply prettier Windows theme.
/// In order to use this class, a WinUI window class should derive from this class,
/// and set the root element as <c>&lt;ThemedWindow&gt;</c> in XAML.
/// 
/// This class is modified based on the example from WinUI3 Gallery.
/// </summary>
public abstract class ThemedWindow : Window
{
    public enum BackdropType
    {
        Mica,
        MicaAlt,
        DesktopAcrylic,
        DefaultColor,
    }
    WindowsSystemDispatcherQueueHelper m_wsdqHelper;
    BackdropType m_currentBackdrop;
    MicaController m_micaController;
    DesktopAcrylicController m_acrylicController;
    SystemBackdropConfiguration m_configurationSource;

    public ThemedWindow()
    {
        // TODO: read the default theme from LocalSettings to restore user's preferences
        //((FrameworkElement)this.Content).RequestedTheme = AppUIBasics.Helper.ThemeHelper.RootTheme;

        m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
        m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
        Activated += ThemedWindow_Initialized;
    }

    private void ThemedWindow_Initialized(object sender, WindowActivatedEventArgs args)
    {
        SetBackdrop(Backdrop);
        Activated -= ThemedWindow_Initialized;
    }

    public BackdropType Backdrop
    {
        get { return m_currentBackdrop; }
        set
        {
            if (m_currentBackdrop != value)
            {
                if (Content != null)
                {
                    SetBackdrop(value);
                }
                else
                {
                    // wait for ThemeWindow_Initialized to execute
                    // if the components are not initialized yet.
                    m_currentBackdrop = value;
                }
            }
        }
    }
    public ICompositionSupportsSystemBackdrop AsSystemBackdropTarget()
    {
        return this.As<ICompositionSupportsSystemBackdrop>();
    }

    public void SetBackdrop(BackdropType type)
    {
        // Reset to default color. If the requested type is supported, we'll update to that.
        // Note: This sample completely removes any previous controller to reset to the default
        //       state. This is done so this sample can show what is expected to be the most
        //       common pattern of an app simply choosing one controller type which it sets at
        //       startup. If an app wants to toggle between Mica and Acrylic it could simply
        //       call RemoveSystemBackdropTarget() on the old controller and then setup the new
        //       controller, reusing any existing m_configurationSource and Activated/Closed
        //       event handlers.
        m_currentBackdrop = BackdropType.DefaultColor;
        if (m_micaController != null)
        {
            m_micaController.Dispose();
            m_micaController = null;
        }
        if (m_acrylicController != null)
        {
            m_acrylicController.Dispose();
            m_acrylicController = null;
        }
        this.Activated -= Window_Activated;
        this.Closed -= Window_Closed;
        ((FrameworkElement)this.Content).ActualThemeChanged -= Window_ThemeChanged;
        m_configurationSource = null;

        if (type == BackdropType.Mica)
        {
            if (TrySetMicaBackdrop(false))
            {
                m_currentBackdrop = type;
            }
            else
            {
                // Mica isn't supported. Try Acrylic.
                type = BackdropType.DesktopAcrylic;
            }
        }
        if (type == BackdropType.MicaAlt)
        {
            if (TrySetMicaBackdrop(true))
            {
                m_currentBackdrop = type;
            }
            else
            {
                // MicaAlt isn't supported. Try Acrylic.
                type = BackdropType.DesktopAcrylic;
            }
        }
        if (type == BackdropType.DesktopAcrylic)
        {
            if (TrySetAcrylicBackdrop())
            {
                m_currentBackdrop = type;
            }
            else
            {
                // Acrylic isn't supported, so take the next option, which is DefaultColor, which is already set.
            }
        }
    }
    bool TrySetMicaBackdrop(bool useMicaAlt)
    {
        if (MicaController.IsSupported())
        {
            // Hooking up the policy object
            m_configurationSource = new SystemBackdropConfiguration();
            this.Activated += Window_Activated;
            this.Closed += Window_Closed;
            ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

            // Initial configuration state.
            m_configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            m_micaController = new MicaController();

            if (useMicaAlt)
            {
                m_micaController.Kind = MicaKind.BaseAlt;
            }
            else
            {
                m_micaController.Kind = MicaKind.Base;
            }

            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            m_micaController.AddSystemBackdropTarget(AsSystemBackdropTarget());
            m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
            return true; // succeeded
        }

        return false; // Mica is not supported on this system
    }

    bool TrySetAcrylicBackdrop()
    {
        if (DesktopAcrylicController.IsSupported())
        {
            // Hooking up the policy object
            m_configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
            this.Activated += Window_Activated;
            this.Closed += Window_Closed;
            ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

            // Initial configuration state.
            m_configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            m_acrylicController = new Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController();

            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            m_acrylicController.AddSystemBackdropTarget(AsSystemBackdropTarget());
            m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
            return true; // succeeded
        }

        return false; // Acrylic is not supported on this system
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
        // use this closed window.
        if (m_micaController != null)
        {
            m_micaController.Dispose();
            m_micaController = null;
        }
        if (m_acrylicController != null)
        {
            m_acrylicController.Dispose();
            m_acrylicController = null;
        }
        this.Activated -= Window_Activated;
        m_configurationSource = null;
    }

    private void Window_ThemeChanged(FrameworkElement sender, object args)
    {
        if (m_configurationSource != null)
        {
            SetConfigurationSourceTheme();
        }
    }

    private void SetConfigurationSourceTheme()
    {
        switch (((FrameworkElement)this.Content).ActualTheme)
        {
            case ElementTheme.Dark: m_configurationSource.Theme = SystemBackdropTheme.Dark; break;
            case ElementTheme.Light: m_configurationSource.Theme = SystemBackdropTheme.Light; break;
            case ElementTheme.Default: m_configurationSource.Theme = SystemBackdropTheme.Default; break;
        }
    }
}
