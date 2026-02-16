using System.Collections.Concurrent;
using System.Text;

using FileSorter.Comparison;
using FileSorter.Models;
using FileSorter.Parsing;

using Shared;

namespace FileSorter.Sorting;

public sealed class ChunkSorter
{
    private readonly SortConfig _config;

    public ChunkSorter(SortConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Pipeline parallel version: reads one chunk at a time while sorting/writing
    /// previous chunks in parallel. Only keeps MaxDegreeOfParallelism chunks in memory.
    /// Uses string pooling to deduplicate repeated text parts.
    /// </summary>
    public List<string> SplitAndSortParallel(string inputPath)
    {
        var chunkFiles = new ConcurrentDictionary<int, string>();
        var stringPool = new StringPool();
        int chunkIndex = 0;

        using var pendingChunks = new BlockingCollection<(Line[] Lines, int Count, int Index)>(
            boundedCapacity: _config.MaxDegreeOfParallelism);

        // Consumer: sort and write chunks in parallel
        var sortTask = Task.Run(() =>
        {
            Parallel.ForEach(
                pendingChunks.GetConsumingEnumerable(),
                new ParallelOptions { MaxDegreeOfParallelism = _config.MaxDegreeOfParallelism },
                chunkData =>
                {
                    string path = SortAndWriteChunk(chunkData.Lines, chunkData.Count, chunkData.Index);
                    chunkFiles[chunkData.Index] = path;
                    Console.WriteLine($"  Chunk {chunkData.Index + 1} sorted and written ({chunkData.Count:N0} lines)");
                });
        });

        // Producer: read file and enqueue chunks (using array instead of List for less GC)
        using (var reader = new StreamReader(inputPath, Encoding.UTF8, true, _config.IoBufferSize))
        {
            long effectiveChunkSize = _config.EffectiveChunkSize;
            // Estimate lines per chunk: avg ~35 bytes per line
            int estimatedLinesPerChunk = (int)(effectiveChunkSize / 35);
            var currentLines = new Line[estimatedLinesPerChunk];
            int lineCount = 0;
            long currentChunkBytes = 0;

            string? rawLine;
            while ((rawLine = reader.ReadLine()) != null)
            {
                if (rawLine.Length == 0)
                    continue;

                Line line = LineParser.ParsePooled(rawLine, stringPool);

                // Grow array if needed
                if (lineCount >= currentLines.Length)
                {
                    Array.Resize(ref currentLines, currentLines.Length * 2);
                }

                currentLines[lineCount++] = line;
                // Estimate bytes: chars + newline (avoids expensive Encoding.GetByteCount)
                currentChunkBytes += rawLine.Length + 1;

                if (currentChunkBytes >= effectiveChunkSize)
                {
                    pendingChunks.Add((currentLines, lineCount, chunkIndex));
                    currentLines = new Line[estimatedLinesPerChunk];
                    lineCount = 0;
                    currentChunkBytes = 0;
                    chunkIndex++;
                }
            }

            if (lineCount > 0)
            {
                pendingChunks.Add((currentLines, lineCount, chunkIndex));
                chunkIndex++;
            }
        }

        pendingChunks.CompleteAdding();
        sortTask.Wait();

        var result = new List<string>(chunkIndex);
        for (int i = 0; i < chunkIndex; i++)
        {
            result.Add(chunkFiles[i]);
        }

        Console.WriteLine($"  Total chunks created: {result.Count}");
        return result;
    }

    private string SortAndWriteChunk(Line[] lines, int count, int chunkIndex)
    {
        Array.Sort(lines, 0, count, LineComparer.Instance);

        string chunkPath = Path.Combine(_config.TempDirectory, $"chunk_{chunkIndex:D6}.tmp");

        using var writer = new StreamWriter(chunkPath, false, Encoding.UTF8, _config.IoBufferSize);
        for (int i = 0; i < count; i++)
        {
            // Write directly without ToString() allocation
            ref readonly var line = ref lines[i];
            writer.Write(line.Number);
            writer.Write(". ");
            writer.WriteLine(line.Text);
        }

        return chunkPath;
    }
}
