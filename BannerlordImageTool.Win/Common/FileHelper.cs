using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace BannerlordImageTool.Win.Common;

public class FileHelper
{
    public static async Task<StorageFolder> PickFolder()
    {
        var picker = new FolderPicker();
        BindHwnd(picker);
        var folder = await picker.PickSingleFolderAsync();
        if (folder is not null)
        {
            StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
        }
        return folder;
    }
    public static async Task<IReadOnlyList<StorageFile>> PickMultipleFiles()
    {
        var picker = new FileOpenPicker();
        BindHwnd(picker);
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.FileTypeFilter.Add(".png");
        return await picker.PickMultipleFilesAsync();
    }

    private static void BindHwnd(object target)
    {
        var hwnd = WindowNative.GetWindowHandle(App.Current.MainWindow);
        InitializeWithWindow.Initialize(target, hwnd);
    }
}
