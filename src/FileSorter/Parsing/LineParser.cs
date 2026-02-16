using System.Collections.Concurrent;

using FileSorter.Models;

namespace FileSorter.Parsing;

public static class LineParser
{
    private const string Separator = ". ";

    public static Line Parse(string rawLine)
    {
        var span = rawLine.AsSpan();
        int separatorIndex = span.IndexOf(Separator.AsSpan(), StringComparison.Ordinal);

        if (separatorIndex < 0)
            throw new FormatException($"Invalid line format: '{rawLine}'");

        long number = long.Parse(span[..separatorIndex]);
        string text = span[(separatorIndex + Separator.Length)..].ToString();

        return new Line(number, text);
    }

    /// <summary>
    /// Parses with string pooling — reuses string instances for duplicate text parts.
    /// Drastically reduces memory and GC pressure for files with repeated strings.
    /// </summary>
    public static Line ParsePooled(string rawLine, StringPool pool)
    {
        var span = rawLine.AsSpan();
        int separatorIndex = span.IndexOf(Separator.AsSpan(), StringComparison.Ordinal);

        if (separatorIndex < 0)
            throw new FormatException($"Invalid line format: '{rawLine}'");

        long number = long.Parse(span[..separatorIndex]);
        string text = pool.GetOrAdd(span[(separatorIndex + Separator.Length)..]);

        return new Line(number, text);
    }

    public static bool TryParse(string rawLine, out Line line)
    {
        line = default;
        var span = rawLine.AsSpan();
        int separatorIndex = span.IndexOf(Separator.AsSpan(), StringComparison.Ordinal);

        if (separatorIndex < 0)
            return false;

        if (!long.TryParse(span[..separatorIndex], out long number))
            return false;

        string text = span[(separatorIndex + Separator.Length)..].ToString();
        line = new Line(number, text);
        return true;
    }
}

/// <summary>
/// Thread-safe string pool. Deduplicates identical strings to reduce memory usage.
/// </summary>
public sealed class StringPool
{
    private readonly ConcurrentDictionary<string, string> _pool = new(StringComparer.Ordinal);

    public string GetOrAdd(ReadOnlySpan<char> span)
    {
        // First, create a temporary string to look up
        string key = span.ToString();
        return _pool.GetOrAdd(key, key);
    }
}
