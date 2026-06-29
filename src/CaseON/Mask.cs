using System.Text;

namespace CaseON;

/// <summary>
/// Masks sensitive values for display and logging. Call as <c>CaseON.Mask.Email(value)</c> or, after
/// <c>using CaseON;</c>, simply <c>Mask.Email(value)</c>.
/// </summary>
/// <remarks>
/// Masking reduces accidental disclosure in UI and logs; it is not a security control. Methods throw
/// <see cref="ArgumentNullException"/> for <see langword="null"/> input.
/// </remarks>
public static class Mask
{
    private const char DefaultMaskChar = '*';

    /// <summary>Masks the local part of an email address, keeping the first character and the domain.</summary>
    /// <param name="input">The email address to mask.</param>
    /// <param name="maskChar">The masking character. Defaults to <c>*</c>.</param>
    /// <returns>The masked email, e.g. <c>j***@example.com</c>. Non-email input is masked as plain text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Email(string input, char maskChar = DefaultMaskChar)
    {
        ArgumentNullException.ThrowIfNull(input);

        var at = input.IndexOf('@', StringComparison.Ordinal);
        if (at <= 0)
        {
            return Custom(input, 1, Math.Max(0, input.Length - 1), maskChar);
        }

        var local = input[..at];
        var domain = input[at..];
        var maskedLocal = local.Length <= 1
            ? local
            : local[0] + new string(maskChar, local.Length - 1);
        return maskedLocal + domain;
    }

    /// <summary>Masks a phone number, keeping the last four digits.</summary>
    /// <param name="input">The phone number to mask.</param>
    /// <param name="maskChar">The masking character. Defaults to <c>*</c>.</param>
    /// <returns>The masked phone number.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Phone(string input, char maskChar = DefaultMaskChar) => KeepLast(input, 4, maskChar);

    /// <summary>Masks a card number, keeping the last four digits.</summary>
    /// <param name="input">The card number to mask.</param>
    /// <param name="maskChar">The masking character. Defaults to <c>*</c>.</param>
    /// <returns>The masked card number.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string CreditCard(string input, char maskChar = DefaultMaskChar) => KeepLast(input, 4, maskChar);

    /// <summary>Masks a primary account number (PAN), keeping the last four digits.</summary>
    /// <param name="input">The PAN to mask.</param>
    /// <param name="maskChar">The masking character. Defaults to <c>*</c>.</param>
    /// <returns>The masked PAN.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Pan(string input, char maskChar = DefaultMaskChar) => KeepLast(input, 4, maskChar);

    /// <summary>Masks an IBAN, keeping the first four and last four characters.</summary>
    /// <param name="input">The IBAN to mask.</param>
    /// <param name="maskChar">The masking character. Defaults to <c>*</c>.</param>
    /// <returns>The masked IBAN.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Iban(string input, char maskChar = DefaultMaskChar)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Length <= 8)
        {
            return new string(maskChar, input.Length);
        }

        return Custom(input, 4, input.Length - 8, maskChar);
    }

    /// <summary>Masks a US Social Security Number, keeping the last four digits (<c>***-**-1234</c>).</summary>
    /// <param name="input">The SSN to mask.</param>
    /// <param name="maskChar">The masking character. Defaults to <c>*</c>.</param>
    /// <returns>The masked SSN.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Ssn(string input, char maskChar = DefaultMaskChar) => KeepLast(input, 4, maskChar);

    /// <summary>Masks every character of a password.</summary>
    /// <param name="input">The password to mask.</param>
    /// <param name="maskChar">The masking character. Defaults to <c>*</c>.</param>
    /// <returns>A string of mask characters the same length as the input.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string Password(string input, char maskChar = DefaultMaskChar)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new string(maskChar, input.Length);
    }

    /// <summary>Masks <paramref name="count"/> characters of <paramref name="input"/> starting at <paramref name="start"/>.</summary>
    /// <param name="input">The text to mask.</param>
    /// <param name="start">The zero-based index at which masking begins.</param>
    /// <param name="count">The number of characters to mask.</param>
    /// <param name="maskChar">The masking character. Defaults to <c>*</c>.</param>
    /// <returns>The text with the specified range replaced by mask characters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> or <paramref name="count"/> is negative, or the range exceeds the input length.</exception>
    public static string Custom(string input, int start, int count, char maskChar = DefaultMaskChar)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (start + count > input.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "The masked range exceeds the input length.");
        }

        if (count == 0)
        {
            return input;
        }

        var builder = new StringBuilder(input.Length);
        builder.Append(input.AsSpan(0, start));
        builder.Append(maskChar, count);
        builder.Append(input.AsSpan(start + count));
        return builder.ToString();
    }

    private static string KeepLast(string input, int visible, char maskChar)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Mask all characters except the trailing visible alphanumeric characters; preserve separators.
        var maskUntil = input.Length;
        var seen = 0;
        for (var i = input.Length - 1; i >= 0 && seen < visible; i--)
        {
            if (char.IsLetterOrDigit(input[i]))
            {
                seen++;
                maskUntil = i;
            }
        }

        var builder = new StringBuilder(input.Length);
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (i < maskUntil && char.IsLetterOrDigit(c))
            {
                builder.Append(maskChar);
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
