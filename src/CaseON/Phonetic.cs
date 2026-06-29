using System.Text;

namespace CaseON;

/// <summary>
/// Phonetic encoding algorithms for "sounds-like" matching. Call as <c>CaseON.Phonetic.Soundex("Robert")</c>
/// or, after <c>using CaseON;</c>, simply <c>Phonetic.Soundex("Robert")</c>.
/// </summary>
/// <remarks>
/// These algorithms reduce a word to a code so that similarly pronounced words share the same code.
/// Soundex, Metaphone and NYSIIS target English; Cologne (Kölner Phonetik) targets German. Encoders
/// throw <see cref="ArgumentException"/> for <see langword="null"/>, empty or white-space input.
/// </remarks>
public static class Phonetic
{
    /// <summary>Computes the four-character American Soundex code for <paramref name="word"/>.</summary>
    /// <param name="word">The word to encode.</param>
    /// <returns>A Soundex code such as <c>R163</c>.</returns>
    /// <exception cref="ArgumentException"><paramref name="word"/> is null, empty or white-space.</exception>
    /// <example><code>Phonetic.Soundex("Robert"); // "R163"</code></example>
    public static string Soundex(string word)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(word);

        var builder = new StringBuilder(4);
        var previousCode = '0';
        foreach (var raw in word)
        {
            if (!char.IsLetter(raw))
            {
                continue;
            }

            var c = char.ToUpperInvariant(raw);
            if (builder.Length == 0)
            {
                builder.Append(c);
                previousCode = SoundexCode(c);
                continue;
            }

            var code = SoundexCode(c);
            if (code != '0' && code != previousCode)
            {
                builder.Append(code);
                if (builder.Length == 4)
                {
                    break;
                }
            }

            // 'H' and 'W' do not reset the previous code; vowels do.
            if (c != 'H' && c != 'W')
            {
                previousCode = code;
            }
        }

        if (builder.Length == 0)
        {
            throw new ArgumentException("Input contains no letters.", nameof(word));
        }

