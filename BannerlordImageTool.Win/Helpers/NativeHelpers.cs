using Microsoft.UI.Xaml;
using Serilog;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Vanara.PInvoke;
using WinRT.Interop;

namespace BannerlordImageTool.Win.Helpers;

public static class NativeHelpers
{
    public static void BindWindowToTarget(object target, Window wnd = null)
    {
        InitializeWithWindow.Initialize(target, GetHwnd(wnd));
    }
    public static IntPtr GetHwnd(Window wnd = null)
    {
        return WindowNative.GetWindowHandle(wnd ?? App.Current.MainWindow);
    }
    public static bool IsCurrentThreadSTA()
    {
        return Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;
    }
    public static TReturn RunCom<TReturn>(Func<TReturn> func)
    {
        if (!IsCurrentThreadSTA())
        {
            throw new InvalidOperationException("The current thread must be STA.");
        }
        try
        {
            var hr = (int)Ole32.CoInitialize();
            return hr < HRESULT.S_OK ? throw new HRESULTException(hr, "Failed to initialize the COM components") : func();
        }
        catch (COMException ex)
        {
            Log.Error("COMException during opening folder: {Exception}", ex);
            throw;
        }
        finally
        {
            Ole32.CoUninitialize();
        }
    }
}
public class HRESULTException : Exception
{
    public HRESULT HRESULT { get; }
    public HRESULTException(HRESULT hr, string message) : base(message)
    {
        HRESULT = hr;
    }
}
