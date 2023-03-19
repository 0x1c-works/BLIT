using BannerlordImageTool.Banner;

namespace BannerlordImageTool.CLI.Commands
{
    public class Banner : ConsoleAppBase
    {
        public void MergeTex([Option(0)] string outDir, [Option(1)] int groupId, params string[] srcFiles)
        {
            var tm = new TextureMerger(OutputResolution.Res4K);
            tm.Merge(outDir, groupId, srcFiles);
        }
    }
}
