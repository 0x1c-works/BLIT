using Godot;
using System.Threading.Tasks;

namespace BLIT.scripts.Services;
public class ConfirmDialogService {
    public Task<bool> Ask(Control parent, string title, string text, string? okText = null, string? cancelText = null) {
        var t = new TaskCompletionSource<bool>();
        var confirm = new ConfirmationDialog() {
            Title = title,
            DialogText = text,
            OkButtonText = okText ?? "YES",
            CancelButtonText = cancelText ?? "NO",
            DialogAutowrap = true,
            MinSize = new Vector2I(320, 100),
        };
        Button ok = confirm.GetOkButton();
        Button cancel = confirm.GetCancelButton();

        ok.CustomMinimumSize = new Vector2I(60, 0);
        ok.Pressed += () => {
            t.SetResult(true);
        };
        cancel.CustomMinimumSize = new Vector2I(60, 0);
        confirm.Canceled += () => {
            t.SetResult(false);
        };
        parent.AddChild(confirm);
        confirm.PopupCentered();
        return t.Task;
    }
}
