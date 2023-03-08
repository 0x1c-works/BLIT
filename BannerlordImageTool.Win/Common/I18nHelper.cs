using Microsoft.Windows.ApplicationModel.Resources;

namespace BannerlordImageTool.Win.Common;

public class I18n
{
    public static I18n Current { get => App.Current.I18n; }


    private ResourceLoader _resLoader;
    private ResourceManager _resManager;

    internal I18n(ResourceLoader resLoader, ResourceManager resManager)
    {
        _resLoader = resLoader;
        _resManager = resManager;
    }

    public string GetString(string id)
    {
        return _resLoader.GetString(id);
    }
    /// <summary>
    /// Get string from the resource by its full ID, which should include the resource scope.
    /// For example, the resource "Foo" in the default Resources.resw will be refered to as "Resources/Foo".
    /// </summary>
    /// <param name="fullID"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public string GetStringByFullID(string fullID, ResourceContext context = null)
    {
        ResourceCandidate candidate;
        if (context is null)
        {
            candidate = _resManager.MainResourceMap.GetValue(fullID);
        }
        else
        {
            candidate = _resManager.MainResourceMap.GetValue(fullID, context);
        }
        return candidate.ValueAsString;
    }
}
