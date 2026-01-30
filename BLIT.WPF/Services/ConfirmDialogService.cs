using BLIT.WPF.Helpers;
using Wpf.Ui.Controls;

namespace BLIT.WPF.Services;

public interface IConfirmDialogService {
    Task<Result> Show(string title, string content, Level level);

    enum Level {
        Question,
        Warning,
        Danger,
    }

    enum Result {
        Yes, No, Cancel,
    }
}

public class ConfirmDialogService : IConfirmDialogService {
    #region IConfirmDialogService Members

    public async Task<IConfirmDialogService.Result> Show(string title, string content,
        IConfirmDialogService.Level level) {
        var mb = new MessageBox() {
            Title = title,
            Content = content,
            PrimaryButtonText = I18n.Current.GetString("Yes"),
            PrimaryButtonAppearance =  GetLevelAppearance(level),
            PrimaryButtonIcon = GetLevelIcon(level),
            SecondaryButtonText = I18n.Current.GetString("No"),
            IsCloseButtonEnabled = false,
        };
        return ToResult(await mb.ShowDialogAsync());
    }

    #endregion

    private IConfirmDialogService.Result ToResult(MessageBoxResult boxResult) {
        return boxResult switch {
            MessageBoxResult.Primary => IConfirmDialogService.Result.Yes,
            MessageBoxResult.Secondary => IConfirmDialogService.Result.No,
            _ => IConfirmDialogService.Result.Cancel
        };
    }

    private ControlAppearance GetLevelAppearance(IConfirmDialogService.Level level) {
        return level switch {
            IConfirmDialogService.Level.Warning => ControlAppearance.Caution,
            IConfirmDialogService.Level.Danger => ControlAppearance.Danger,
            _ => ControlAppearance.Primary,
        };
    }
    private IconElement? GetLevelIcon(IConfirmDialogService.Level level) {
        return level switch {
            IConfirmDialogService.Level.Warning => new SymbolIcon(SymbolRegular.Warning20),
            IConfirmDialogService.Level.Danger => new SymbolIcon(SymbolRegular.ErrorCircle20),
            _ => null,
        };
    }
}