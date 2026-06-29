using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CaseON;

/// <summary>
/// Detects and censors profanity using a built-in English word list (overridable). Call as
/// <c>CaseON.Profanity.Contains(text)</c> or, after <c>using CaseON;</c>, simply
/// <c>Profanity.Contains(text)</c>.
/// </summary>
/// <remarks>
/// Matching is whole-word and case-insensitive. The built-in list is intentionally small; supply a
/// custom list for production moderation. This is a heuristic filter and is not a substitute for human
/// moderation.
/// </remarks>
public static partial class Profanity
{
    private static readonly HashSet<string> DefaultWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "arse", "ass", "asshole", "bastard", "bitch", "bollocks", "bugger", "crap",
        "damn", "dick", "dickhead", "douche", "fuck", "fucker", "fucking", "shit",
        "shite", "slut", "twat", "wanker", "prick", "piss", "cock", "cunt", "whore",
    };

    [GeneratedRegex(@"[\p{L}]+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex WordRegex();

    /// <summary>Determines whether <paramref name="text"/> contains a profane word from the built-in list.</summary>
    /// <param name="text">The text to scan.</param>
    /// <returns><see langword="true"/> if profanity is found; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    public static bool Contains(string text) => Contains(text, DefaultWords);

    /// <summary>Determines whether <paramref name="text"/> contains a word from a custom <paramref name="words"/> list.</summary>
    /// <param name="text">The text to scan.</param>
    /// <param name="words">The blocked words.</param>
    /// <returns><see langword="true"/> if a blocked word is found; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="words"/> is <see langword="null"/>.</exception>
    public static bool Contains(string text, IEnumerable<string> words)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(words);
        if (text.Length == 0)
        {
            return false;
        }

        var set = AsSet(words);
        foreach (Match match in WordRegex().Matches(text))
        {
            if (set.Contains(match.Value))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Finds all distinct profane words in <paramref name="text"/> using the built-in list.</summary>
    /// <param name="text">The text to scan.</param>
    /// <returns>An array of matched words, lower-cased and de-duplicated (possibly empty).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    public static string[] Find(string text) => Find(text, DefaultWords);

    /// <summary>Finds all distinct words from a custom <paramref name="words"/> list in <paramref name="text"/>.</summary>
    /// <param name="text">The text to scan.</param>
    /// <param name="words">The blocked words.</param>
    /// <returns>An array of matched words, lower-cased and de-duplicated (possibly empty).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="words"/> is <see langword="null"/>.</exception>
    public static string[] Find(string text, IEnumerable<string> words)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(words);
        if (text.Length == 0)
        {
            return [];
        }

        var set = AsSet(words);
        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in WordRegex().Matches(text))
        {
            if (set.Contains(match.Value))
            {
                found.Add(match.Value.ToLowerInvariant());
            }
        }

        return [.. found];
    }

    /// <summary>Censors profane words in <paramref name="text"/> using the built-in list.</summary>
    /// <param name="text">The text to censor.</param>
    /// <param name="maskChar">The masking character. Defaults to <c>*</c>.</param>
    /// <returns>The censored text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    public static string Censor(string text, char maskChar = '*') => Censor(text, DefaultWords, maskChar);

    /// <summary>Censors words from a custom <paramref name="words"/> list in <paramref name="text"/>.</summary>
    /// <param name="text">The text to censor.</param>
    /// <param name="words">The blocked words.</param>
    /// <param name="maskChar">The masking character. Defaults to <c>*</c>.</param>
    /// <returns>The censored text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="words"/> is <see langword="null"/>.</exception>
    public static string Censor(string text, IEnumerable<string> words, char maskChar = '*')
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(words);
        if (text.Length == 0)
        {
            return text;
        }

        var set = AsSet(words);
        var builder = new StringBuilder(text.Length);
        var lastIndex = 0;

        foreach (Match match in WordRegex().Matches(text))
        {
            if (!set.Contains(match.Value))
            {
                continue;
            }

            builder.Append(text, lastIndex, match.Index - lastIndex);
            builder.Append(maskChar, match.Length);
            lastIndex = match.Index + match.Length;
        }

        builder.Append(text, lastIndex, text.Length - lastIndex);
        return builder.ToString();
    }

    private static HashSet<string> AsSet(IEnumerable<string> words) =>
        ReferenceEquals(words, DefaultWords)
            ? DefaultWords
            : new HashSet<string>(words, StringComparer.OrdinalIgnoreCase);
}
