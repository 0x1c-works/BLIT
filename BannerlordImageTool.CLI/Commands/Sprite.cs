using BannerlordImageTool.Sprite;

namespace BannerlordImageTool.CLI.Commands;

public class Sprite : ConsoleAppBase
{
    readonly SpriteUnpacker _unpacker = new();
    public void UnpackSingle(string srcFile, string dimensions, string outFile)
    {
        _unpacker.UnpackSingle(srcFile, outFile, SpriteRegion.FromString(dimensions));
    }

    public void UnpackCsv(string csvFile, string srcDir, string outDir, string srcExt = "png", string outExt = "png")
    {
        _unpacker.UnpackFromCSV(csvFile, srcDir, outDir, srcExt, outExt);
    }
}
