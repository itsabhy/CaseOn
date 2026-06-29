using System.Globalization;
using System.Text;

namespace CaseON;

/// <summary>
/// Normalizes text: strips accents, collapses whitespace, unifies line endings, applies Unicode
/// normalization forms, converts "smart" punctuation to ASCII and folds Unicode digits to ASCII. Call as
/// <c>CaseON.Normalize.Accents(text)</c> or, after <c>using CaseON;</c>, simply <c>Normalize.Accents(text)</c>.
/// </summary>
/// <remarks>Methods accept any non-<see langword="null"/> input (including empty strings) and throw <see cref="ArgumentNullException"/> for <see langword="null"/>.</remarks>
public static class Normalize
{
    /// <summary>Removes diacritics (accents) from <paramref name="input"/>, e.g. <c>café</c> becomes <c>cafe</c>.</summary>
    /// <param name="input">The text to process.</param>
    /// <returns>The text with combining marks removed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Accents(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Length == 0)
        {
            return input;
        }

        var decomposed = input.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>Collapses all runs of whitespace in <paramref name="input"/> to single spaces and trims the ends.</summary>
    /// <param name="input">The text to process.</param>
    /// <returns>The whitespace-normalized text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Whitespace(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var builder = new StringBuilder(input.Length);
        var inWhitespace = false;
        foreach (var c in input)
        {
            if (char.IsWhiteSpace(c))
            {
                inWhitespace = true;
                continue;
            }

            if (inWhitespace && builder.Length > 0)
            {
                builder.Append(' ');
            }

            inWhitespace = false;
            builder.Append(c);
        }

        return builder.ToString();
    }

    /// <summary>Converts all line endings in <paramref name="input"/> to <paramref name="lineEnding"/>.</summary>
    /// <param name="input">The text to process.</param>
    /// <param name="lineEnding">The target line ending. Defaults to <c>\n</c>.</param>
    /// <returns>The text with unified line endings.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> or <paramref name="lineEnding"/> is <see langword="null"/>.</exception>
    public static string LineEndings(string input, string lineEnding = "\n")
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(lineEnding);

        var builder = new StringBuilder(input.Length);
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c == '\r')
            {
                builder.Append(lineEnding);
                if (i + 1 < input.Length && input[i + 1] == '\n')
                {
                    i++;
                }
            }
            else if (c == '\n')
            {
                builder.Append(lineEnding);
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    /// <summary>Applies a Unicode normalization form to <paramref name="input"/>.</summary>
    /// <param name="input">The text to process.</param>
    /// <param name="form">The normalization form. Defaults to <see cref="NormalizationForm.FormC"/>.</param>
    /// <returns>The normalized text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Unicode(string input, NormalizationForm form = NormalizationForm.FormC)
    {
        ArgumentNullException.ThrowIfNull(input);
        return input.Length == 0 ? input : input.Normalize(form);
    }

    /// <summary>Converts "smart" punctuation (curly quotes, en/em dashes, ellipsis) in <paramref name="input"/> to plain ASCII.</summary>
    /// <param name="input">The text to process.</param>
    /// <returns>The text with punctuation normalized to ASCII.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Punctuation(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var builder = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            switch (c)
            {
                case '\u2018' or '\u2019' or '\u201A' or '\u2032':
                    builder.Append('\'');
                    break;
                case '\u201C' or '\u201D' or '\u201E' or '\u2033':
                    builder.Append('"');
                    break;
                case '\u2013' or '\u2014' or '\u2212':
                    builder.Append('-');
                    break;
                case '\u2026':
                    builder.Append("...");
                    break;
                case '\u00A0':
                    builder.Append(' ');
                    break;
                default:
                    builder.Append(c);
                    break;
            }
        }

        return builder.ToString();
    }

    /// <summary>Folds Unicode decimal digits (e.g. Arabic-Indic) in <paramref name="input"/> to ASCII <c>0-9</c>.</summary>
    /// <param name="input">The text to process.</param>
    /// <returns>The text with digits normalized to ASCII.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Digits(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var builder = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (char.IsDigit(c) && !char.IsAsciiDigit(c))
            {
                var value = CharUnicodeInfo.GetDigitValue(c);
                builder.Append(value >= 0 ? (char)('0' + value) : c);
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
