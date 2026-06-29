using System.Text;
using CaseON.Internal;

namespace CaseON;

/// <summary>
/// Decodes text from common binary-to-text and escaping formats. Call as <c>CaseON.Decode.Base64(text)</c>
/// or, after <c>using CaseON;</c>, simply <c>Decode.Base64(text)</c>. Use <see cref="Encode"/> for the inverse.
/// </summary>
/// <remarks>Binary decodings interpret the decoded bytes as UTF-8. Methods throw <see cref="ArgumentNullException"/> for <see langword="null"/> input and <see cref="FormatException"/> for malformed input.</remarks>
public static class Decode
{
    /// <summary>Decodes a Base64 string to text.</summary>
    /// <param name="input">The Base64 text to decode.</param>
    /// <returns>The decoded UTF-8 string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="input"/> is not valid Base64.</exception>
    public static string Base64(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Encoding.UTF8.GetString(Convert.FromBase64String(input));
    }

    /// <summary>Decodes a URL-safe Base64 string (with or without padding) to text.</summary>
    /// <param name="input">The Base64Url text to decode.</param>
    /// <returns>The decoded UTF-8 string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="input"/> is not valid Base64Url.</exception>
    public static string Base64Url(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var normalized = input.Replace('-', '+').Replace('_', '/');
        switch (normalized.Length % 4)
        {
            case 2:
                normalized += "==";
                break;
            case 3:
                normalized += "=";
                break;
        }

        return Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
    }

    /// <summary>Decodes an RFC 4648 Base32 string to text.</summary>
    /// <param name="input">The Base32 text to decode.</param>
    /// <returns>The decoded UTF-8 string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="input"/> contains invalid Base32 characters.</exception>
    public static string Base32(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Encoding.UTF8.GetString(BaseCodec.Base32Decode(input));
    }

    /// <summary>Decodes a Base58 string (Bitcoin alphabet) to text.</summary>
    /// <param name="input">The Base58 text to decode.</param>
    /// <returns>The decoded UTF-8 string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="input"/> contains invalid Base58 characters.</exception>
    public static string Base58(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Encoding.UTF8.GetString(BaseCodec.Base58Decode(input));
    }

    /// <summary>Decodes an Ascii85 (Base85) string to text.</summary>
    /// <param name="input">The Ascii85 text to decode.</param>
    /// <returns>The decoded UTF-8 string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="input"/> contains invalid Ascii85 characters.</exception>
    public static string Base85(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Encoding.UTF8.GetString(BaseCodec.Ascii85Decode(input));
    }

    /// <summary>Decodes a hexadecimal string to text.</summary>
    /// <param name="input">The hex text to decode.</param>
    /// <returns>The decoded UTF-8 string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="input"/> is not a valid hex string.</exception>
    public static string Hex(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Encoding.UTF8.GetString(Convert.FromHexString(input));
    }

    /// <summary>Decodes a percent-encoded (URL-encoded) string.</summary>
    /// <param name="input">The URL-encoded text to decode.</param>
    /// <returns>The decoded text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Url(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Uri.UnescapeDataString(input);
    }

    /// <summary>Decodes HTML entities in <paramref name="input"/>.</summary>
    /// <param name="input">The HTML-encoded text to decode.</param>
    /// <returns>The decoded text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Html(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return System.Net.WebUtility.HtmlDecode(input);
    }

    /// <summary>Unescapes a JavaScript string literal body in <paramref name="input"/>.</summary>
    /// <param name="input">The escaped text to decode (without surrounding quotes).</param>
    /// <returns>The unescaped text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="input"/> contains an invalid escape sequence.</exception>
    public static string JavaScript(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var builder = new StringBuilder(input.Length);
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c != '\\')
            {
                builder.Append(c);
                continue;
            }

            i++;
            if (i >= input.Length)
            {
                throw new FormatException("Dangling escape character at end of input.");
            }

            var e = input[i];
            switch (e)
            {
                case 'n':
                    builder.Append('\n');
                    break;
                case 'r':
                    builder.Append('\r');
                    break;
                case 't':
                    builder.Append('\t');
                    break;
                case 'b':
                    builder.Append('\b');
                    break;
                case 'f':
                    builder.Append('\f');
                    break;
                case '0':
                    builder.Append('\0');
                    break;
                case '"':
                    builder.Append('"');
                    break;
                case '\'':
                    builder.Append('\'');
                    break;
                case '\\':
                    builder.Append('\\');
                    break;
                case 'u':
                    builder.Append(ReadHex(input, ref i, 4));
                    break;
                case 'x':
                    builder.Append(ReadHex(input, ref i, 2));
                    break;
                default:
                    builder.Append(e);
                    break;
            }
        }

        return builder.ToString();
    }

    private static char ReadHex(string input, ref int i, int length)
    {
        if (i + length >= input.Length)
        {
            throw new FormatException("Truncated Unicode escape sequence.");
        }

        var hex = input.Substring(i + 1, length);
        if (!ushort.TryParse(hex, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var code))
        {
            throw new FormatException($"Invalid escape sequence '\\{(length == 4 ? 'u' : 'x')}{hex}'.");
        }

        i += length;
        return (char)code;
    }
}
