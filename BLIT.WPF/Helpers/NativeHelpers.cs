using Serilog;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Vanara.PInvoke;

namespace BLIT.WPF.Helpers;

public static class NativeHelpers {
    public static IntPtr GetHwnd(Window? wnd = null) {
        Window? window = wnd ?? Application.Current.MainWindow;
        if (window == null) {
            return IntPtr.Zero;
        }

        return new WindowInteropHelper(window).Handle;
    }

    public static bool IsCurrentThreadSTA() {
        return Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;
    }

    public static TReturn RunCom<TReturn>(Func<TReturn> func) {
        if (!IsCurrentThreadSTA()) {
            throw new InvalidOperationException("The current thread must be STA.");
        }

        try {
            var hr = (int)Ole32.CoInitialize();
            return hr < HRESULT.S_OK
                ? throw new HRESULTException(hr, "Failed to initialize the COM components")
                : func();
        } catch (COMException ex) {
            Log.Error(ex, "COMException during COM operation: {Exception}");
            throw;
        } finally {
            Ole32.CoUninitialize();
        }
    }
}

public class HRESULTException : Exception {
    public HRESULTException(HRESULT hr, string message) : base(message) {
        HRESULT = hr;
    }

    public HRESULT HRESULT { get; }
}