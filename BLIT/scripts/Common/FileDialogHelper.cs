using Godot;
using System.Linq;

namespace BLIT.scripts.Common;
public static class FileDialogHelper {
    public record struct Extension(string Name, string[] Extensions) {
        public override string ToString() {
            var extString = string.Join(", ", Extensions.Select(ext => $"*{ext}"));
            return $"{extString} ; {Name}";
        }
    }

    public static Extension SUPPORTED_IMAGES {
        get {
            var exts = new string[] { ".png", ".webp" };
            return new Extension("SUPPORTED_IMAGES", exts);
        }
    }

    public static FileDialog CreateNative(Extension[] extensions, string? currentPath = null) {
        var dlg = new FileDialog() {
            Access = FileDialog.AccessEnum.Filesystem,
            UseNativeDialog = true,
            InitialPosition = Window.WindowInitialPosition.CenterMainWindowScreen,
            CurrentPath = currentPath ?? string.Empty,
            Filters = extensions.Select(ext => ext.ToString()).ToArray(),
            CancelButtonText = "CANCEL",
        };
        return dlg;
    }
}
