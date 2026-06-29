using System.Collections.Generic;
using System.Text;

namespace CaseON;

/// <summary>
/// Transliterates text to ASCII. Call as <c>CaseON.Transliterate.ToAscii("Crème brûlée")</c> or, after
/// <c>using CaseON;</c>, simply <c>Transliterate.ToAscii(...)</c>.
/// </summary>
/// <remarks>
/// Diacritics are removed via Unicode decomposition, and a curated table maps common non-decomposable
/// Latin letters, ligatures, punctuation and symbols (e.g. <c>ß</c> -&gt; <c>ss</c>, <c>æ</c> -&gt; <c>ae</c>,
/// <c>€</c> -&gt; <c>EUR</c>). The focus is Latin-script text; characters with no mapping are replaced with
/// the configured fallback.
/// </remarks>
public static class Transliterate
{
    private static readonly Dictionary<char, string> Map = new()
    {
        ['ß'] = "ss", ['æ'] = "ae", ['Æ'] = "AE", ['œ'] = "oe", ['Œ'] = "OE",
        ['ø'] = "o", ['Ø'] = "O", ['đ'] = "d", ['Đ'] = "D", ['ð'] = "d", ['Ð'] = "D",
        ['ł'] = "l", ['Ł'] = "L", ['þ'] = "th", ['Þ'] = "Th", ['ħ'] = "h", ['Ħ'] = "H",
        ['ı'] = "i", ['İ'] = "I", ['ĸ'] = "k", ['ŉ'] = "n", ['ƒ'] = "f",
        ['–'] = "-", ['—'] = "-", ['‐'] = "-", ['‑'] = "-",
        ['“'] = "\"", ['”'] = "\"", ['„'] = "\"", ['‘'] = "'", ['’'] = "'", ['‚'] = "'",
        ['…'] = "...", ['•'] = "*", ['·'] = ".", ['«'] = "<<", ['»'] = ">>",
        ['€'] = "EUR", ['£'] = "GBP", ['¥'] = "JPY", ['¢'] = "c", ['¤'] = "$",
        ['©'] = "(c)", ['®'] = "(r)", ['™'] = "(tm)", ['°'] = "deg", ['±'] = "+/-",
        ['×'] = "x", ['÷'] = "/", ['№'] = "No", ['§'] = "S", ['¶'] = "P",
        ['\u00A0'] = " ",
    };

    /// <summary>Transliterates <paramref name="input"/> to ASCII.</summary>
    /// <param name="input">The text to transliterate.</param>
    /// <param name="unknownReplacement">The string substituted for non-ASCII characters that have no mapping. Defaults to an empty string (drop).</param>
    /// <returns>An ASCII representation of <paramref name="input"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> or <paramref name="unknownReplacement"/> is <see langword="null"/>.</exception>
    /// <example><code>Transliterate.ToAscii("Crème brûlée"); // "Creme brulee"</code></example>
    public static string ToAscii(string input, string unknownReplacement = "")
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(unknownReplacement);
        if (input.Length == 0)
        {
            return input;
        }

        // Decompose so that combining diacritics can be dropped, then re-process.
        var decomposed = input.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(input.Length);

        foreach (var c in decomposed)
        {
            if (char.IsAscii(c))
            {
                builder.Append(c);
                continue;
            }

            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (Map.TryGetValue(c, out var mapped))
            {
                builder.Append(mapped);
            }
            else
            {
                builder.Append(unknownReplacement);
            }
        }

        return builder.ToString();
    }
}
