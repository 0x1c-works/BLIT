using BannerlordImageTool.Win.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vanara.PInvoke;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace BannerlordImageTool.Win.Services;

public interface IFileDialogService
{
    Task<StorageFolder> OpenFolder(Guid stateGuid);
    Task<StorageFile> OpenFile(Guid stateGuid, params string[] exts);
    Task<StorageFile> OpenFile(Guid stateGuid, string suggestedPath, params string[] exts);
    Task<IReadOnlyList<StorageFile>> OpenFiles(Guid stateGuid, params string[] exts);
    Task<StorageFile> SaveFile(Guid stateGuid, IDictionary<string, IList<string>> fileTypes,
        string suggestedFileName = "",
        StorageFile suggestedFile = null);
}
public class FileDialogService : IFileDialogService
{
    public async Task<StorageFolder> OpenFolder(Guid stateGuid)
    {
        return await NativeHelpers.RunCom(async () => {
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
                ofd.SetClientGuid(stateGuid);
                ofd.SetDefaultFolder(Shell32.KNOWNFOLDERID.FOLDERID_DocumentsLibrary.GetIShellItem());

                hr = ofd.Show(NativeHelpers.GetHwnd());
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
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(stateGuid.ToString(), folder);
                }
                return folder;
            }
        });
    }

    public async Task<StorageFile> OpenFile(Guid stateGuid, params string[] exts)
    {
        var picker = PrepareFileOpenPicker(stateGuid.ToString(), exts);
        return await picker.PickSingleFileAsync();
    }
    public async Task<StorageFile> OpenFile(Guid stateGuid, string suggestedPath, params string[] exts)
    {
        var picker = PrepareFileOpenPicker(stateGuid.ToString(), exts);
        // TODO: support having the suggested path selected (will need native call)
        return await picker.PickSingleFileAsync();
    }
    public async Task<IReadOnlyList<StorageFile>> OpenFiles(Guid stateGuid, params string[] exts)
    {
        var picker = PrepareFileOpenPicker(stateGuid.ToString(), exts);
        return await picker.PickMultipleFilesAsync();
    }
    private FileOpenPicker PrepareFileOpenPicker(string stateId, string[] exts)
    {
        var picker = new FileOpenPicker() {
            SettingsIdentifier = stateId,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };
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
        NativeHelpers.BindWindowToTarget(picker);
        return picker;
    }

    public async Task<StorageFile> SaveFile(Guid stateGuid,
                                            IDictionary<string, IList<string>> fileTypes,
                                            string suggestedFileName = "",
                                            StorageFile suggestedFile = null)
    {
        var picker = PrepareFileSavePicker(stateGuid.ToString(), fileTypes);
        picker.SuggestedFileName = suggestedFileName;
        picker.SuggestedSaveFile = suggestedFile;
        return await picker.PickSaveFileAsync();
    }
    private FileSavePicker PrepareFileSavePicker(string stateId, IDictionary<string, IList<string>> fileTypes)
    {
        if (fileTypes == null || fileTypes.Count == 0)
        {
            throw new ArgumentNullException(nameof(fileTypes));
        }
        var picker = new FileSavePicker() {
            SettingsIdentifier = stateId,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };
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
        NativeHelpers.BindWindowToTarget(picker);
        return picker;

    }
}
