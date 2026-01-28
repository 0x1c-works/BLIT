using BLIT.WPF.Controls;

namespace BLIT.WPF.Services;

public interface ILoadingService {
    void RegisterControl(LoadingOverlay overlay);
    void Show(string message);
    void ShowProgress(string message, int total);
    void UpdateProgress(int current);
    void Hide();
}

public class LoadingService : ILoadingService {
    private LoadingOverlay? _overlay;
    
    public void Hide() {
        if (_overlay != null) {
            _overlay.IsLoading = false;
            _overlay.IsShowingProgress = false;
        }
    }

    public void RegisterControl(LoadingOverlay overlay) {
        if (overlay == _overlay) return;
        Hide();
        _overlay = overlay;
    }

    public void Show(string message) {
        if (_overlay != null) {
            _overlay.Message = message;
            _overlay.IsShowingProgress = false;
            _overlay.IsLoading = true;
        }
    }

    public void ShowProgress(string message, int total) {
        if (_overlay != null) {
            _overlay.Message = message;
            _overlay.TotalProgress = total;
            _overlay.CurrentProgress = 0;
            _overlay.IsShowingProgress = true;
            _overlay.IsLoading = true;
        }
    }

    public void UpdateProgress(int current) {
        if (_overlay != null) {
            _overlay.CurrentProgress = current;
        }
    }
}
