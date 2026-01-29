using BLIT.Sprite;

namespace BLIT.CLI.Commands;

/// <summary>Sprite unpacking commands.</summary>
public class SpriteCommands {
    private readonly SpriteUnpacker _unpacker = new();

    /// <summary>Unpack a single sprite image.</summary>
    /// <param name="srcFile">Source sprite file path.</param>
    /// <param name="dimensions">Sprite dimensions as WxH format.</param>
    /// <param name="outFile">Output file path.</param>
    public void UnpackSingle(string srcFile, string dimensions, string outFile) {
        _unpacker.UnpackSingle(srcFile, outFile, SpriteRegion.FromString(dimensions));
    }

    /// <summary>Unpack sprites from CSV definition file.</summary>
    /// <param name="csvFile">CSV definition file path.</param>
    /// <param name="srcDir">Source directory containing sprite files.</param>
    /// <param name="outDir">Output directory for unpacked sprites.</param>
    /// <param name="srcExt">Source file extension (default: png).</param>
    /// <param name="outExt">Output file extension (default: png).</param>
    public void UnpackCsv(string csvFile, string srcDir, string outDir, string srcExt = "png", string outExt = "png") {
        _unpacker.UnpackFromCSV(csvFile, srcDir, outDir, srcExt, outExt);
    }
}