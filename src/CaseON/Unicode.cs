using System.Globalization;
using CaseON.Internal;

namespace CaseON;

/// <summary>
/// Unicode inspection helpers: categories, emoji detection, combining marks, text direction and
/// surrogate pairs. Call as <c>CaseON.Unicode.IsEmoji("🙂")</c> or, after <c>using CaseON;</c>, simply
/// <c>Unicode.IsEmoji(...)</c>.
/// </summary>
public static class Unicode
{
    /// <summary>Gets the Unicode category of a character.</summary>
    /// <param name="value">The character to classify.</param>
    /// <returns>The <see cref="UnicodeCategory"/> of the character.</returns>
    public static UnicodeCategory Category(char value) => CharUnicodeInfo.GetUnicodeCategory(value);

    /// <summary>Gets the Unicode category of the character at <paramref name="index"/> (handling surrogate pairs).</summary>
    /// <param name="value">The string to inspect.</param>
    /// <param name="index">The index of the character or surrogate pair.</param>
    /// <returns>The <see cref="UnicodeCategory"/> at the position.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static UnicodeCategory Category(string value, int index)
    {
        ArgumentNullException.ThrowIfNull(value);
        return CharUnicodeInfo.GetUnicodeCategory(value, index);
    }

    /// <summary>Determines whether <paramref name="value"/> contains at least one emoji.</summary>
    /// <param name="value">The text to inspect.</param>
    /// <returns><see langword="true"/> if any emoji is present; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsEmoji(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        foreach (var rune in value.EnumerateRunes())
        {
            if (UnicodeInfo.IsEmoji(rune.Value))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Counts the combining (non-spacing and enclosing) marks in <paramref name="value"/>.</summary>
    /// <param name="value">The text to inspect.</param>
    /// <returns>The number of combining marks.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static int CombiningMarkCount(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var count = 0;
        foreach (var c in value)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category is UnicodeCategory.NonSpacingMark or UnicodeCategory.EnclosingMark or UnicodeCategory.SpacingCombiningMark)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Determines whether <paramref name="value"/> contains any right-to-left characters (e.g. Arabic or Hebrew).</summary>
    /// <param name="value">The text to inspect.</param>
    /// <returns><see langword="true"/> if a right-to-left character is present; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsRightToLeft(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        foreach (var rune in value.EnumerateRunes())
        {
            if (IsRtlCodePoint(rune.Value))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Counts the UTF-16 surrogate pairs in <paramref name="value"/> (characters outside the Basic Multilingual Plane).</summary>
    /// <param name="value">The text to inspect.</param>
    /// <returns>The number of surrogate pairs.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static int SurrogatePairCount(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var count = 0;
        for (var i = 0; i < value.Length - 1; i++)
        {
            if (char.IsSurrogatePair(value[i], value[i + 1]))
            {
                count++;
                i++;
            }
        }

        return count;
    }

    /// <summary>Counts the Unicode code points (runes) in <paramref name="value"/>, treating surrogate pairs as one.</summary>
    /// <param name="value">The text to inspect.</param>
    /// <returns>The number of code points.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static int CodePointCount(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var count = 0;
        foreach (var _ in value.EnumerateRunes())
        {
            count++;
        }

        return count;
    }

    private static bool IsRtlCodePoint(int codePoint) =>
        codePoint is (>= 0x0590 and <= 0x05FF)      // Hebrew
            or (>= 0x0600 and <= 0x06FF)             // Arabic
            or (>= 0x0700 and <= 0x074F)             // Syriac
            or (>= 0x0750 and <= 0x077F)             // Arabic Supplement
            or (>= 0x08A0 and <= 0x08FF)             // Arabic Extended-A
            or (>= 0xFB1D and <= 0xFB4F)             // Hebrew presentation forms
            or (>= 0xFB50 and <= 0xFDFF)             // Arabic presentation forms-A
            or (>= 0xFE70 and <= 0xFEFF);            // Arabic presentation forms-B
}
