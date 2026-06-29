using System.Collections.Generic;
using CaseON;
using Xunit;

namespace CaseON.Tests;

public class AnalysisTests
{
    [Fact]
    public void Compare_Works()
    {
        Assert.True(Compare.Equals("a", "A", ignoreCase: true));
        Assert.False(Compare.Equals("a", "A"));
        Assert.True(Compare.StartsWith("hello", "HE", ignoreCase: true));
        Assert.True(Compare.NaturalCompare("file2", "file10") < 0);
        Assert.True(Compare.NaturalCompare("file10", "file2") > 0);
    }

    [Fact]
    public void Search_Works()
    {
        Assert.True(Search.ContainsWord("the cat sat", "cat"));
        Assert.False(Search.ContainsWord("category", "cat"));
        Assert.True(Search.ContainsAny("hello world", "x", "world"));
        Assert.True(Search.ContainsAll("hello world", "hello", "world"));
        Assert.False(Search.ContainsAll("hello", "hello", "world"));
        Assert.True(Search.IsDigits("12345"));
        Assert.True(Search.IsLetters("abc"));
        Assert.False(Search.IsLetters("ab1"));
    }

    [Fact]
    public void Template_Format_And_Interpolate()
    {
        var values = new Dictionary<string, object?> { ["name"] = "Bob", ["count"] = 3 };
        Assert.Equal("Hi Bob, 3 items", Template.Format("Hi {name}, {count} items", values));
        Assert.Equal("{x}", Template.Format("{{x}}", values));
        Assert.Equal("Hi Bob", Template.Interpolate("Hi {Name}", new { Name = "Bob" }));
    }

    [Fact]
    public void Metrics_Works()
    {
        Assert.Equal(3, Metrics.WordCount("one two three"));
        Assert.Equal(0.0, Metrics.Entropy("aaaa"), 6);
        Assert.Equal(1.0, Metrics.Entropy("ab"), 6);
        Assert.True(Metrics.ReadingTime("word ", 200) > TimeSpan.Zero);
    }

    [Fact]
    public void Analyze_Text_ProducesStats()
    {
        var stats = Analyze.Text("Hello world. Bye now.");
        Assert.Equal(4, stats.Words);
        Assert.Equal(2, stats.Sentences);
        Assert.True(stats.AverageWordLength > 0);
    }

    [Fact]
    public void Password_Validation_Works()
    {
        var weak = Password.Validate("abc");
        Assert.False(weak.IsValid);
        Assert.NotEmpty(weak.Errors);

        var strong = Password.Validate("Aa1!bcde");
        Assert.True(strong.IsValid);
        Assert.True(strong.Entropy > 0);
        Assert.True(Password.Strength("Tr0ub4dour&3xtra!Long") >= PasswordStrength.Strong);
    }

    [Fact]
    public void RegexTools_Works()
    {
        Assert.Equal(@"a\.b", RegexTools.Escape("a.b"));
        Assert.Equal(new[] { "1", "2" }, RegexTools.Matches("a1b2", @"\d"));
        Assert.Equal("a#b#", RegexTools.Replace("a1b2", @"\d", "#"));

        var groups = RegexTools.NamedGroups("2023-01", @"(?<y>\d{4})-(?<m>\d{2})");
        Assert.Equal("2023", groups["y"]);
        Assert.Equal("01", groups["m"]);
    }

    [Fact]
    public void Diff_Lines_Works()
    {
        var diff = Diff.Lines("a\nb\nc", "a\nx\nc");
        Assert.Contains("b", diff.Removed);
        Assert.Contains("x", diff.Added);
        Assert.Contains("a", diff.Unchanged);
        Assert.Contains("c", diff.Unchanged);
    }

    [Fact]
    public void Unicode_Works()
    {
        Assert.True(Unicode.IsEmoji("hi \U0001F642"));
        Assert.False(Unicode.IsEmoji("hi"));
        Assert.Equal(2, Unicode.CodePointCount("a\U0001F30D"));
        Assert.Equal(1, Unicode.SurrogatePairCount("a\U0001F30D"));
        Assert.True(Unicode.IsRightToLeft("\u05E9\u05DC\u05D5\u05DD"));
        Assert.False(Unicode.IsRightToLeft("hello"));
    }
}
