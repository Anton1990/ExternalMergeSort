using FileSorter.Models;

namespace FileSorter.Comparison;

public sealed class LineComparer : IComparer<Line>
{
    public static readonly LineComparer Instance = new();

    public int Compare(Line x, Line y)
    {
        int textComparison = string.Compare(x.Text, y.Text, StringComparison.Ordinal);
        return textComparison != 0
            ? textComparison
            : x.Number.CompareTo(y.Number);
    }
}
