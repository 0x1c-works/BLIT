namespace BLIT.Banner;

public class BannerUtils {
    public static string GetGroupName(int groupID) {
        return $"banners_{groupID}";
    }

    public static string GetAtlasName(int groupID, int atlasIndex) {
        return $"{GetGroupName(groupID)}_{atlasIndex + 1:d2}";
    }
    public static int GetIconID(int groupID, int iconIndex) {
        var idstr = $"{groupID}{iconIndex + 1:d2}";
        return !int.TryParse(idstr, out var id) ? throw new InvalidDataException($"invalid banner icon ID: {idstr}") : id;
    }
}
