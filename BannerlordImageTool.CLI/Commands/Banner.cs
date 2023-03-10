using BannerlordImageTool.BannerTex;

namespace BannerlordImageTool.CLI.Commands
{
    public class Banner : ConsoleAppBase
    {
        public void MergeTex([Option(0)] string outFile, params string[] srcFiles)
        {
            var tm = new TextureMerger(OutputResolution.Res4K);
            tm.Merge(outFile, srcFiles);
        }
    }
}
