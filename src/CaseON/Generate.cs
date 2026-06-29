using System.Security.Cryptography;
using System.Text;

namespace CaseON;

/// <summary>
/// Generates random strings, tokens and identifiers using a cryptographically secure random number
/// generator. Call as <c>CaseON.Generate.Token()</c> or, after <c>using CaseON;</c>, simply
/// <c>Generate.Token()</c>.
/// </summary>
/// <remarks>All randomness is sourced from <see cref="RandomNumberGenerator"/>, making the output suitable for security-sensitive tokens.</remarks>
public static class Generate
{
    private const string Lower = "abcdefghijklmnopqrstuvwxyz";
    private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Digits = "0123456789";
    private const string Symbols = "!@#$%^&*()-_=+[]{};:,.?";

    /// <summary>Generates a random string of the given <paramref name="length"/> from a custom character set.</summary>
    /// <param name="length">The number of characters to generate.</param>
    /// <param name="charset">The set of characters to draw from.</param>
    /// <returns>A random string.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    /// <exception cref="ArgumentException"><paramref name="charset"/> is null or empty.</exception>
    public static string String(int length, string charset)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentException.ThrowIfNullOrEmpty(charset);

        if (length == 0)
        {
            return string.Empty;
        }

        var buffer = new char[length];
        for (var i = 0; i < length; i++)
        {
            buffer[i] = charset[RandomNumberGenerator.GetInt32(charset.Length)];
        }

        return new string(buffer);
    }

    /// <summary>Generates a random alphabetic string (<c>A-Z</c>, <c>a-z</c>).</summary>
    /// <param name="length">The number of characters to generate.</param>
    /// <returns>A random alphabetic string.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static string Alpha(int length) => String(length, Upper + Lower);

    /// <summary>Generates a random numeric string (<c>0-9</c>).</summary>
    /// <param name="length">The number of digits to generate.</param>
    /// <returns>A random numeric string.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static string Numeric(int length) => String(length, Digits);

    /// <summary>Generates a random alphanumeric string (<c>A-Z</c>, <c>a-z</c>, <c>0-9</c>).</summary>
    /// <param name="length">The number of characters to generate.</param>
    /// <returns>A random alphanumeric string.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static string AlphaNumeric(int length) => String(length, Upper + Lower + Digits);

    /// <summary>Generates a random lower-case hexadecimal string.</summary>
    /// <param name="length">The number of hex characters to generate.</param>
    /// <returns>A random hex string.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static string Hex(int length) => String(length, "0123456789abcdef");

    /// <summary>Generates random bytes and returns them as Base64.</summary>
    /// <param name="byteCount">The number of random bytes to generate.</param>
    /// <returns>A Base64-encoded random string.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="byteCount"/> is negative.</exception>
    public static string Base64(int byteCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(byteCount);
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(byteCount));
    }

    /// <summary>Generates a URL-safe random token (Base64Url without padding).</summary>
    /// <param name="byteCount">The number of random bytes of entropy. Defaults to 32 (256 bits).</param>
    /// <returns>A URL-safe random token.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="byteCount"/> is not positive.</exception>
    public static string Token(int byteCount = 32)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(byteCount);
        var bytes = RandomNumberGenerator.GetBytes(byteCount);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>Generates a new <see cref="System.Guid"/> as a string.</summary>
    /// <returns>A new GUID in <c>D</c> format.</returns>
    public static string Guid() => System.Guid.NewGuid().ToString();

    /// <summary>Generates a random password that includes at least one character from each enabled class.</summary>
    /// <param name="length">The password length. Must be at least the number of enabled character classes.</param>
    /// <param name="includeUppercase">Include upper-case letters. Defaults to <see langword="true"/>.</param>
    /// <param name="includeLowercase">Include lower-case letters. Defaults to <see langword="true"/>.</param>
    /// <param name="includeDigits">Include digits. Defaults to <see langword="true"/>.</param>
    /// <param name="includeSymbols">Include symbols. Defaults to <see langword="true"/>.</param>
    /// <returns>A randomly generated password.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is smaller than the number of enabled classes.</exception>
    /// <exception cref="ArgumentException">No character classes are enabled.</exception>
    public static string Password(
        int length,
        bool includeUppercase = true,
        bool includeLowercase = true,
        bool includeDigits = true,
        bool includeSymbols = true)
    {
        var classes = new List<string>(4);
        if (includeUppercase)
        {
            classes.Add(Upper);
        }

        if (includeLowercase)
        {
            classes.Add(Lower);
        }

        if (includeDigits)
        {
            classes.Add(Digits);
        }

        if (includeSymbols)
        {
            classes.Add(Symbols);
        }

        if (classes.Count == 0)
        {
            throw new ArgumentException("At least one character class must be enabled.", nameof(includeUppercase));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(length, classes.Count);

        var all = string.Concat(classes);
        var chars = new char[length];

        // Guarantee at least one character from each enabled class.
        for (var i = 0; i < classes.Count; i++)
        {
            chars[i] = classes[i][RandomNumberGenerator.GetInt32(classes[i].Length)];
        }

        for (var i = classes.Count; i < length; i++)
        {
            chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
        }

        // Fisher-Yates shuffle so the guaranteed characters are not always at the front.
        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}
