using System.Diagnostics;
using System.Text;

using Shared;

string outputPath;
long targetSize;

if (args.Length >= 2)
{
    outputPath = args[0];
    targetSize = ParseSize(args[1]);
}
else
{
    string defaultPath = Path.Combine(SolutionRoot.Path, "in.txt");

    Console.WriteLine("=== File Generator ===");
    Console.WriteLine();
    Console.Write($"Enter output file path (press Enter for {defaultPath}): ");
    string? input = Console.ReadLine()?.Trim().Trim('"');
    outputPath = string.IsNullOrEmpty(input) ? defaultPath : input;

    Console.Write("Enter target file size (e.g. 100MB, 1GB, 10GB): ");
    string? sizeInput = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(sizeInput))
    {
        Console.WriteLine("Error: size cannot be empty.");
        return 1;
    }

    targetSize = ParseSize(sizeInput);
}

Console.WriteLine($"Generating file: {outputPath}");
Console.WriteLine($"Target size: {FormatSize(targetSize)}");

var stopwatch = Stopwatch.StartNew();

string[] sampleStrings = GenerateSampleStrings(1000);
var random = new Random(42);

long bytesWritten = 0;
long lineCount = 0;
long lastReportedPercent = -1;

using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8, bufferSize: 128 * 1024))
{
    while (bytesWritten < targetSize)
    {
        long number = random.NextInt64(1, 1_000_000_000);
        string text = sampleStrings[random.Next(sampleStrings.Length)];
        string line = $"{number}. {text}";

        writer.WriteLine(line);
        bytesWritten += Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length;
        lineCount++;

        long percent = bytesWritten * 100 / targetSize;
        if (percent != lastReportedPercent && percent % 5 == 0)
        {
            lastReportedPercent = percent;
            Console.Write($"\rProgress: {percent}% ({FormatSize(bytesWritten)} / {FormatSize(targetSize)})   ");
        }
    }
}

stopwatch.Stop();
Console.WriteLine();
Console.WriteLine($"Done! Lines: {lineCount:N0}, Size: {FormatSize(bytesWritten)}, Time: {stopwatch.Elapsed}");
return 0;

static string[] GenerateSampleStrings(int count)
{
    var words = new[]
    {
        "Apple", "Banana", "Cherry", "Dragon fruit", "Elderberry",
        "Fig", "Grape", "Honeydew", "Indian fig", "Jackfruit",
        "Kiwi", "Lemon", "Mango", "Nectarine", "Orange",
        "Papaya", "Quince", "Raspberry", "Strawberry", "Tangerine",
        "something", "is the best", "is yellow", "is great", "is awesome",
        "tastes good", "very delicious", "fresh and ripe", "organic", "tropical",
    };

    var random = new Random(123);
    var strings = new string[count];

    for (int i = 0; i < count; i++)
    {
        int wordCount = random.Next(1, 5);
        var sb = new StringBuilder();
        for (int w = 0; w < wordCount; w++)
        {
            if (w > 0) sb.Append(' ');
            sb.Append(words[random.Next(words.Length)]);
        }
        strings[i] = sb.ToString();
    }

    return strings;
}

static long ParseSize(string sizeStr)
{
    sizeStr = sizeStr.Trim().ToUpperInvariant();

    if (sizeStr.EndsWith("GB"))
        return (long)(double.Parse(sizeStr[..^2]) * 1024 * 1024 * 1024);
    if (sizeStr.EndsWith("MB"))
        return (long)(double.Parse(sizeStr[..^2]) * 1024 * 1024);
    if (sizeStr.EndsWith("KB"))
        return (long)(double.Parse(sizeStr[..^2]) * 1024);

    return long.Parse(sizeStr);
}

static string FormatSize(long bytes)
{
    if (bytes >= 1024L * 1024 * 1024)
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    if (bytes >= 1024L * 1024)
        return $"{bytes / (1024.0 * 1024):F2} MB";
    if (bytes >= 1024)
        return $"{bytes / 1024.0:F2} KB";
    return $"{bytes} B";
}
