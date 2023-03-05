// See https://aka.ms/new-console-template for more information
using BannerlordImageTool.BannerTex;

Console.WriteLine("Hello, World!");

if (args.Length < 2)
{
    Console.WriteLine("need more parameters.\n" +
        "usage: BannerlordImageTool.CLI <outName> [srcFile1 srcFile2 ...]");

}
else
{
    var outName = args[0];
    var srcFiles = args.Skip(1).ToArray();

    var tm = new TextureMerger(OutputResolution.Res4K);
    tm.Merge(outName, srcFiles);
}
