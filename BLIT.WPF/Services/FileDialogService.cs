using BLIT.WPF.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace BLIT.WPF.Services;

public interface IFileDialogService {
    Task<string?> OpenFolder(Guid stateGuid);
    Task<string?> OpenFile(Guid stateGuid, FileType[] fileTypes);
    Task<string?> OpenFile(Guid stateGuid, string? suggestedPath, FileType[] fileTypes);
    Task<IReadOnlyList<string>> OpenFiles(Guid stateGuid, FileType[] fileTypes);
    string? SaveFile(Guid stateGuid, FileType[] fileTypes, string? suggestedFileName = null, string? overwritingFilePath = null);
}

public class NotFoundException : Exception {
    public string FaultPath { get; }
    public NotFoundException(string faultPath) : base($"path not found: {faultPath}") {
        FaultPath = faultPath;
    }
}

public class FileDialogService : IFileDialogService {
    public async Task<string?> OpenFolder(Guid stateGuid) {
        return await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => {
            return NativeHelpers.RunCom(() => {
                Shell32.IFileOpenDialog fd = CreateFileOpenDialog(stateGuid, Shell32.FILEOPENDIALOGOPTIONS.FOS_PICKFOLDERS);

                if (IsUserCancelled(fd.Show(NativeHelpers.GetHwnd()))) {
                    return null;
                }
                Shell32.IShellItem selectedFolder = fd.GetFolder();
                var path = selectedFolder.GetDisplayName(Shell32.SIGDN.SIGDN_FILESYSPATH);
                if (string.IsNullOrEmpty(path)) {
                    throw new NotFoundException("(failed)");
                }
                if (!Directory.Exists(path)) {
                    throw new NotFoundException(path);
                }

                return path;
            });
        }).Task;
    }

    public async Task<string?> OpenFile(Guid stateGuid, FileType[] fileTypes) {
        return await OpenFile(stateGuid, null, fileTypes);
    }

    public async Task<string?> OpenFile(Guid stateGuid, string? suggestedPath, FileType[] fileTypes) {
        return await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => {
            return NativeHelpers.RunCom(() => {
                Shell32.IFileOpenDialog fd = CreateFileOpenDialog(stateGuid);
                fd.SetFileTypes((uint)fileTypes.Length, fileTypes.Select(ft => ft.ToFilterSpec()).ToArray());

                // Locate the suggested file if exists
                if (ParseFilePath(suggestedPath, out var dir, out var fileName)) {
                    Shell32.IShellItem folderItem = Shell32.SHCreateItemFromParsingName<Shell32.IShellItem>(dir);
                    fd.SetFolder(folderItem);
                    fd.SetFileName(fileName);
                }

                if (IsUserCancelled(fd.Show(NativeHelpers.GetHwnd()))) {
                    return null;
                }
                var path = fd.GetResult().GetDisplayName(Shell32.SIGDN.SIGDN_FILESYSPATH);
                if (string.IsNullOrEmpty(path)) {
                    throw new InvalidOperationException("failed to get the file path");
                }
                return path;
            });
        }).Task;
    }

    public async Task<IReadOnlyList<string>> OpenFiles(Guid stateGuid, FileType[] fileTypes) {
        return await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => {
            return NativeHelpers.RunCom<IReadOnlyList<string>>(() => {
                Shell32.IFileOpenDialog fd = CreateFileOpenDialog(stateGuid, Shell32.FILEOPENDIALOGOPTIONS.FOS_ALLOWMULTISELECT);
                fd.SetFileTypes((uint)fileTypes.Length, fileTypes.Select(ft => ft.ToFilterSpec()).ToArray());

                if (IsUserCancelled(fd.Show(NativeHelpers.GetHwnd()))) {
                    return Array.Empty<string>();
                }
                Shell32.IShellItemArray results = fd.GetResults();
                var count = results.GetCount();
                var files = new string[count];

                for (uint i = 0; i < count; i++) {
                    var path = results.GetItemAt(i).GetDisplayName(Shell32.SIGDN.SIGDN_FILESYSPATH);
                    if (string.IsNullOrEmpty(path)) {
                        throw new InvalidOperationException($"failed to get the file path {path}");
                    }
                    files[i] = path;
                }
                return files;
            });
        }).Task;
    }

    private Shell32.IFileOpenDialog CreateFileOpenDialog(Guid stateGuid, Shell32.FILEOPENDIALOGOPTIONS opts = 0) {
        HRESULT hr = Ole32.CoCreateInstance(typeof(Shell32.CFileOpenDialog).GUID,
                                        null,
                                        Ole32.CLSCTX.CLSCTX_INPROC_SERVER,
                                        typeof(Shell32.IFileOpenDialog).GUID,
                                        out var ppv);
        if (hr != HRESULT.S_OK) {
            throw new HRESULTException(hr, "error in CoCreateInstance for a FileOpenDialog");
        }
        var fd = (Shell32.IFileOpenDialog)ppv;

        fd.SetClientGuid(stateGuid);
        fd.SetDefaultFolder(Shell32.KNOWNFOLDERID.FOLDERID_DocumentsLibrary.GetIShellItem());
        fd.SetOptions(fd.GetOptions() | Shell32.FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM | opts);
        return fd;
    }

    private bool IsUserCancelled(HRESULT hr) {
        return hr == HRESULT.HRESULT_FROM_WIN32(Win32Error.ERROR_CANCELLED);
    }

    public string? SaveFile(Guid stateGuid,
                            FileType[] fileTypes,
                            string? suggestedFileName = "",
                            string? overwritingFilePath = null) {
        return NativeHelpers.RunCom(() => {
            HRESULT hr = Ole32.CoCreateInstance(typeof(Shell32.CFileSaveDialog).GUID,
                                            null,
                                            Ole32.CLSCTX.CLSCTX_INPROC_SERVER,
                                            typeof(Shell32.IFileSaveDialog).GUID,
                                            out var ppv);
            if (hr != HRESULT.S_OK) {
                throw new HRESULTException(hr, "error in CoCreateInstance for a FileSaveDialog");
            }
            var fd = (Shell32.IFileSaveDialog)ppv;

            fd.SetClientGuid(stateGuid);
            fd.SetDefaultFolder(Shell32.KNOWNFOLDERID.FOLDERID_DocumentsLibrary.GetIShellItem());
            fd.SetOptions(fd.GetOptions() | Shell32.FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM);
            if (fileTypes.Length > 0) {
                fd.SetDefaultExtension(fileTypes[0].Extension);
            }
            fd.SetFileTypes((uint)fileTypes.Length, fileTypes.Select(ft => ft.ToFilterSpec()).ToArray());

            if (overwritingFilePath != null && ParseFilePath(overwritingFilePath, out var dir, out var fileName)) {
                Shell32.IShellItem folder = Shell32.SHCreateItemFromParsingName<Shell32.IShellItem>(dir);
                fd.SetFolder(folder);
                fd.SetFileName(fileName);
            } else if (!string.IsNullOrWhiteSpace(suggestedFileName)) {
                var ext = Path.GetExtension(suggestedFileName).TrimStart('.');
                fd.SetDefaultExtension(ext);
                fd.SetFileName(Path.GetFileNameWithoutExtension(suggestedFileName));
            }

            return IsUserCancelled(fd.Show(NativeHelpers.GetHwnd())) 
                ? null 
                : fd.GetResult().GetDisplayName(Shell32.SIGDN.SIGDN_FILESYSPATH);
        });
    }

    private bool ParseFilePath(string? filePath, out string? dir, out string? fileName) {
        dir = null;
        fileName = null;
        if (string.IsNullOrWhiteSpace(filePath)) {
            return false;
        }
        var fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(filePath)) {
            return false;
        }
        dir = Path.GetDirectoryName(fullPath);
        fileName = Path.GetFileName(filePath);
        return true;
    }
}
