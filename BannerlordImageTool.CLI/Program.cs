// See https://aka.ms/new-console-template for more information
using BannerlordImageTool.BannerTex;
using BannerlordImageTool.Sprite;

Console.WriteLine("Hello, World!");

if (args.Length < 1)
{
    Console.WriteLine("need more parameters.\n" +
        "usage: BannerlordImageTool.CLI <command> <arguments>");
    return;
}

Dictionary<string, Func<string[], int>> _cmdHandlers = new() {
    {"bannerTex",DoBannerTex},
    {"unpackSprite",UnpackSprite},
};

var cmd = args[0];
if (!_cmdHandlers.TryGetValue(cmd, out var handler))
{
    var cmdList = string.Join((char)0, _cmdHandlers.Keys.Select(k => $"\n\t{k}"));
    Console.WriteLine("unknown command.\navailable commands:" + cmdList);
    return;
}
handler(args.Skip(1).ToArray());

static int DoBannerTex(string[] args)
{
    /* Usage:
     * bannerTex <outFile> [src1 [src2 ...]]
     */
    var outName = args[0];
    var srcFiles = args.Skip(1).ToArray();

    var tm = new TextureMerger(OutputResolution.Res4K);
    tm.Merge(outName, srcFiles);
    return 0;
}

static int UnpackSprite(string[] args)
{
    /* Usage: 
     * BannerlordImageTool.CLI unpackSprite <spritesheet> <x,y,w,h> <outfile>
     */
    if (args.Length != 3)
    {
        Console.WriteLine($"usage: <spritesheet> <x,y,w,h> <outFile>");
        return 1;
    }
    var unpacker = new SpriteUnpacker();
    unpacker.UnpackSingle(args[0], args[2], SpriteRegion.FromString(args[1]));
    return 0;
}
