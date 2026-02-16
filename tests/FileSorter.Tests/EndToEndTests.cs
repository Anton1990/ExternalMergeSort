using System.Text;

using FileSorter.Comparison;
using FileSorter.Models;
using FileSorter.Parsing;
using FileSorter.Sorting;

using Shared;

namespace FileSorter.Tests;

public class EndToEndTests : IDisposable
{
    private readonly string _tempDir;

    public EndToEndTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"sort_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void Sort_TaskExample_ProducesExpectedOutput()
    {
        string inputPath = Path.Combine(_tempDir, "input.txt");
        string outputPath = Path.Combine(_tempDir, "output.txt");

        File.WriteAllLines(inputPath, new[]
        {
            "415. Apple",
            "30432. Something something something",
            "1. Apple",
            "32. Cherry is the best",
            "2. Banana is yellow",
        });

        var config = new SortConfig
        {
            ChunkSizeBytes = 1024, // very small chunks to test multi-chunk
            TempDirectory = _tempDir,
        };

        var sorter = new ExternalMergeSorter(config);
        sorter.Sort(inputPath, outputPath);

        var outputLines = File.ReadAllLines(outputPath);

        Assert.Equal(5, outputLines.Length);
        Assert.Equal("1. Apple", outputLines[0]);
        Assert.Equal("415. Apple", outputLines[1]);
        Assert.Equal("2. Banana is yellow", outputLines[2]);
        Assert.Equal("32. Cherry is the best", outputLines[3]);
        Assert.Equal("30432. Something something something", outputLines[4]);
    }

    [Fact]
    public void Sort_SingleLine_Works()
    {
        string inputPath = Path.Combine(_tempDir, "input.txt");
        string outputPath = Path.Combine(_tempDir, "output.txt");

        File.WriteAllText(inputPath, "1. Hello\n");

        var config = new SortConfig { TempDirectory = _tempDir };
        var sorter = new ExternalMergeSorter(config);
        sorter.Sort(inputPath, outputPath);

        var lines = File.ReadAllLines(outputPath);
        Assert.Single(lines);
        Assert.Equal("1. Hello", lines[0]);
    }

    [Fact]
    public void Sort_AllSameString_SortsByNumber()
    {
        string inputPath = Path.Combine(_tempDir, "input.txt");
        string outputPath = Path.Combine(_tempDir, "output.txt");

        File.WriteAllLines(inputPath, new[]
        {
            "100. Same",
            "1. Same",
            "50. Same",
            "10. Same",
        });

        var config = new SortConfig
        {
            ChunkSizeBytes = 50,
            TempDirectory = _tempDir,
        };

        var sorter = new ExternalMergeSorter(config);
        sorter.Sort(inputPath, outputPath);

        var outputLines = File.ReadAllLines(outputPath);
        Assert.Equal(4, outputLines.Length);
        Assert.Equal("1. Same", outputLines[0]);
        Assert.Equal("10. Same", outputLines[1]);
        Assert.Equal("50. Same", outputLines[2]);
        Assert.Equal("100. Same", outputLines[3]);
    }

    [Fact]
    public void Sort_MultipleChunks_MergesCorrectly()
    {
        string inputPath = Path.Combine(_tempDir, "input.txt");
        string outputPath = Path.Combine(_tempDir, "output.txt");

        var random = new Random(42);
        var sb = new StringBuilder();
        var expected = new List<Line>();

        for (int i = 0; i < 1000; i++)
        {
            long number = random.NextInt64(1, 100_000);
            string text = $"Word{random.Next(1, 50)}";
            sb.AppendLine($"{number}. {text}");
            expected.Add(new Line(number, text));
        }

        File.WriteAllText(inputPath, sb.ToString());

        expected.Sort(LineComparer.Instance);

        var config = new SortConfig
        {
            ChunkSizeBytes = 2 * 1024, // 2 KB chunks = many chunks
            TempDirectory = _tempDir,
        };

        var sorter = new ExternalMergeSorter(config);
        sorter.Sort(inputPath, outputPath);

        var outputLines = File.ReadAllLines(outputPath);
        Assert.Equal(expected.Count, outputLines.Length);

        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].ToString(), outputLines[i]);
        }
    }

    [Fact]
    public void Sort_EmptyFile_ProducesEmptyOutput()
    {
        string inputPath = Path.Combine(_tempDir, "input.txt");
        string outputPath = Path.Combine(_tempDir, "output.txt");

        File.WriteAllText(inputPath, "");

        var config = new SortConfig { TempDirectory = _tempDir };
        var sorter = new ExternalMergeSorter(config);
        sorter.Sort(inputPath, outputPath);

        var lines = File.ReadAllLines(outputPath);
        Assert.Empty(lines);
    }
}
