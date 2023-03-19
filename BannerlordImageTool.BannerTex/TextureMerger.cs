using ImageMagick;

namespace BannerlordImageTool.Banner;

public class TextureMerger
{
    public const int ROWS = 4;
    public const int COLS = 4;

    static readonly Dictionary<OutputResolution, int> CELL_SIZES = new()
    {
        {OutputResolution.Res2K,512 },
        {OutputResolution.Res4K,1024 },
    };

    private OutputResolution _resolution;

    public TextureMerger(OutputResolution resolution)
    {
        _resolution = resolution;
        MagickNET.SetGhostscriptDirectory(AppDomain.CurrentDomain.BaseDirectory);
    }

    public void Merge(string outDir, int groupID, string[] sourceFileNames)
    {
        using var collection = new MagickImageCollection();
        var outBasePath = Path.Join(outDir, BannerUtils.GetGroupName(groupID));

        var next = sourceFileNames;
        var index = 0;
        while (next.Length > 0)
        {
            next = MakeSingleTexture(outBasePath, index++, next);
        }
    }

    private string[] MakeSingleTexture(string outBasePath, int texIndex, string[] sourceFileNames)
    {
        var index = 0;
        using var tex = new MagickImageCollection();
        MagickImageCollection? row = null;
        try
        {
            while (index < ROWS * COLS)
            {
                var processedCount = MakeRow(tex, sourceFileNames.Skip(index).Take(COLS));
                if (processedCount == 0) break;
                index += processedCount;
            }
            if (tex.Count > 0)
            {
                // output tex
                var outputFile = ($"{outBasePath}_{texIndex + 1:d2}.psd");
                var output = tex.AppendVertically();
                output.BackgroundColor = new MagickColor(0, 0, 0, 0);
                output.Extent(GetTextureGeometry(), Gravity.Northwest);
                output.Write(outputFile);
                Console.WriteLine($"Generated: {outputFile}");
            }
            return sourceFileNames.Skip(index).ToArray();
        }
        finally
        {
            row?.Dispose();
        }
    }

    private int MakeRow(MagickImageCollection tex, IEnumerable<string> files)
    {
        if (!files.Any()) return 0;

        using var row = new MagickImageCollection();
        var count = 0;
        foreach (var file in files)
        {
            row.Add(ResizeCell(new MagickImage(file)));
            count++;
        }
        if (row.Count > 0)
        {
            var rowTex = row.AppendHorizontally();
            rowTex.BackgroundColor = MagickColor.FromRgba(0, 0, 0, 0);
            tex.Add(rowTex);
        }
        return count;
    }

    private MagickGeometry GetTextureGeometry()
    {
        if (!CELL_SIZES.TryGetValue(_resolution, out var size))
        {
            throw new ArgumentException($"unsupported output resolution: {_resolution}");
        }
        return new MagickGeometry(size * COLS, size * ROWS);
    }

    private MagickGeometry GetCellGeometry()
    {
        if (!CELL_SIZES.TryGetValue(_resolution, out var size))
        {
            throw new ArgumentException($"unsupported output resolution: {_resolution}");
        }
        return new MagickGeometry(size);
    }

    private IMagickImage<ushort> ResizeCell(IMagickImage<ushort> image)
    {
        var geo = GetCellGeometry();
        image.Resize(geo);
        image.Crop(geo, Gravity.Center);
        image.RePage();
        return image;
    }
}

public enum OutputResolution
{
    INVALID = -1,
    Res2K,
    Res4K,
}
