namespace Shared;

public sealed class SortConfig
{
    /// <summary>
    /// Maximum chunk size in bytes to read into memory at once.
    /// Calculated automatically from MemoryLimitBytes and MaxDegreeOfParallelism.
    /// </summary>
    public long ChunkSizeBytes { get; set; } = 512L * 1024 * 1024;

    /// <summary>
    /// Total memory budget for the sorter.
    /// ChunkSizeBytes will be calculated as: MemoryLimitBytes / MaxDegreeOfParallelism.
    /// Default: 0 (disabled, uses ChunkSizeBytes directly).
    /// </summary>
    public long MemoryLimitBytes { get; set; }

    /// <summary>
    /// I/O buffer size for StreamReader/StreamWriter.
    /// Default: 128 KB.
    /// </summary>
    public int IoBufferSize { get; init; } = 128 * 1024;

    /// <summary>
    /// Directory for temporary chunk files. Defaults to system temp.
    /// </summary>
    public string TempDirectory { get; init; } = Path.Combine(SolutionRoot.Path, "chunks");

    /// <summary>
    /// Max degree of parallelism for sorting chunks.
    /// Default: number of processors.
    /// </summary>
    public int MaxDegreeOfParallelism { get; init; } = Environment.ProcessorCount;

    /// <summary>
    /// If MemoryLimitBytes is set, calculates optimal chunk size.
    /// Otherwise returns ChunkSizeBytes as-is.
    /// </summary>
    public long EffectiveChunkSize =>
        MemoryLimitBytes > 0
            ? MemoryLimitBytes / MaxDegreeOfParallelism
            : ChunkSizeBytes;
}
