using System.Collections.Generic;

namespace CaseON;

/// <summary>
/// Computes readability and content metrics for text. Call as <c>CaseON.Metrics.WordCount(text)</c>
/// or, after <c>using CaseON;</c>, simply <c>Metrics.WordCount(text)</c>.
/// </summary>
/// <remarks>Methods treat <see langword="null"/> or empty input as having zero content.</remarks>
public static class Metrics
{
    /// <summary>Counts the words in <paramref name="input"/>.</summary>
    /// <param name="input">The text to measure.</param>
    /// <returns>The number of words.</returns>
    public static int WordCount(string? input) => Tokenize.Words(input).Length;

    /// <summary>Counts the sentences in <paramref name="input"/>.</summary>
    /// <param name="input">The text to measure.</param>
    /// <returns>The number of sentences.</returns>
    public static int SentenceCount(string? input) => Tokenize.Sentences(input).Length;

    /// <summary>Counts the paragraphs in <paramref name="input"/>.</summary>
    /// <param name="input">The text to measure.</param>
    /// <returns>The number of paragraphs.</returns>
    public static int ParagraphCount(string? input) => Tokenize.Paragraphs(input).Length;

    /// <summary>Counts the lines in <paramref name="input"/>.</summary>
    /// <param name="input">The text to measure.</param>
    /// <returns>The number of lines.</returns>
    public static int LineCount(string? input) => Tokenize.Lines(input).Length;

    /// <summary>Counts the distinct words in <paramref name="input"/>.</summary>
    /// <param name="input">The text to measure.</param>
    /// <param name="ignoreCase">When <see langword="true"/> (default), words differing only in case are treated as equal.</param>
    /// <returns>The number of unique words.</returns>
    public static int UniqueWords(string? input, bool ignoreCase = true)
    {
        var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        return new HashSet<string>(Tokenize.Words(input), comparer).Count;
    }

    /// <summary>Estimates the reading time for <paramref name="input"/>.</summary>
    /// <param name="input">The text to measure.</param>
    /// <param name="wordsPerMinute">The assumed reading speed. Defaults to 200 words per minute.</param>
    /// <returns>The estimated reading time.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="wordsPerMinute"/> is not positive.</exception>
    public static TimeSpan ReadingTime(string? input, int wordsPerMinute = 200)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(wordsPerMinute);
        var words = WordCount(input);
        return TimeSpan.FromMinutes((double)words / wordsPerMinute);
    }

    /// <summary>Computes the frequency of each character in <paramref name="input"/>.</summary>
    /// <param name="input">The text to measure.</param>
    /// <returns>A dictionary mapping each character to its occurrence count.</returns>
    public static IReadOnlyDictionary<char, int> CharacterFrequency(string? input)
    {
        var frequency = new Dictionary<char, int>();
        if (string.IsNullOrEmpty(input))
        {
            return frequency;
        }

        foreach (var c in input)
        {
            frequency[c] = frequency.TryGetValue(c, out var count) ? count + 1 : 1;
        }

        return frequency;
    }

    /// <summary>Computes the Shannon entropy of <paramref name="input"/> in bits per character.</summary>
    /// <param name="input">The text to measure.</param>
    /// <returns>The entropy in bits per character; <c>0</c> for empty input.</returns>
    public static double Entropy(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return 0;
        }

        var frequency = CharacterFrequency(input);
        double entropy = 0;
        double length = input.Length;
        foreach (var count in frequency.Values)
        {
            var probability = count / length;
            entropy -= probability * Math.Log2(probability);
        }

        return entropy;
    }
}
