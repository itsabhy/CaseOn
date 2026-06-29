using System.Collections.Generic;

namespace CaseON.Internal;

/// <summary>
/// Internal helper that splits identifiers and phrases into their constituent words.
/// Boundaries are detected at separator characters (whitespace, <c>_</c>, <c>-</c>, <c>.</c>,
/// <c>/</c>, <c>\</c>) and at case transitions.
/// </summary>
internal static class WordTokenizer
{
    private static bool IsSeparator(char c) =>
        char.IsWhiteSpace(c) || c is '_' or '-' or '.' or '/' or '\\';

    /// <summary>
    /// Splits <paramref name="input"/> into words.
    /// </summary>
    /// <param name="input">The text to split.</param>
    /// <param name="preserveAcronyms">
    /// When <see langword="false"/> (default), every upper-case letter starts a new word
    /// (so <c>HTTPServer</c> becomes <c>H T T P Server</c>). When <see langword="true"/>,
    /// runs of upper-case letters are kept together as acronyms (so <c>HTTPServer</c>
    /// becomes <c>HTTP Server</c>).
    /// </param>
    /// <returns>The ordered list of non-empty words.</returns>
    public static List<string> Split(string input, bool preserveAcronyms = false)
    {
        var words = new List<string>();
        var start = 0;
        var length = input.Length;

        for (var i = 0; i < length; i++)
        {
            var c = input[i];

            if (IsSeparator(c))
            {
                if (i > start)
                {
                    words.Add(input.Substring(start, i - start));
                }

                start = i + 1;
                continue;
            }

            if (i > start && IsBoundary(input, i, preserveAcronyms))
            {
                words.Add(input.Substring(start, i - start));
                start = i;
            }
        }

        if (length > start)
        {
            words.Add(input.Substring(start, length - start));
        }

        return words;
    }

    private static bool IsBoundary(string input, int i, bool preserveAcronyms)
    {
        var prev = input[i - 1];
        var cur = input[i];

        if (!preserveAcronyms)
        {
            // Legacy behavior: start a new word before every upper-case letter.
            return char.IsUpper(cur);
        }

        // lower/digit -> upper  (e.g. "fooBar" -> "foo" | "Bar")
        if ((char.IsLower(prev) || char.IsDigit(prev)) && char.IsUpper(cur))
        {
            return true;
        }

        // end of an acronym run: UPPER followed by Upper+lower (e.g. "HTTPServer" -> "HTTP" | "Server")
        if (char.IsUpper(prev) && char.IsUpper(cur) && i + 1 < input.Length && char.IsLower(input[i + 1]))
        {
            return true;
        }

        // letter <-> digit transition
        if (char.IsLetter(prev) != char.IsLetter(cur) && (char.IsDigit(prev) || char.IsDigit(cur)))
        {
            return true;
        }

        return false;
    }
}
