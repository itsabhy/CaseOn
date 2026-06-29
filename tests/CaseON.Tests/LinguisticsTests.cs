using CaseON;
using Xunit;

namespace CaseON.Tests;

public class LinguisticsTests
{
    [Theory]
    [InlineData("Crème brûlée", "Creme brulee")]
    [InlineData("ß", "ss")]
    [InlineData("æon", "aeon")]
    [InlineData("naïve café", "naive cafe")]
    public void Transliterate_ToAscii_Works(string input, string expected) =>
        Assert.Equal(expected, Transliterate.ToAscii(input));

    [Fact]
    public void Transliterate_MapsSymbols() =>
        Assert.Equal("EUR 5", Transliterate.ToAscii("€ 5"));

    [Fact]
    public void Language_DetectsScript()
    {
        Assert.Equal(ScriptKind.Cyrillic, Language.Script("Привет мир"));
        Assert.Equal(ScriptKind.Latin, Language.Script("hello"));
        Assert.Equal(ScriptKind.Han, Language.Script("中文"));
    }

    [Fact]
    public void Language_DetectsLanguage()
    {
        Assert.Equal("en", Language.Detect("the quick brown fox is in the box and that is for you").IsoCode);
        Assert.Equal("ru", Language.Detect("Привет, как дела?").IsoCode);
        Assert.Equal("ja", Language.Detect("これはテストです").IsoCode);
    }

    [Fact]
    public void Profanity_DetectsAndCensors()
    {
        Assert.True(Profanity.Contains("you are a damn fool"));
        Assert.False(Profanity.Contains("you are a clever fellow"));
        Assert.Contains("damn", Profanity.Find("oh damn"));
        Assert.Equal("oh ****", Profanity.Censor("oh damn"));
    }

    [Fact]
    public void Profanity_RespectsCustomList()
    {
        var words = new[] { "foo" };
        Assert.True(Profanity.Contains("a foo walks in", words));
        Assert.Equal("a *** walks in", Profanity.Censor("a foo walks in", words));
    }
}
