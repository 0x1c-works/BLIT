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
    public static async Task<StorageFolder> PickFolder(string accessToken, string settingsId = null)
    {
        var picker = new FolderPicker();
        BindHwnd(picker);
        picker.SettingsIdentifier = settingsId;
        var folder = await picker.PickSingleFolderAsync();
        if (folder is not null)
        {
            StorageApplicationPermissions.FutureAccessList.AddOrReplace(accessToken, folder);
        }
        return folder;
    }
    public static async Task<StorageFile> PickSingleFile(params string[] exts)
    {
        var picker = PrepareFileOpenPicker(exts);
        return await picker.PickSingleFileAsync();
    }
    public static async Task<IReadOnlyList<StorageFile>> PickMultipleFiles(params string[] exts)
    {
        var picker = PrepareFileOpenPicker(exts);
        return await picker.PickMultipleFilesAsync();
    }
    private static FileOpenPicker PrepareFileOpenPicker(string[] exts)
    {

        var picker = new FileOpenPicker();
        BindHwnd(picker);
        picker.ViewMode = PickerViewMode.Thumbnail;
        if (exts == null || exts.Length == 0)
        {
            picker.FileTypeFilter.Add("*");
        }
        else
        {
            foreach (var ext in exts)
            {
                var validExt = ext.Replace("*", "").ToLower();
                if (!validExt.StartsWith("."))
                {
                    validExt = "." + validExt;
                }
                picker.FileTypeFilter.Add(validExt);
            }
        }
        return picker;
    }
    private static void BindHwnd(object target)
    {
        var hwnd = WindowNative.GetWindowHandle(App.Current.MainWindow);
        InitializeWithWindow.Initialize(target, hwnd);
    }
}