        return builder.Append('0', 4 - builder.Length).ToString();
    }

    /// <summary>Determines whether two words share the same Soundex code.</summary>
    /// <param name="first">The first word.</param>
    /// <param name="second">The second word.</param>
    /// <returns><see langword="true"/> if the words sound alike under Soundex; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Either argument is null, empty or white-space.</exception>
    public static bool SoundexEquals(string first, string second) =>
        string.Equals(Soundex(first), Soundex(second), StringComparison.Ordinal);

    /// <summary>Computes the Metaphone code for <paramref name="word"/>.</summary>
    /// <param name="word">The word to encode.</param>
    /// <param name="maxLength">The maximum length of the returned code. Defaults to 4.</param>
    /// <returns>A Metaphone code such as <c>0MSN</c> for "Thompson".</returns>
    /// <exception cref="ArgumentException"><paramref name="word"/> is null, empty or white-space.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is not positive.</exception>
    public static string Metaphone(string word, int maxLength = 4)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(word);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);

        var letters = new StringBuilder(word.Length);
        foreach (var c in word)
        {
            if (char.IsLetter(c))
            {
                letters.Append(char.ToUpperInvariant(c));
            }
        }

        if (letters.Length == 0)
        {
            throw new ArgumentException("Input contains no letters.", nameof(word));
        }

        var s = letters.ToString();
        var n = s.Length;
        var result = new StringBuilder(maxLength);
        var i = 0;

        // Handle initial-letter exceptions.
        if (n >= 2)
        {
            var start = s[..2];
            if (start is "AE" or "GN" or "KN" or "PN" or "WR")
            {
                i = 1;
            }
            else if (s[0] == 'X')
            {
                result.Append('S');
                i = 1;
            }
            else if (start == "WH")
            {
                result.Append('W');
                i = 2;
            }
        }

        char At(int index) => index >= 0 && index < n ? s[index] : '\0';
        bool IsVowel(char c) => c is 'A' or 'E' or 'I' or 'O' or 'U';

        while (i < n && result.Length < maxLength)
        {
            var c = s[i];

            // Skip duplicate letters except 'C'.
            if (c != 'C' && i > 0 && s[i - 1] == c)
            {
                i++;
                continue;
            }

            switch (c)
            {
                case 'A' or 'E' or 'I' or 'O' or 'U':
                    if (i == 0)
                    {
                        result.Append(c);
                    }

                    break;
                case 'B':
                    if (!(i == n - 1 && At(i - 1) == 'M'))
                    {
                        result.Append('B');
                    }

                    break;
                case 'C':
                    if (At(i + 1) == 'I' && At(i + 2) == 'A')
                    {
                        result.Append('X');
                    }
                    else if (At(i + 1) == 'H')
                    {
                        result.Append(At(i - 1) == 'S' ? 'K' : 'X');
                        i++;
                    }
                    else if (At(i + 1) is 'I' or 'E' or 'Y')
                    {
                        if (At(i - 1) != 'S')
                        {
                            result.Append('S');
                        }
                    }
                    else
                    {
                        result.Append('K');
                    }

                    break;
                case 'D':
                    if (At(i + 1) == 'G' && At(i + 2) is 'E' or 'I' or 'Y')
                    {
                        result.Append('J');
                        i += 2;
                    }
                    else
                    {
                        result.Append('T');
                    }

                    break;
                case 'G':
                    if (At(i + 1) == 'H')
                    {
                        if (!(i > 0 && IsVowel(At(i - 1))))
                        {
                            // silent in many words; approximate by skipping
                        }
                        else
                        {
                            result.Append('K');
                        }

                        i++;
                    }
                    else if (At(i + 1) == 'N')
                    {
                        // silent
                    }
                    else if (At(i + 1) is 'I' or 'E' or 'Y')
                    {
                        result.Append('J');
                    }
                    else
                    {
                        result.Append('K');
                    }

                    break;
                case 'H':
                    if (IsVowel(At(i - 1)) && !IsVowel(At(i + 1)))
                    {
                        // silent
                    }
                    else if (At(i - 1) is 'C' or 'S' or 'P' or 'T' or 'G')
                    {
                        // handled by preceding consonant
                    }
                    else
                    {
                        result.Append('H');
                    }

                    break;
                case 'J':
                    result.Append('J');
                    break;
                case 'K':
                    if (At(i - 1) != 'C')
                    {
                        result.Append('K');
                    }

                    break;
                case 'L':
                    result.Append('L');
                    break;
                case 'M':
                    result.Append('M');
                    break;
                case 'N':
                    result.Append('N');
                    break;
                case 'P':
                    result.Append(At(i + 1) == 'H' ? 'F' : 'P');
                    if (At(i + 1) == 'H')
                    {
                        i++;
                    }

                    break;
                case 'Q':
                    result.Append('K');
                    break;
                case 'R':
                    result.Append('R');
                    break;
                case 'S':
                    if (At(i + 1) == 'H')
                    {
                        result.Append('X');
                        i++;
                    }
                    else if (At(i + 1) == 'I' && At(i + 2) is 'O' or 'A')
                    {
                        result.Append('X');
                    }
                    else
                    {
                        result.Append('S');
                    }

                    break;
                case 'T':
                    if (At(i + 1) == 'H')
                    {
                        result.Append('0');
                        i++;
                    }
                    else if (At(i + 1) == 'I' && At(i + 2) is 'O' or 'A')
                    {
                        result.Append('X');
                    }
                    else
                    {
                        result.Append('T');
                    }

                    break;
                case 'V':
                    result.Append('F');
                    break;
                case 'W' or 'Y':
                    if (IsVowel(At(i + 1)))
                    {
                        result.Append(c);
                    }

                    break;
                case 'X':
                    result.Append("KS");
                    break;
                case 'Z':
                    result.Append('S');
                    break;
                default:
                    break;
            }

            i++;
        }

        return result.Length > maxLength ? result.ToString(0, maxLength) : result.ToString();
    }

    /// <summary>Determines whether two words share the same Metaphone code.</summary>
    /// <param name="first">The first word.</param>
    /// <param name="second">The second word.</param>
    /// <returns><see langword="true"/> if the words sound alike under Metaphone; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Either argument is null, empty or white-space.</exception>
    public static bool MetaphoneEquals(string first, string second) =>
        string.Equals(Metaphone(first), Metaphone(second), StringComparison.Ordinal);

    /// <summary>Computes the NYSIIS (New York State Identification and Intelligence System) code for <paramref name="word"/>.</summary>
    /// <param name="word">The word to encode.</param>
    /// <returns>A NYSIIS phonetic code.</returns>
    /// <exception cref="ArgumentException"><paramref name="word"/> is null, empty or white-space.</exception>
    public static string Nysiis(string word)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(word);

        var sb = new StringBuilder();
        foreach (var c in word)
        {
            if (char.IsLetter(c))
            {
                sb.Append(char.ToUpperInvariant(c));
            }
        }

        if (sb.Length == 0)
        {
            throw new ArgumentException("Input contains no letters.", nameof(word));
        }

        var s = sb.ToString();

        s = ReplaceStart(s, "MAC", "MCC");
        s = ReplaceStart(s, "KN", "NN");
        s = ReplaceStart(s, "K", "C");
        s = ReplaceStart(s, "PH", "FF");
        s = ReplaceStart(s, "PF", "FF");
        s = ReplaceStart(s, "SCH", "SSS");

        s = ReplaceEnd(s, "EE", "Y");
        s = ReplaceEnd(s, "IE", "Y");
        s = ReplaceEnd(s, "DT", "D");
        s = ReplaceEnd(s, "RT", "D");
        s = ReplaceEnd(s, "RD", "D");
        s = ReplaceEnd(s, "NT", "D");
        s = ReplaceEnd(s, "ND", "D");

        var key = new StringBuilder();
        key.Append(s[0]);

        for (var i = 1; i < s.Length; i++)
        {
            var c = s[i];
            var prev = s[i - 1];
            var next = i + 1 < s.Length ? s[i + 1] : '\0';

            string mapped;
            switch (c)
            {
                case 'E' when next == 'V':
                    mapped = "AF";
                    i++;
                    break;
                case 'A' or 'E' or 'I' or 'O' or 'U':
                    mapped = "A";
                    break;
                case 'Q':
                    mapped = "G";
                    break;
                case 'Z':
                    mapped = "S";
                    break;
                case 'M':
                    mapped = "N";
                    break;
                case 'K':
                    mapped = next == 'N' ? "N" : "C";
                    break;
                case 'S' when next == 'C' && i + 2 < s.Length && s[i + 2] == 'H':
                    mapped = "SSS";
                    i += 2;
                    break;
                case 'P' when next == 'H':
                    mapped = "FF";
                    i++;
                    break;
                case 'H':
                    mapped = IsNysiisVowel(prev) && (next == '\0' || !IsNysiisVowel(next)) ? prev.ToString() : "H";
                    break;
                case 'W' when IsNysiisVowel(prev):
                    mapped = prev.ToString();
                    break;
                default:
                    mapped = c.ToString();
                    break;
            }

            if (mapped.Length > 0 && (key.Length == 0 || key[^1] != mapped[0]))
            {
                key.Append(mapped);
            }
        }

        // Trim trailing 'S', 'A' and 'AY' per the algorithm.
        var result = key.ToString();
        if (result.EndsWith('S') && result.Length > 1)
        {
            result = result[..^1];
        }

        if (result.EndsWith("AY", StringComparison.Ordinal))
        {
            result = result[..^2] + "Y";
        }

        if (result.EndsWith('A') && result.Length > 1)
        {
            result = result[..^1];
        }

        return result;
    }

    /// <summary>Determines whether two words share the same NYSIIS code.</summary>
    /// <param name="first">The first word.</param>
    /// <param name="second">The second word.</param>
    /// <returns><see langword="true"/> if the words sound alike under NYSIIS; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Either argument is null, empty or white-space.</exception>
    public static bool NysiisEquals(string first, string second) =>
        string.Equals(Nysiis(first), Nysiis(second), StringComparison.Ordinal);

    /// <summary>Computes the Cologne phonetics (Kölner Phonetik) code for <paramref name="word"/>, suited to German.</summary>
    /// <param name="word">The word to encode.</param>
    /// <returns>A numeric Cologne phonetic code.</returns>
    /// <exception cref="ArgumentException"><paramref name="word"/> is null, empty or white-space.</exception>
    public static string Cologne(string word)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(word);

        var sb = new StringBuilder();
        foreach (var c in word)
        {
            if (char.IsLetter(c))
            {
                sb.Append(char.ToUpperInvariant(c));
            }
        }

        if (sb.Length == 0)
        {
            throw new ArgumentException("Input contains no letters.", nameof(word));
        }

        var s = sb.ToString();
        var codes = new StringBuilder();

        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            var prev = i > 0 ? s[i - 1] : '\0';
            var next = i + 1 < s.Length ? s[i + 1] : '\0';

            var code = c switch
            {
                'A' or 'E' or 'I' or 'J' or 'O' or 'U' or 'Y' => '0',
                'H' => 'x',
                'B' => '1',
                'P' => next == 'H' ? '3' : '1',
                'D' or 'T' => next is 'C' or 'S' or 'Z' ? '8' : '2',
                'F' or 'V' or 'W' => '3',
                'G' or 'K' or 'Q' => '4',
                'C' => CologneC(prev, next, i == 0),
                'X' => prev is 'C' or 'K' or 'Q' ? '8' : 'X',
                'L' => '5',
                'M' or 'N' => '6',
                'R' => '7',
                'S' or 'Z' => '8',
                _ => 'x',
            };

            if (code == 'X')
            {
                // 'X' not after C/K/Q expands to '48'
                codes.Append("48");
                continue;
            }

            if (code == 'x')
            {
                continue;
            }

            // Collapse consecutive duplicate digits.
            if (codes.Length == 0 || codes[^1] != code)
            {
                codes.Append(code);
            }
        }

        // Remove all '0' except a leading one.
        var output = new StringBuilder();
        for (var i = 0; i < codes.Length; i++)
        {
            if (codes[i] != '0' || i == 0)
            {
                output.Append(codes[i]);
            }
        }

        return output.ToString();
    }

    /// <summary>Determines whether two words share the same Cologne phonetic code.</summary>
    /// <param name="first">The first word.</param>
    /// <param name="second">The second word.</param>
    /// <returns><see langword="true"/> if the words sound alike under Cologne phonetics; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Either argument is null, empty or white-space.</exception>
    public static bool CologneEquals(string first, string second) =>
        string.Equals(Cologne(first), Cologne(second), StringComparison.Ordinal);

    private static char SoundexCode(char c) => c switch
    {
        'B' or 'F' or 'P' or 'V' => '1',
        'C' or 'G' or 'J' or 'K' or 'Q' or 'S' or 'X' or 'Z' => '2',
        'D' or 'T' => '3',
        'L' => '4',
        'M' or 'N' => '5',
        'R' => '6',
        _ => '0',
    };

    private static bool IsNysiisVowel(char c) => c is 'A' or 'E' or 'I' or 'O' or 'U';

    private static char CologneC(char prev, char next, bool isStart)
    {
        if (isStart)
        {
            return next is 'A' or 'H' or 'K' or 'L' or 'O' or 'Q' or 'R' or 'U' or 'X' ? '4' : '8';
        }

        if (prev is 'S' or 'Z')
        {
            return '8';
        }

        return next is 'A' or 'H' or 'K' or 'O' or 'Q' or 'U' or 'X' ? '4' : '8';
    }

    private static string ReplaceStart(string value, string prefix, string replacement) =>
        value.StartsWith(prefix, StringComparison.Ordinal) ? replacement + value[prefix.Length..] : value;

    private static string ReplaceEnd(string value, string suffix, string replacement) =>
        value.EndsWith(suffix, StringComparison.Ordinal) ? value[..^suffix.Length] + replacement : value;
}
