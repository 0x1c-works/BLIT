// Copied from the WinUI3 Gallery example

using System.Runtime.InteropServices;
using Windows.System;

// For DllImport

internal class WindowsSystemDispatcherQueueHelper {
    private object m_dispatcherQueueController;

    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options,
        [In] [Out] [MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

    public void EnsureWindowsSystemDispatcherQueueController() {
        if (DispatcherQueue.GetForCurrentThread() != null) {
            // one already exists, so we'll just use it.
            return;
        }

        if (m_dispatcherQueueController == null) {
            DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
            options.threadType = 2; // DQTYPE_THREAD_CURRENT
            options.apartmentType = 2; // DQTAT_COM_STA

            CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
        }
    }

    #region Nested type: DispatcherQueueOptions

    [StructLayout(LayoutKind.Sequential)]
    private struct DispatcherQueueOptions {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }

    #endregion
}