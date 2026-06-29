namespace CaseON;

/// <summary>Aggregate statistics describing a piece of text.</summary>
/// <param name="Characters">The total number of characters.</param>
/// <param name="Letters">The number of letter characters.</param>
/// <param name="Digits">The number of digit characters.</param>
/// <param name="Whitespace">The number of white-space characters.</param>
/// <param name="Words">The number of words.</param>
/// <param name="UniqueWords">The number of distinct words (case-insensitive).</param>
/// <param name="Sentences">The number of sentences.</param>
/// <param name="Paragraphs">The number of paragraphs.</param>
/// <param name="Lines">The number of lines.</param>
/// <param name="AverageWordLength">The mean word length in characters.</param>
/// <param name="ReadingTime">The estimated reading time at 200 words per minute.</param>
public sealed record TextStats(
    int Characters,
    int Letters,
    int Digits,
    int Whitespace,
    int Words,
    int UniqueWords,
    int Sentences,
    int Paragraphs,
    int Lines,
    double AverageWordLength,
    TimeSpan ReadingTime);

/// <summary>
/// Produces an aggregate <see cref="TextStats"/> summary for text. Call as <c>CaseON.Analyze.Text(input)</c>
/// or, after <c>using CaseON;</c>, simply <c>Analyze.Text(input)</c>.
/// </summary>
public static class Analyze
{
    /// <summary>Computes aggregate statistics for <paramref name="input"/>.</summary>
    /// <param name="input">The text to analyze. <see langword="null"/> is treated as empty.</param>
    /// <returns>A <see cref="TextStats"/> describing the text.</returns>
    public static TextStats Text(string? input)
    {
        input ??= string.Empty;

        var letters = 0;
        var digits = 0;
        var whitespace = 0;
        foreach (var c in input)
        {
            if (char.IsLetter(c))
            {
                letters++;
            }
            else if (char.IsDigit(c))
            {
                digits++;
            }
            else if (char.IsWhiteSpace(c))
            {
                whitespace++;
            }
        }

        var words = Tokenize.Words(input);
        var totalWordLength = 0;
        foreach (var word in words)
        {
            totalWordLength += word.Length;
        }

        var averageWordLength = words.Length == 0 ? 0 : (double)totalWordLength / words.Length;

        return new TextStats(
            Characters: input.Length,
            Letters: letters,
            Digits: digits,
            Whitespace: whitespace,
            Words: words.Length,
            UniqueWords: Metrics.UniqueWords(input),
            Sentences: Metrics.SentenceCount(input),
            Paragraphs: Metrics.ParagraphCount(input),
            Lines: string.IsNullOrEmpty(input) ? 0 : Metrics.LineCount(input),
            AverageWordLength: averageWordLength,
            ReadingTime: Metrics.ReadingTime(input));
    }
}
