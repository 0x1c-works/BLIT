using BLIT.Win.Controls;

namespace BLIT.Win.Services;

public interface ILoadingService
{
    void RegisterControl(LoadingOverlay overlay);
    void Show(string message);
    void Hide();
}

public class LoadingService : ILoadingService
{
    LoadingOverlay _overlay;
    public void Hide()
    {
        if (_overlay != null)
        {
            _overlay.IsLoading = false;
        }
    }

    public void RegisterControl(LoadingOverlay overlay)
    {
        if (overlay == _overlay) return;
        Hide();
        _overlay = overlay;
    }

    public void Show(string message)
    {
        if (_overlay != null)
        {
            _overlay.Message = message;
            _overlay.IsLoading = true;
        }
    }
}
