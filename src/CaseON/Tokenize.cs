using System.Collections.Generic;
using System.Text.RegularExpressions;
using CaseON.Internal;

namespace CaseON;

/// <summary>
/// Splits text into words, sentences, lines, paragraphs and identifier components. Call as
/// <c>CaseON.Tokenize.Words(text)</c> or, after <c>using CaseON;</c>, simply <c>Tokenize.Words(text)</c>.
/// </summary>
/// <remarks>Methods return an empty array for <see langword="null"/> or empty input.</remarks>
public static partial class Tokenize
{
    [GeneratedRegex(@"[\p{L}\p{N}]+(?:['’][\p{L}]+)*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex WordRegex();

    [GeneratedRegex(@"[^.!?]+[.!?]+|\S+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SentenceRegex();

    /// <summary>Splits <paramref name="input"/> into words, ignoring punctuation and whitespace.</summary>
    /// <param name="input">The text to tokenize.</param>
    /// <returns>An array of words (possibly empty).</returns>
    public static string[] Words(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return [];
        }

        var matches = WordRegex().Matches(input);
        var result = new string[matches.Count];
        for (var i = 0; i < matches.Count; i++)
        {
            result[i] = matches[i].Value;
        }

        return result;
    }

    /// <summary>Splits <paramref name="input"/> into sentences.</summary>
    /// <param name="input">The text to tokenize.</param>
    /// <returns>An array of trimmed sentences (possibly empty).</returns>
    public static string[] Sentences(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        var matches = SentenceRegex().Matches(input);
        var result = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            var sentence = match.Value.Trim();
            if (sentence.Length > 0)
            {
                result.Add(sentence);
            }
        }

        return [.. result];
    }

    /// <summary>Splits <paramref name="input"/> into lines, handling <c>\n</c>, <c>\r</c> and <c>\r\n</c>.</summary>
    /// <param name="input">The text to tokenize.</param>
    /// <returns>An array of lines (possibly empty).</returns>
    public static string[] Lines(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return [];
        }

        return input.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
    }

    /// <summary>Splits <paramref name="input"/> into paragraphs separated by one or more blank lines.</summary>
    /// <param name="input">The text to tokenize.</param>
    /// <returns>An array of trimmed paragraphs (possibly empty).</returns>
    public static string[] Paragraphs(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        var normalized = Normalize.LineEndings(input, "\n");
        var blocks = normalized.Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>(blocks.Length);
        foreach (var block in blocks)
        {
            var trimmed = block.Trim();
            if (trimmed.Length > 0)
            {
                result.Add(trimmed);
            }
        }

        return [.. result];
    }

    /// <summary>Splits a <c>camelCase</c> identifier into its component words.</summary>
    /// <param name="input">The identifier to split.</param>
    /// <returns>An array of words (possibly empty).</returns>
    public static string[] Camel(string? input) => SplitIdentifier(input);

    /// <summary>Splits a <c>PascalCase</c> identifier into its component words.</summary>
    /// <param name="input">The identifier to split.</param>
    /// <returns>An array of words (possibly empty).</returns>
    public static string[] Pascal(string? input) => SplitIdentifier(input);

    /// <summary>Splits a <c>snake_case</c> identifier into its component words.</summary>
    /// <param name="input">The identifier to split.</param>
    /// <returns>An array of words (possibly empty).</returns>
    public static string[] Snake(string? input) =>
        string.IsNullOrEmpty(input) ? [] : input.Split('_', StringSplitOptions.RemoveEmptyEntries);

    /// <summary>Splits a <c>kebab-case</c> identifier into its component words.</summary>
    /// <param name="input">The identifier to split.</param>
    /// <returns>An array of words (possibly empty).</returns>
    public static string[] Kebab(string? input) =>
        string.IsNullOrEmpty(input) ? [] : input.Split('-', StringSplitOptions.RemoveEmptyEntries);

    private static string[] SplitIdentifier(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return [];
        }

        return [.. WordTokenizer.Split(input, preserveAcronyms: true)];
    }
}
