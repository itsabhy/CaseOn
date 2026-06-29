using System.Text;
using System.Text.RegularExpressions;

namespace CaseON;

/// <summary>
/// Removes unwanted characters from text: emoji, HTML tags, diacritics, punctuation, control characters,
/// digits, letters, special characters and excess whitespace. Call as <c>CaseON.Clean.RemoveHtml(text)</c>
/// or, after <c>using CaseON;</c>, simply <c>Clean.RemoveHtml(text)</c>.
/// </summary>
/// <remarks>Methods accept any non-<see langword="null"/> input (including empty strings) and throw <see cref="ArgumentNullException"/> for <see langword="null"/>.</remarks>
public static partial class Clean
{
    [GeneratedRegex(@"<[^>]*>", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HtmlTagRegex();

    /// <summary>Removes emoji and pictographic symbols from <paramref name="input"/>.</summary>
    /// <param name="input">The text to clean.</param>
    /// <returns>The text without emoji.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string RemoveEmoji(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var builder = new StringBuilder(input.Length);
        foreach (var rune in input.EnumerateRunes())
        {
            if (!Internal.UnicodeInfo.IsEmoji(rune.Value))
            {
                builder.Append(rune.ToString());
            }
        }

        return builder.ToString();
    }

    /// <summary>Removes HTML tags from <paramref name="input"/> and decodes HTML entities.</summary>
    /// <param name="input">The text to clean.</param>
    /// <returns>The plain-text content.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string RemoveHtml(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Length == 0)
        {
            return input;
        }

        string stripped;
        try
        {
            stripped = HtmlTagRegex().Replace(input, string.Empty);
        }
        catch (RegexMatchTimeoutException)
        {
            stripped = input;
        }

        return System.Net.WebUtility.HtmlDecode(stripped);
    }

    /// <summary>Removes diacritics (accents) from <paramref name="input"/>.</summary>
    /// <param name="input">The text to clean.</param>
    /// <returns>The text without combining marks.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string RemoveDiacritics(string input) => Normalize.Accents(input);

    /// <summary>Removes punctuation characters from <paramref name="input"/>.</summary>
    /// <param name="input">The text to clean.</param>
    /// <returns>The text without punctuation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string RemovePunctuation(string input) =>
        RemoveWhere(input, static c => char.IsPunctuation(c) || char.IsSymbol(c));

    /// <summary>Removes control characters from <paramref name="input"/>.</summary>
    /// <param name="input">The text to clean.</param>
    /// <returns>The text without control characters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string RemoveControlCharacters(string input) =>
        RemoveWhere(input, static c => char.IsControl(c) && !char.IsWhiteSpace(c));

    /// <summary>Removes all digits from <paramref name="input"/>.</summary>
    /// <param name="input">The text to clean.</param>
    /// <returns>The text without digits.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string RemoveNumbers(string input) => RemoveWhere(input, char.IsDigit);

    /// <summary>Removes all letters from <paramref name="input"/>.</summary>
    /// <param name="input">The text to clean.</param>
    /// <returns>The text without letters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string RemoveLetters(string input) => RemoveWhere(input, char.IsLetter);

    /// <summary>Removes characters that are neither letters, digits nor whitespace from <paramref name="input"/>.</summary>
    /// <param name="input">The text to clean.</param>
    /// <returns>The text with special characters removed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string RemoveSpecialCharacters(string input) =>
        RemoveWhere(input, static c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

    /// <summary>Removes all whitespace from <paramref name="input"/>.</summary>
    /// <param name="input">The text to clean.</param>
    /// <returns>The text without whitespace.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string RemoveWhitespace(string input) => RemoveWhere(input, char.IsWhiteSpace);

    /// <summary>Collapses consecutive whitespace in <paramref name="input"/> to single spaces and trims the ends.</summary>
    /// <param name="input">The text to clean.</param>
    /// <returns>The text with duplicate spaces collapsed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string CollapseWhitespace(string input) => Normalize.Whitespace(input);

    private static string RemoveWhere(string input, Func<char, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(input);

        var builder = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (!predicate(c))
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
