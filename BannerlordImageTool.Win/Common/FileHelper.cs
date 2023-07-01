using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Vanara.PInvoke;
using Serilog;
using System.Threading;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
//using PInvoke;

namespace BannerlordImageTool.Win.Common;

public class FileHelper
{
    /// <summary>
    /// Because <c>Windows.Storage.FolderPicker</c> has a bug that on re-opening, its "Select"
    /// button becomes grayed out, so we have to use <c>Shell32.FileOpenDialog</c> instead.
    /// </summary>
    /// <remarks>
    /// More explanation on this bug can be found at:
    /// https://github.com/CommunityToolkit/Maui/issues/1085#issuecomment-1464286082
    /// </remarks>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<StorageFolder> OpenFolder(string accessToken)
    {
        if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA || (int)Ole32.CoInitialize() < HRESULT.S_OK)
        {
            throw new InvalidOperationException("The current thread must be STA.");
        }
        try
        {
            var hr = Ole32.CoCreateInstance(typeof(Shell32.CFileOpenDialog).GUID, null, Ole32.CLSCTX.CLSCTX_INPROC_SERVER, typeof(Shell32.IFileOpenDialog).GUID, out var com);
            if (hr != HRESULT.S_OK)
            {
                throw new HRESULTException(hr, "error in CoCreateInstance a FileOpenDialog");
            }
            else
            {
                var ofd = (Shell32.IFileOpenDialog)com;

                var flags = ofd.GetOptions();
                flags |= Shell32.FILEOPENDIALOGOPTIONS.FOS_PICKFOLDERS;
                ofd.SetOptions(flags);
                ofd.SetDefaultFolder(Shell32.KNOWNFOLDERID.FOLDERID_DocumentsLibrary.GetIShellItem());

                hr = ofd.Show(GetHwnd());
                if (hr == HRESULT.HRESULT_FROM_WIN32(Win32Error.ERROR_CANCELLED))
                {
                    // User closes the dialog by clicking "Cancel".
                    return null;
                }
                var selectedFolder = ofd.GetFolder();
                var path = selectedFolder.GetDisplayName(Shell32.SIGDN.SIGDN_FILESYSPATH);
                Log.Information("selected folder: {Folder}", path);
                if (string.IsNullOrEmpty(path))
                {
                    throw new InvalidOperationException("failed to get the selected folder's path");
                }
                var folder = await StorageFolder.GetFolderFromPathAsync(path);
                if (folder is not null)
                {
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(accessToken, folder);
                }
                return folder;
            }
        }
        catch (COMException ex)
        {
            Log.Error("COMException during opening folder: {Exception}", ex);
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("failed to open folder: {Exception}", ex);
            throw;
        }
        finally
        {
            Ole32.CoUninitialize();
        }
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
        BindHwndToTarget(picker);
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
        BindHwndToTarget(picker);
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
    private static void BindHwndToTarget(object target, Window wnd = null)
    {
        InitializeWithWindow.Initialize(target, GetHwnd(wnd));
    }
    private static IntPtr GetHwnd(Window wnd = null)
    {
        return WindowNative.GetWindowHandle(wnd ?? App.Current.MainWindow);
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
