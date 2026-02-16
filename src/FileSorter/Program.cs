using FileSorter.Sorting;

using Shared;

string inputPath;
string outputPath;

if (args.Length >= 2)
{
    inputPath = args[0];
    outputPath = args[1];
}
else
{
    string defaultInput = Path.Combine(SolutionRoot.Path, "in.txt");
    string defaultOutput = Path.Combine(SolutionRoot.Path, "out.txt");

    Console.WriteLine("=== External Merge Sort ===");
    Console.WriteLine();
    Console.Write($"Enter input file path (press Enter for {defaultInput}): ");
    string? inInput = Console.ReadLine()?.Trim().Trim('"');
    inputPath = string.IsNullOrEmpty(inInput) ? defaultInput : inInput;

    Console.Write($"Enter output file path (press Enter for {defaultOutput}): ");
    string? outInput = Console.ReadLine()?.Trim().Trim('"');
    outputPath = string.IsNullOrEmpty(outInput) ? defaultOutput : outInput;

    Console.Write("Enter memory limit (e.g. 2GB, 512MB) or press Enter for auto: ");
    string? memInput = Console.ReadLine()?.Trim();

    if (!string.IsNullOrEmpty(memInput))
    {
        args = new[] { inputPath, outputPath, "--memory", memInput };
    }
    else
    {
        args = new[] { inputPath, outputPath };
    }
}

var config = new SortConfig();

// Parse optional arguments
for (int i = 2; i < args.Length - 1; i++)
{
    switch (args[i].ToLowerInvariant())
    {
        case "--memory":
            config.MemoryLimitBytes = ParseSize(args[++i]);
            break;
        case "--chunk":
            if (int.TryParse(args[++i], out int chunkMb))
                config.ChunkSizeBytes = (long)chunkMb * 1024 * 1024;
            break;
        case "--threads":
            if (int.TryParse(args[++i], out int threads))
                config = new SortConfig
                {
                    ChunkSizeBytes = config.ChunkSizeBytes,
                    MemoryLimitBytes = config.MemoryLimitBytes,
                    MaxDegreeOfParallelism = threads,
                    TempDirectory = config.TempDirectory,
                    IoBufferSize = config.IoBufferSize,
                };
            break;
        case "--temp":
            config = new SortConfig
            {
                ChunkSizeBytes = config.ChunkSizeBytes,
                MemoryLimitBytes = config.MemoryLimitBytes,
                MaxDegreeOfParallelism = config.MaxDegreeOfParallelism,
                TempDirectory = args[++i],
                IoBufferSize = config.IoBufferSize,
            };
            break;
    }
}

var sorter = new ExternalMergeSorter(config);
sorter.Sort(inputPath, outputPath);

return 0;

static long ParseSize(string sizeStr)
{
    sizeStr = sizeStr.Trim().ToUpperInvariant();

    if (sizeStr.EndsWith("GB"))
        return (long)(double.Parse(sizeStr[..^2]) * 1024 * 1024 * 1024);
    if (sizeStr.EndsWith("MB"))
        return (long)(double.Parse(sizeStr[..^2]) * 1024 * 1024);

    return long.Parse(sizeStr);
}
