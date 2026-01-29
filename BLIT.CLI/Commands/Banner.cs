using BLIT.Banner;

namespace BLIT.CLI.Commands;

/// <summary>Banner texture merging commands.</summary>
public class BannerCommands {
    /// <summary>Merge banner textures.</summary>
    /// <param name="outDir">Output directory path.</param>
    /// <param name="groupId">Banner group ID.</param>
    /// <param name="srcFiles">Source texture files.</param>
    public void MergeTex(string outDir, int groupId, params string[] srcFiles) {
        var tm = new TextureMerger(OutputResolution.Res4K);
        tm.Merge(outDir, groupId, srcFiles);
    }
}