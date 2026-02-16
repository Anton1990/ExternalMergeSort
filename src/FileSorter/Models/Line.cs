namespace FileSorter.Models;

public readonly record struct Line(long Number, string Text)
{
    public override string ToString() => $"{Number}. {Text}";
}
