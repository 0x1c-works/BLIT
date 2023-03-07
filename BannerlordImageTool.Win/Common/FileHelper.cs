using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace BannerlordImageTool.Win.Common;

public class FileHelper
{
    public async Task<StorageFolder> PickFolder(object wnd)
    {
        var picker = new FolderPicker();
        BindHwnd(wnd, picker);
        return await picker.PickSingleFolderAsync();

    }

    private void BindHwnd(object wnd, object target)
    {
        var hwnd = WindowNative.GetWindowHandle(wnd);
        InitializeWithWindow.Initialize(target, hwnd);
    }
}
