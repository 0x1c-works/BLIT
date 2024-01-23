using ImageMagick;

namespace BLIT.Banner;

public class TextureMerger {
    public const int ROWS = 4;
    public const int COLS = 4;
    private static readonly string TEXTURE_SUB_FOLDER = Path.Join("AssetSources", "BannerIcons");
    private static readonly Dictionary<OutputResolution, int> CELL_SIZES = new()
    {
        {OutputResolution.Res2K,512 },
        {OutputResolution.Res4K,1024 },
    };
    private readonly OutputResolution _resolution;

    public TextureMerger(OutputResolution resolution) {
        _resolution = resolution;
        MagickNET.SetGhostscriptDirectory(AppDomain.CurrentDomain.BaseDirectory);
    }

    private static string EnsureOutFolder(string outDir) {
        var dir = Path.Join(outDir, TEXTURE_SUB_FOLDER);
        return Directory.CreateDirectory(dir).FullName;
    }

    public void Merge(string outDir, int groupID, string[] sourceFileNames) {
        using var collection = new MagickImageCollection();
        var outBasePath = Path.Join(EnsureOutFolder(outDir), BannerUtils.GetGroupName(groupID));

        var next = sourceFileNames;
        var index = 0;
        while (next.Length > 0) {
            next = MakeSingleTexture(outBasePath, index++, next);
        }
    }

    private string[] MakeSingleTexture(string outBasePath, int texIndex, string[] sourceFileNames) {
        var index = 0;
        using var tex = new MagickImageCollection();
        MagickImageCollection? row = null;
        try {
            while (index < ROWS * COLS) {
                var processedCount = MakeRow(tex, sourceFileNames.Skip(index).Take(COLS));
                if (processedCount == 0) {
                    break;
                }

                index += processedCount;
            }
            if (tex.Count > 0) {
                // output tex
                var outputFile = $"{outBasePath}_{texIndex + 1:d2}.psd";
                IMagickImage<ushort> output = tex.AppendVertically();
                output.BackgroundColor = new MagickColor(0, 0, 0, 0);
                output.Extent(GetTextureGeometry(), Gravity.Northwest);
                output.Write(outputFile);
                Console.WriteLine($"Generated: {outputFile}");
            }
            return sourceFileNames.Skip(index).ToArray();
        } finally {
            row?.Dispose();
        }
    }

    private int MakeRow(MagickImageCollection tex, IEnumerable<string> files) {
        if (!files.Any()) {
            return 0;
        }

        using var row = new MagickImageCollection();
        var count = 0;
        foreach (var file in files) {
            row.Add(ResizeCell(new MagickImage(file)));
            count++;
        }
        if (row.Count > 0) {
            IMagickImage<ushort> rowTex = row.AppendHorizontally();
            rowTex.BackgroundColor = MagickColor.FromRgba(0, 0, 0, 0);
            tex.Add(rowTex);
        }
        return count;
    }

    private MagickGeometry GetTextureGeometry() {
        return !CELL_SIZES.TryGetValue(_resolution, out var size)
            ? throw new ArgumentException($"unsupported output resolution: {_resolution}")
            : new MagickGeometry(size * COLS, size * ROWS);
    }

    private MagickGeometry GetCellGeometry() {
        return !CELL_SIZES.TryGetValue(_resolution, out var size)
            ? throw new ArgumentException($"unsupported output resolution: {_resolution}")
            : new MagickGeometry(size);
    }

    private IMagickImage<ushort> ResizeCell(IMagickImage<ushort> image) {
        MagickGeometry geo = GetCellGeometry();
        image.Resize(geo);
        image.Crop(geo, Gravity.Center);
        image.RePage();
        return image;
    }
}

public enum OutputResolution {
    INVALID = -1,
    Res2K,
    Res4K,
}
