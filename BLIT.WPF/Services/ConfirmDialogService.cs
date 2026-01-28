using System.Windows;

namespace BLIT.WPF.Services;

public enum ContentDialogResult {
    None,
    Primary,
    Secondary
}

public interface IConfirmDialogService {
    Task<ContentDialogResult> Show(string title, string content, string primaryButton, string secondaryButton);
    Task<ContentDialogResult> ShowWarn(string title, string content);
    Task<ContentDialogResult> ShowDanger(string title, string content);
}

public class ConfirmDialogService : IConfirmDialogService {
    #region IConfirmDialogService Members

    public Task<ContentDialogResult> Show(string title, string content, string primaryButton, string secondaryButton) {
        MessageBoxResult result = MessageBox.Show(
            content,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No
        );

        return Task.FromResult(result == MessageBoxResult.Yes
            ? ContentDialogResult.Primary
            : ContentDialogResult.Secondary);
    }

    public Task<ContentDialogResult> ShowWarn(string title, string content) {
        MessageBoxResult result = MessageBox.Show(
            content,
            title,
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning,
            MessageBoxResult.Cancel
        );

        return Task.FromResult(result == MessageBoxResult.OK
            ? ContentDialogResult.Primary
            : ContentDialogResult.Secondary);
    }

    public Task<ContentDialogResult> ShowDanger(string title, string content) {
        MessageBoxResult result = MessageBox.Show(
            content,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Exclamation,
            MessageBoxResult.No
        );

        return Task.FromResult(result == MessageBoxResult.Yes
            ? ContentDialogResult.Primary
            : ContentDialogResult.Secondary);
    }

    #endregion
}