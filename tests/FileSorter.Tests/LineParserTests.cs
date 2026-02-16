using FileSorter.Models;
using FileSorter.Parsing;

namespace FileSorter.Tests;

public class LineParserTests
{
    [Fact]
    public void Parse_ValidLine_ReturnsCorrectNumberAndText()
    {
        var result = LineParser.Parse("415. Apple");
        Assert.Equal(415, result.Number);
        Assert.Equal("Apple", result.Text);
    }

    [Fact]
    public void Parse_LargeNumber_ReturnsCorrectNumber()
    {
        var result = LineParser.Parse("999999999999. Very long number");
        Assert.Equal(999999999999L, result.Number);
        Assert.Equal("Very long number", result.Text);
    }

    [Fact]
    public void Parse_TextWithDots_ParsesCorrectly()
    {
        var result = LineParser.Parse("1. Hello. World. Test");
        Assert.Equal(1, result.Number);
        Assert.Equal("Hello. World. Test", result.Text);
    }

    [Fact]
    public void Parse_TextWithSpaces_ParsesCorrectly()
    {
        var result = LineParser.Parse("30432. Something something something");
        Assert.Equal(30432, result.Number);
        Assert.Equal("Something something something", result.Text);
    }

    [Fact]
    public void Parse_NumberOne_ParsesCorrectly()
    {
        var result = LineParser.Parse("1. Apple");
        Assert.Equal(1, result.Number);
        Assert.Equal("Apple", result.Text);
    }

    [Fact]
    public void Parse_InvalidFormat_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => LineParser.Parse("no dot here"));
    }

    [Fact]
    public void TryParse_ValidLine_ReturnsTrueAndLine()
    {
        bool success = LineParser.TryParse("42. Banana", out Line line);
        Assert.True(success);
        Assert.Equal(42, line.Number);
        Assert.Equal("Banana", line.Text);
    }

    [Fact]
    public void TryParse_InvalidFormat_ReturnsFalse()
    {
        bool success = LineParser.TryParse("invalid", out _);
        Assert.False(success);
    }

    [Fact]
    public void TryParse_NonNumericBeforeDot_ReturnsFalse()
    {
        bool success = LineParser.TryParse("abc. text", out _);
        Assert.False(success);
    }

    [Fact]
    public void Line_ToString_ProducesCorrectFormat()
    {
        var line = new Line(415, "Apple");
        Assert.Equal("415. Apple", line.ToString());
    }
}
