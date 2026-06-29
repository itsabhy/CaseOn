using CaseON;
using Xunit;

namespace CaseON.Tests;

public class SimilarityPhoneticFuzzyTests
{
    [Theory]
    [InlineData("kitten", "sitting", 3)]
    [InlineData("", "abc", 3)]
    [InlineData("same", "same", 0)]
    public void Levenshtein_Distance_Works(string a, string b, int expected) =>
        Assert.Equal(expected, Similarity.LevenshteinDistance(a, b));

    [Fact]
    public void Levenshtein_Span_Matches_String() =>
        Assert.Equal(
            Similarity.LevenshteinDistance("kitten", "sitting"),
            Similarity.LevenshteinDistance("kitten".AsSpan(), "sitting".AsSpan()));

    [Fact]
    public void Damerau_CountsTransposition() =>
        Assert.Equal(1, Similarity.DamerauLevenshteinDistance("ab", "ba"));

    [Fact]
    public void Hamming_RequiresEqualLength() =>
        Assert.Throws<ArgumentException>(() => Similarity.HammingDistance("abc", "ab"));

    [Theory]
    [InlineData(SimilarityAlgorithm.Levenshtein)]
    [InlineData(SimilarityAlgorithm.DamerauLevenshtein)]
    [InlineData(SimilarityAlgorithm.Jaro)]
    [InlineData(SimilarityAlgorithm.JaroWinkler)]
    [InlineData(SimilarityAlgorithm.Lcs)]
    [InlineData(SimilarityAlgorithm.SorensenDice)]
    [InlineData(SimilarityAlgorithm.Jaccard)]
    [InlineData(SimilarityAlgorithm.Cosine)]
    public void Compute_StaysWithinBounds(SimilarityAlgorithm algorithm)
    {
        var score = Similarity.Compute("hello world", "hallo word", algorithm);
        Assert.InRange(score, 0.0, 1.0);
        Assert.Equal(1.0, Similarity.Compute("same text", "same text", algorithm), 10);
    }

    [Fact]
    public void JaroWinkler_BoostsPrefix() =>
        Assert.InRange(Similarity.JaroWinkler("MARTHA", "MARHTA"), 0.95, 0.97);

    [Theory]
    [InlineData("Robert", "R163")]
    [InlineData("Rupert", "R163")]
    [InlineData("Tymczak", "T522")]
    public void Soundex_Works(string word, string expected) =>
        Assert.Equal(expected, Phonetic.Soundex(word));

    [Fact]
    public void SoundexEquals_Works() => Assert.True(Phonetic.SoundexEquals("Robert", "Rupert"));

    [Fact]
    public void Phonetic_Encoders_ReturnNonEmpty()
    {
        Assert.NotEmpty(Phonetic.Metaphone("Thompson"));
        Assert.NotEmpty(Phonetic.Nysiis("Watkins"));
        Assert.NotEmpty(Phonetic.Cologne("Müller"));
    }

    [Fact]
    public void Fuzzy_BestMatch_FindsClosest()
    {
        var best = Fuzzy.BestMatch("aple", new[] { "apple", "grape", "table" });
        Assert.NotNull(best);
        Assert.Equal("apple", best.Value.Value);
    }

    [Fact]
    public void Fuzzy_RankMatches_OrdersByScore()
    {
        var ranked = Fuzzy.RankMatches("apple", new[] { "grape", "apple", "apples" });
        Assert.Equal("apple", ranked[0].Value);
        Assert.True(ranked[0].Score >= ranked[1].Score);
    }

    [Fact]
    public void Fuzzy_Search_Filters()
    {
        var results = Fuzzy.Search("apple", new[] { "apple", "zzzzz" }, threshold: 0.6);
        Assert.Single(results);
        Assert.Equal("apple", results[0].Value);
    }
}
