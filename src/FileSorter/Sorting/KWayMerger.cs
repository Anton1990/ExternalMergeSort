using System.Text;

using FileSorter.Comparison;
using FileSorter.Models;
using FileSorter.Parsing;

using Shared;

namespace FileSorter.Sorting;

public sealed class KWayMerger
{
    private readonly SortConfig _config;

    public KWayMerger(SortConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Merges multiple sorted chunk files into a single sorted output file
    /// using a PriorityQueue (min-heap) for efficient K-way merge.
    /// </summary>
    public void Merge(List<string> chunkFiles, string outputPath)
    {
        var readers = new StreamReader[chunkFiles.Count];
        var heap = new PriorityQueue<int, Line>(LineComparer.Instance);

        try
        {
            // Open all chunk files and seed the heap
            for (int i = 0; i < chunkFiles.Count; i++)
            {
                readers[i] = new StreamReader(chunkFiles[i], Encoding.UTF8, true, _config.IoBufferSize);
                if (TryReadNextLine(readers[i], out Line line))
                {
                    heap.Enqueue(i, line);
                }
            }

            long linesWritten = 0;
            long lastReportedMillion = 0;

            using var writer = new StreamWriter(outputPath, false, Encoding.UTF8, _config.IoBufferSize);

            while (heap.Count > 0)
            {
                heap.TryDequeue(out int fileIndex, out Line smallestLine);

                writer.Write(smallestLine.Number);
                writer.Write(". ");
                writer.WriteLine(smallestLine.Text);
                linesWritten++;

                if (linesWritten / 1_000_000 > lastReportedMillion)
                {
                    lastReportedMillion = linesWritten / 1_000_000;
                    Console.Write($"\r  Merging: {linesWritten:N0} lines written...   ");
                }

                if (TryReadNextLine(readers[fileIndex], out Line nextLine))
                {
                    heap.Enqueue(fileIndex, nextLine);
                }
            }

            Console.WriteLine($"\r  Merging complete: {linesWritten:N0} lines written.   ");
        }
        finally
        {
            foreach (var reader in readers)
            {
                reader?.Dispose();
            }
        }
    }

    private static bool TryReadNextLine(StreamReader reader, out Line line)
    {
        line = default;

        while (true)
        {
            string? rawLine = reader.ReadLine();
            if (rawLine == null)
                return false;

            if (string.IsNullOrWhiteSpace(rawLine))
                continue;

            line = LineParser.Parse(rawLine);
            return true;
        }
    }
}
