using Microsoft.Windows.ApplicationModel.Resources;

namespace BLIT.Win.Helpers;

public class I18n {
    public static I18n Current => App.Current.I18n;

    private readonly ResourceLoader _resLoader;
    private readonly ResourceManager _resManager;

    internal I18n(ResourceLoader resLoader, ResourceManager resManager) {
        _resLoader = resLoader;
        _resManager = resManager;
    }

    public string GetString(string id) {
        return _resLoader.GetString(id);
    }
    /// <summary>
    /// Get string from the resource by its full ID, which should include the resource scope.
    /// For example, the resource "Foo" in the default Resources.resw will be refered to as "Resources/Foo".
    /// </summary>
    /// <param name="fullID"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public string GetStringByFullID(string fullID, ResourceContext context = null) {
        ResourceCandidate candidate = context is null ? _resManager.MainResourceMap.GetValue(fullID) : _resManager.MainResourceMap.GetValue(fullID, context);
        return candidate.ValueAsString;
    }
}
