using FileSorter.Comparison;
using FileSorter.Models;

namespace FileSorter.Tests;

public class LineComparerTests
{
    private readonly LineComparer _comparer = LineComparer.Instance;

    [Fact]
    public void Compare_DifferentText_SortsByTextAlphabetically()
    {
        var apple = new Line(1, "Apple");
        var banana = new Line(1, "Banana");

        Assert.True(_comparer.Compare(apple, banana) < 0);
        Assert.True(_comparer.Compare(banana, apple) > 0);
    }

    [Fact]
    public void Compare_SameText_SortsByNumberAscending()
    {
        var first = new Line(1, "Apple");
        var second = new Line(415, "Apple");

        Assert.True(_comparer.Compare(first, second) < 0);
        Assert.True(_comparer.Compare(second, first) > 0);
    }

    [Fact]
    public void Compare_SameTextAndNumber_ReturnsZero()
    {
        var a = new Line(1, "Apple");
        var b = new Line(1, "Apple");

        Assert.Equal(0, _comparer.Compare(a, b));
    }

    [Fact]
    public void Compare_CaseSensitive_UppercaseBeforeLowercase()
    {
        var upper = new Line(1, "Apple");
        var lower = new Line(1, "apple");

        // Ordinal: uppercase letters come before lowercase
        Assert.True(_comparer.Compare(upper, lower) < 0);
    }

    [Fact]
    public void Compare_MatchesExpectedOutput()
    {
        // From the task description
        var lines = new List<Line>
        {
            new(415, "Apple"),
            new(30432, "Something something something"),
            new(1, "Apple"),
            new(32, "Cherry is the best"),
            new(2, "Banana is yellow"),
        };

        lines.Sort(_comparer);

        Assert.Equal(new Line(1, "Apple"), lines[0]);
        Assert.Equal(new Line(415, "Apple"), lines[1]);
        Assert.Equal(new Line(2, "Banana is yellow"), lines[2]);
        Assert.Equal(new Line(32, "Cherry is the best"), lines[3]);
        Assert.Equal(new Line(30432, "Something something something"), lines[4]);
    }
}
