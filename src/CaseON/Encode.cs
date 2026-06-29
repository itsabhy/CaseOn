using System.Text;
using CaseON.Internal;

namespace CaseON;

/// <summary>
/// Encodes text to common binary-to-text and escaping formats. Call as <c>CaseON.Encode.Base64(text)</c>
/// or, after <c>using CaseON;</c>, simply <c>Encode.Base64(text)</c>. Use <see cref="Decode"/> for the inverse.
/// </summary>
/// <remarks>Binary encodings operate on the UTF-8 bytes of the input. Methods throw <see cref="ArgumentNullException"/> for <see langword="null"/> input.</remarks>
public static class Encode
{
    /// <summary>Encodes <paramref name="input"/> as Base64.</summary>
    /// <param name="input">The text to encode.</param>
    /// <returns>The Base64 representation of the UTF-8 bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Base64(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
    }

    /// <summary>Encodes <paramref name="input"/> as URL-safe Base64 (no padding).</summary>
    /// <param name="input">The text to encode.</param>
    /// <returns>The Base64Url representation of the UTF-8 bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Base64Url(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>Encodes <paramref name="input"/> as RFC 4648 Base32.</summary>
    /// <param name="input">The text to encode.</param>
    /// <returns>The Base32 representation of the UTF-8 bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Base32(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return BaseCodec.Base32Encode(Encoding.UTF8.GetBytes(input));
    }

    /// <summary>Encodes <paramref name="input"/> as Base58 (Bitcoin alphabet).</summary>
    /// <param name="input">The text to encode.</param>
    /// <returns>The Base58 representation of the UTF-8 bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Base58(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return BaseCodec.Base58Encode(Encoding.UTF8.GetBytes(input));
    }

    /// <summary>Encodes <paramref name="input"/> as Ascii85 (Base85).</summary>
    /// <param name="input">The text to encode.</param>
    /// <returns>The Ascii85 representation of the UTF-8 bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Base85(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return BaseCodec.Ascii85Encode(Encoding.UTF8.GetBytes(input));
    }

    /// <summary>Encodes <paramref name="input"/> as a lower-case hexadecimal string.</summary>
    /// <param name="input">The text to encode.</param>
    /// <returns>The hex representation of the UTF-8 bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Hex(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Convert.ToHexString(Encoding.UTF8.GetBytes(input)).ToLowerInvariant();
    }

    /// <summary>Percent-encodes <paramref name="input"/> for safe inclusion in a URL.</summary>
    /// <param name="input">The text to encode.</param>
    /// <returns>The URL-encoded text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Url(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Uri.EscapeDataString(input);
    }

    /// <summary>HTML-encodes <paramref name="input"/>, escaping characters such as <c>&lt;</c>, <c>&gt;</c> and <c>&amp;</c>.</summary>
    /// <param name="input">The text to encode.</param>
    /// <returns>The HTML-encoded text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Html(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return System.Net.WebUtility.HtmlEncode(input);
    }

    /// <summary>Escapes <paramref name="input"/> for use inside a JavaScript string literal.</summary>
    /// <param name="input">The text to escape.</param>
    /// <returns>The escaped text (without surrounding quotes).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string JavaScript(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var builder = new StringBuilder(input.Length + 8);
        foreach (var c in input)
        {
            switch (c)
            {
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\'':
                    builder.Append("\\'");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                case '\b':
                    builder.Append("\\b");
                    break;
                case '\f':
                    builder.Append("\\f");
                    break;
                case '\u2028':
                    builder.Append("\\u2028");
                    break;
                case '\u2029':
                    builder.Append("\\u2029");
                    break;
                default:
                    if (c < ' ')
                    {
                        builder.Append("\\u").Append(((int)c).ToString("x4", System.Globalization.CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(c);
                    }

                    break;
            }
        }

        return builder.ToString();
    }
}
