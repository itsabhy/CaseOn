using System.Collections.Generic;
using System.Linq;
using CaseON.Internal;

namespace CaseON;

/// <summary>
/// Searches and tests text for words, phrases, patterns and character classes. Call as
/// <c>CaseON.Search.ContainsWord(text, "cat")</c> or, after <c>using CaseON;</c>, simply
/// <c>Search.ContainsWord(...)</c>.
/// </summary>
public static class Search
{
    /// <summary>Determines whether <paramref name="text"/> contains <paramref name="word"/> as a whole word.</summary>
    /// <param name="text">The text to search.</param>
    /// <param name="word">The whole word to find.</param>
    /// <param name="ignoreCase">When <see langword="true"/>, the match ignores case.</param>
    /// <returns><see langword="true"/> if the word is found on word boundaries; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="word"/> is <see langword="null"/>.</exception>
    public static bool ContainsWord(string text, string word, bool ignoreCase = false)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(word);
        if (word.Length == 0)
        {
            return false;
        }

        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var index = 0;
        while ((index = text.IndexOf(word, index, comparison)) >= 0)
        {
            var before = index == 0 || !IsWordChar(text[index - 1]);
            var afterIndex = index + word.Length;
            var after = afterIndex == text.Length || !IsWordChar(text[afterIndex]);
            if (before && after)
            {
                return true;
            }

            index += 1;
        }

        return false;
    }

    /// <summary>Determines whether <paramref name="text"/> contains the given <paramref name="phrase"/>.</summary>
    /// <param name="text">The text to search.</param>
    /// <param name="phrase">The phrase to find.</param>
    /// <param name="ignoreCase">When <see langword="true"/>, the match ignores case.</param>
    /// <returns><see langword="true"/> if the phrase is found; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="phrase"/> is <see langword="null"/>.</exception>
    public static bool ContainsPhrase(string text, string phrase, bool ignoreCase = false)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(phrase);
        return text.Contains(phrase, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    /// <summary>Determines whether <paramref name="text"/> matches the regular expression <paramref name="pattern"/>.</summary>
    /// <param name="text">The text to test.</param>
    /// <param name="pattern">The regular-expression pattern. Evaluated with a one-second timeout.</param>
    /// <returns><see langword="true"/> if the pattern matches; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public static bool IsMatch(string text, string pattern)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(pattern);
        return RegexCache.IsMatch(text, pattern);
    }

    /// <summary>Determines whether <paramref name="text"/> contains any of the given <paramref name="values"/>.</summary>
    /// <param name="text">The text to search.</param>
    /// <param name="values">The candidate substrings.</param>
    /// <returns><see langword="true"/> if at least one value is present; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="values"/> is <see langword="null"/>.</exception>
    public static bool ContainsAny(string text, params string[] values)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(values);
        return values.Any(v => v is not null && text.Contains(v, StringComparison.Ordinal));
    }

    /// <summary>Determines whether <paramref name="text"/> contains all of the given <paramref name="values"/>.</summary>
    /// <param name="text">The text to search.</param>
    /// <param name="values">The required substrings.</param>
    /// <returns><see langword="true"/> if every value is present; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="values"/> is <see langword="null"/>.</exception>
    public static bool ContainsAll(string text, params string[] values)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(values);
        return values.All(v => v is not null && text.Contains(v, StringComparison.Ordinal));
    }

    /// <summary>Determines whether every character of <paramref name="text"/> is contained in <paramref name="allowed"/>.</summary>
    /// <param name="text">The text to test.</param>
    /// <param name="allowed">The set of permitted characters.</param>
    /// <returns><see langword="true"/> if all characters are allowed; otherwise <see langword="false"/>. Empty text returns <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="allowed"/> is <see langword="null"/>.</exception>
    public static bool ContainsOnly(string text, string allowed)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(allowed);
        if (text.Length == 0)
        {
            return false;
        }

        var set = new HashSet<char>(allowed);
        foreach (var c in text)
        {
            if (!set.Contains(c))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Determines whether <paramref name="text"/> consists solely of letters.</summary>
    /// <param name="text">The text to test.</param>
    /// <returns><see langword="true"/> if every character is a letter; otherwise <see langword="false"/>. Empty text returns <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    public static bool IsLetters(string text) => AllChars(text, char.IsLetter);

    /// <summary>Determines whether <paramref name="text"/> consists solely of digits.</summary>
    /// <param name="text">The text to test.</param>
    /// <returns><see langword="true"/> if every character is a digit; otherwise <see langword="false"/>. Empty text returns <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    public static bool IsDigits(string text) => AllChars(text, char.IsDigit);

    /// <summary>Determines whether <paramref name="text"/> consists solely of symbols and punctuation.</summary>
    /// <param name="text">The text to test.</param>
    /// <returns><see langword="true"/> if every character is a symbol or punctuation; otherwise <see langword="false"/>. Empty text returns <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    public static bool IsSymbols(string text) => AllChars(text, static c => char.IsSymbol(c) || char.IsPunctuation(c));

    private static bool AllChars(string text, Func<char, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length == 0)
        {
            return false;
        }

        foreach (var c in text)
        {
            if (!predicate(c))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';
}
