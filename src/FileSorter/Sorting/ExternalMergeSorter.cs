using System.Diagnostics;

using Shared;

namespace FileSorter.Sorting;

public sealed class ExternalMergeSorter
{
    private readonly SortConfig _config;

    public ExternalMergeSorter(SortConfig config)
    {
        _config = config;
    }

    public void Sort(string inputPath, string outputPath)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException("Input file not found.", inputPath);

        var totalStopwatch = Stopwatch.StartNew();

        Console.WriteLine($"Input file: {inputPath}");
        Console.WriteLine($"Output file: {outputPath}");
        if (_config.MemoryLimitBytes > 0)
            Console.WriteLine($"Memory limit: {_config.MemoryLimitBytes / (1024.0 * 1024):F0} MB");
        Console.WriteLine($"Chunk size: {_config.EffectiveChunkSize / (1024.0 * 1024):F0} MB");
        Console.WriteLine($"Parallelism: {_config.MaxDegreeOfParallelism}");
        Console.WriteLine($"Temp dir: {_config.TempDirectory}");
        Console.WriteLine();

        // Ensure temp directory exists
        Directory.CreateDirectory(_config.TempDirectory);

        // Phase 1: Split & Sort
        Console.WriteLine("Phase 1: Splitting and sorting chunks...");
        var phase1Stopwatch = Stopwatch.StartNew();

        var chunkSorter = new ChunkSorter(_config);
        List<string> chunkFiles = chunkSorter.SplitAndSortParallel(inputPath);

        phase1Stopwatch.Stop();
        Console.WriteLine($"Phase 1 complete in {phase1Stopwatch.Elapsed}");
        Console.WriteLine();

        // Phase 2: K-Way Merge
        Console.WriteLine("Phase 2: Merging sorted chunks...");
        var phase2Stopwatch = Stopwatch.StartNew();

        var merger = new KWayMerger(_config);
        merger.Merge(chunkFiles, outputPath);

        phase2Stopwatch.Stop();
        Console.WriteLine($"Phase 2 complete in {phase2Stopwatch.Elapsed}");
        Console.WriteLine();

        // Cleanup temp files and directory
        Console.WriteLine("Cleaning up temporary files...");
        try
        {
            Directory.Delete(_config.TempDirectory, recursive: true);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: could not delete temp directory '{_config.TempDirectory}': {ex.Message}");
        }

        totalStopwatch.Stop();
        Console.WriteLine();
        Console.WriteLine($"Sorting complete!");
        Console.WriteLine($"  Total time: {totalStopwatch.Elapsed}");
        Console.WriteLine($"  Phase 1 (split+sort): {phase1Stopwatch.Elapsed}");
        Console.WriteLine($"  Phase 2 (merge):      {phase2Stopwatch.Elapsed}");

        var inputInfo = new FileInfo(inputPath);
        var outputInfo = new FileInfo(outputPath);
        Console.WriteLine($"  Input size:  {inputInfo.Length / (1024.0 * 1024):F2} MB");
        Console.WriteLine($"  Output size: {outputInfo.Length / (1024.0 * 1024):F2} MB");
    }
}
