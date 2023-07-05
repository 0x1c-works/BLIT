using BannerlordImageTool.Win.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vanara.PInvoke;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace BannerlordImageTool.Win.Services;

public interface IFileDialogService
{
    Task<StorageFolder> OpenFolder(Guid stateGuid);
    Task<StorageFile> OpenFile(Guid stateGuid, FileType[] fileTypes);
    Task<StorageFile> OpenFile(Guid stateGuid, string suggestedPath, FileType[] fileTypes);
    Task<IReadOnlyList<StorageFile>> OpenFiles(Guid stateGuid, FileType[] fileTypes);
    string SaveFile(Guid stateGuid, FileType[] fileTypes, string suggestedFileName = null, string overwritingFilePath = null);
}

public class FileDialogService : IFileDialogService
{
    public async Task<StorageFolder> OpenFolder(Guid stateGuid)
    {
        return await NativeHelpers.RunCom(async () => {
            Shell32.IFileOpenDialog ofd = CreateFileOpenDialog(stateGuid, Shell32.FILEOPENDIALOGOPTIONS.FOS_PICKFOLDERS);

            if (IsUserCancelled(ofd.Show(NativeHelpers.GetHwnd())))
            {
                // User closes the dialog by clicking "Cancel".
                return null;
            }
            Shell32.IShellItem selectedFolder = ofd.GetFolder();
            var path = selectedFolder.GetDisplayName(Shell32.SIGDN.SIGDN_FILESYSPATH);
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("failed to get the selected folder's path");
            }
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
            if (folder is not null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(stateGuid.ToString(), folder);
            }
            return folder;
        });
    }

    public async Task<StorageFile> OpenFile(Guid stateGuid, FileType[] fileTypes)
    {
        return await OpenFile(stateGuid, null, fileTypes);
    }
    public async Task<StorageFile> OpenFile(Guid stateGuid, string suggestedPath, FileType[] fileTypes)
    {
        return await NativeHelpers.RunCom(async () => {
            Shell32.IFileOpenDialog fd = CreateFileOpenDialog(stateGuid);
            fd.SetFileTypes((uint)fileTypes.Length, fileTypes.Select(ft => ft.ToFilterSpec()).ToArray());

            // Locate the suggested file if exists
            if (ParseFilePath(suggestedPath, out var dir, out var fileName))
            {
                Shell32.IShellItem folderItem = Shell32.SHCreateItemFromParsingName<Shell32.IShellItem>(dir);
                fd.SetFolder(folderItem);
                fd.SetFileName(fileName);
            }

            if (IsUserCancelled(fd.Show(NativeHelpers.GetHwnd())))
            {
                return null;
            }
            var path = fd.GetResult().GetDisplayName(Shell32.SIGDN.SIGDN_FILESYSPATH);
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("failed to get the file path");
            }
            StorageFile file = await StorageFile.GetFileFromPathAsync(path);
            return file;
        });
    }
    public async Task<IReadOnlyList<StorageFile>> OpenFiles(Guid stateGuid, FileType[] fileTypes)
    {
        return await NativeHelpers.RunCom(async () => {
            Shell32.IFileOpenDialog fd = CreateFileOpenDialog(stateGuid, Shell32.FILEOPENDIALOGOPTIONS.FOS_ALLOWMULTISELECT);
            fd.SetFileTypes((uint)fileTypes.Length, fileTypes.Select(ft => ft.ToFilterSpec()).ToArray());

            if (IsUserCancelled(fd.Show(NativeHelpers.GetHwnd())))
            {
                return null;
            }
            Shell32.IShellItemArray results = fd.GetResults();
            var count = results.GetCount();
            var files = new StorageFile[count];

            for (uint i = 0; i < count; i++)
            {
                var path = results.GetItemAt(i).GetDisplayName(Shell32.SIGDN.SIGDN_FILESYSPATH);
                if (string.IsNullOrEmpty(path))
                {
                    throw new InvalidOperationException($"failed to get the file path {path}");
                }
                files[i] = await StorageFile.GetFileFromPathAsync(path);
            }
            return files;
        });
    }
    Shell32.IFileOpenDialog CreateFileOpenDialog(Guid stateGuid, Shell32.FILEOPENDIALOGOPTIONS opts = 0)
    {
        HRESULT hr = Ole32.CoCreateInstance(typeof(Shell32.CFileOpenDialog).GUID,
                                        null,
                                        Ole32.CLSCTX.CLSCTX_INPROC_SERVER,
                                        typeof(Shell32.IFileOpenDialog).GUID,
                                        out var ppv);
        if (hr != HRESULT.S_OK)
        {
            throw new HRESULTException(hr, "error in CoCreateInstance for a FileOpenDialog");
        }
        var fd = (Shell32.IFileOpenDialog)ppv;

        fd.SetClientGuid(stateGuid);
        fd.SetDefaultFolder(Shell32.KNOWNFOLDERID.FOLDERID_DocumentsLibrary.GetIShellItem());
        fd.SetOptions(fd.GetOptions() | Shell32.FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM | opts);
        return fd;
    }
    bool IsUserCancelled(HRESULT hr)
    {
        return hr == HRESULT.HRESULT_FROM_WIN32(Win32Error.ERROR_CANCELLED);
    }
    public string SaveFile(Guid stateGuid,
                                            FileType[] fileTypes,
                                            string suggestedFileName = "",
                                            string overwritingFilePath = null)
    {

        return NativeHelpers.RunCom(() => {
            HRESULT hr = Ole32.CoCreateInstance(typeof(Shell32.CFileSaveDialog).GUID,
                                            null,
                                            Ole32.CLSCTX.CLSCTX_INPROC_SERVER,
                                            typeof(Shell32.IFileSaveDialog).GUID,
                                            out var ppv);
            if (hr != HRESULT.S_OK)
            {
                throw new HRESULTException(hr, "error in CoCreateInstance for a FileSaveDialog");
            }
            var fd = (Shell32.IFileSaveDialog)ppv;

            fd.SetClientGuid(stateGuid);
            fd.SetDefaultFolder(Shell32.KNOWNFOLDERID.FOLDERID_DocumentsLibrary.GetIShellItem());
            fd.SetOptions(fd.GetOptions() | Shell32.FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM);
            if (fileTypes.Length > 0)
            {
                fd.SetDefaultExtension(fileTypes[0].Extension);
            }
            fd.SetFileTypes((uint)fileTypes.Length, fileTypes.Select(ft => ft.ToFilterSpec()).ToArray());

            if (overwritingFilePath != null && ParseFilePath(overwritingFilePath, out var dir, out var fileName))
            {
                Shell32.IShellItem folder = Shell32.SHCreateItemFromParsingName<Shell32.IShellItem>(dir);
                fd.SetFolder(folder);
                fd.SetFileName(fileName);
            }
            else if (!string.IsNullOrWhiteSpace(suggestedFileName))
            {
                var ext = Path.GetExtension(suggestedFileName).TrimStart('.');
                fd.SetDefaultExtension(ext);
                fd.SetFileName(Path.GetFileNameWithoutExtension(suggestedFileName));
            }

            return IsUserCancelled(fd.Show(NativeHelpers.GetHwnd())) ? null : fd.GetResult().GetDisplayName(Shell32.SIGDN.SIGDN_FILESYSPATH);
        });
    }
    bool ParseFilePath(string filePath, out string dir, out string fileName)
    {
        dir = null;
        fileName = null;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }
        var fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(filePath))
        {
            return false;
        }
        dir = Path.GetDirectoryName(fullPath);
        fileName = Path.GetFileName(filePath);
        return true;
    }
}
