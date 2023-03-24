using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace BannerlordImageTool.Win.Common;

public class FileHelper
{
    public static async Task<StorageFolder> OpenFolder(string accessToken, string settingsId = null)
    {
        var picker = new FolderPicker() {
            SettingsIdentifier = settingsId,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };
        BindHwnd(picker);
        var folder = await picker.PickSingleFolderAsync();
        if (folder is not null)
        {
            StorageApplicationPermissions.FutureAccessList.AddOrReplace(accessToken, folder);
        }
        return folder;
    }
    public static async Task<StorageFile> OpenSingleFile(params string[] exts)
    {
        var picker = PrepareFileOpenPicker(exts);
        return await picker.PickSingleFileAsync();
    }
    public static async Task<IReadOnlyList<StorageFile>> OpenMultipleFiles(params string[] exts)
    {
        var picker = PrepareFileOpenPicker(exts);
        return await picker.PickMultipleFilesAsync();
    }
    public static async Task<StorageFile> SaveFile(IDictionary<string, IList<string>> fileTypes,
        string suggestedFileName = "",
        StorageFile suggestedFile = null)
    {
        var picker = PrepareFileSavePicker(fileTypes);
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.SuggestedFileName = suggestedFileName;
        picker.SuggestedSaveFile = suggestedFile;
        return await picker.PickSaveFileAsync();
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
    private static FileSavePicker PrepareFileSavePicker(IDictionary<string, IList<string>> fileTypes)
    {
        var picker = new FileSavePicker();
        BindHwnd(picker);
        if (fileTypes == null || fileTypes.Count == 0)
        {
            throw new ArgumentNullException(nameof(fileTypes));
        }
        foreach (var fileType in fileTypes)
        {
            var typeName = fileType.Key;
            var exts = fileType.Value.ToArray();
            for (var i = 0; i < exts.Length; i++)
            {
                var ext = exts[i].Replace("*", "").ToLower();
                if (!ext.StartsWith("."))
                {
                    ext = "." + ext;
                }
                exts[i] = ext;
            }
            picker.FileTypeChoices.Add(typeName, exts);
        }
        return picker;

    }
    private static void BindHwnd(object target)
    {
        var hwnd = WindowNative.GetWindowHandle(App.Current.MainWindow);
        InitializeWithWindow.Initialize(target, hwnd);
    }
}
