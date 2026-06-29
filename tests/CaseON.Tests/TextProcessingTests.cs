using CaseON;
using Xunit;

namespace CaseON.Tests;

public class TextProcessingTests
{
    [Fact]
    public void Extract_FindsFragments()
    {
        const string text = "Email a@b.com or visit https://x.io #news @bob, call +1 415 555 2671, id 42.";
        Assert.Contains("a@b.com", Extract.Emails(text));
        Assert.Contains("https://x.io", Extract.Urls(text));
        Assert.Contains("news", Extract.Hashtags(text));
        Assert.Contains("bob", Extract.Mentions(text));
        Assert.Contains("42", Extract.Numbers(text));
    }

    [Fact]
    public void Normalize_Works()
    {
        Assert.Equal("cafe", Normalize.Accents("café"));
        Assert.Equal("a b c", Normalize.Whitespace("  a   b\t\nc  "));
        Assert.Equal("a\nb\nc", Normalize.LineEndings("a\r\nb\rc", "\n"));
        Assert.Equal("123", Normalize.Digits("\u0661\u0662\u0663")); // Arabic-Indic digits
    }

    [Fact]
    public void Clean_Works()
    {
        Assert.Equal("hi", Clean.RemoveHtml("<b>hi</b>"));
        Assert.Equal("abc", Clean.RemovePunctuation("a,b.c!"));
        Assert.Equal("abc", Clean.RemoveNumbers("a1b2c"));
        Assert.Equal("Hello", Clean.RemoveEmoji("Hello\U0001F600"));
    }

    [Fact]
    public void Tokenize_Works()
    {
        Assert.Equal(3, Tokenize.Words("one two three").Length);
        Assert.Equal(2, Tokenize.Sentences("Hi there. How are you?").Length);
        Assert.Equal(3, Tokenize.Lines("a\nb\nc").Length);
        Assert.Equal(new[] { "first", "name" }, Tokenize.Snake("first_name"));
        Assert.Equal(new[] { "first", "Name" }, Tokenize.Camel("firstName"));
    }

    [Fact]
    public void Slug_Generates()
    {
        Assert.Equal("hello-world", Slug.Generate("Héllo, World!"));
        Assert.Equal("a_b", Slug.Generate("a b", '_'));
    }

    [Theory]
    [InlineData("category", "categories")]
    [InlineData("person", "people")]
    [InlineData("bus", "buses")]
    [InlineData("knife", "knives")]
    public void Pluralize_Works(string singular, string plural) =>
        Assert.Equal(plural, Inflect.Pluralize(singular));

    [Theory]
    [InlineData("categories", "category")]
    [InlineData("people", "person")]
    public void Singularize_Works(string plural, string singular) =>
        Assert.Equal(singular, Inflect.Singularize(plural));

    [Theory]
    [InlineData(1, "1st")]
    [InlineData(2, "2nd")]
    [InlineData(3, "3rd")]
    [InlineData(11, "11th")]
    [InlineData(21, "21st")]
    [InlineData(112, "112th")]
    public void Ordinalize_Works(int number, string expected) =>
        Assert.Equal(expected, Inflect.Ordinalize(number));

    [Fact]
    public void InflectWord_RespectsCount()
    {
        Assert.Equal("case", Inflect.Word("cases", 1));
        Assert.Equal("cases", Inflect.Word("case", 3));
    }
}
