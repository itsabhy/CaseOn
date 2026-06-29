using CaseON.Internal;

namespace CaseON;

public static partial class Validate
{
    /// <summary>Determines whether <paramref name="input"/> is a valid postal code for the specified <paramref name="country"/>.</summary>
    /// <param name="input">The value to validate.</param>
    /// <param name="country">The country whose postal-code format applies. Defaults to <see cref="CountryCode.UnitedStates"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the value matches the country's postal-code pattern. When the country has
    /// no defined pattern, a permissive check (3-10 alphanumeric characters, optionally with spaces or
    /// hyphens) is applied instead.
    /// </returns>
    public static bool PostalCode(string? input, CountryCode country = CountryCode.UnitedStates)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var pattern = Country.GetInfo(country).PostalCodePattern;
        if (pattern is null)
        {
            return GenericPostalCode(input);
        }

        return RegexCache.IsMatch(input, pattern);
    }

    /// <summary>Determines whether <paramref name="input"/> is a plausible phone number for the specified <paramref name="country"/>.</summary>
    /// <param name="input">The value to validate. Spaces, dashes, dots and parentheses are ignored.</param>
    /// <param name="country">The country whose international calling code applies.</param>
    /// <returns>
    /// <see langword="true"/> if the value is digits in international form for the country (optionally
    /// prefixed with the calling code) within E.164 length limits; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>Structural check only; it does not verify that the number is assigned or reachable.</remarks>
    public static bool PhoneNumber(string? input, CountryCode country)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var callingCode = Country.GetCallingCode(country).ToString(System.Globalization.CultureInfo.InvariantCulture);

        var hasPlus = false;
        Span<char> digits = stackalloc char[input.Length];
        var count = 0;
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsAsciiDigit(c))
            {
                digits[count++] = c;
            }
            else if (c == '+' && count == 0 && !hasPlus)
            {
                hasPlus = true;
            }
            else if (c is ' ' or '-' or '.' or '(' or ')' or '/')
            {
                continue;
            }
            else
            {
                return false;
            }
        }

        if (count == 0)
        {
            return false;
        }

        var national = digits[..count];

        // Strip a leading international prefix ("00") or the country calling code when present.
        var span = (ReadOnlySpan<char>)national;
        if (hasPlus && span.StartsWith(callingCode))
        {
            span = span[callingCode.Length..];
        }
        else if (span.StartsWith("00") && span[2..].StartsWith(callingCode))
        {
            span = span[(2 + callingCode.Length)..];
        }

        // Remaining national significant number: between 4 and 14 digits (E.164 caps the total at 15).
        return span.Length is >= 4 and <= 14;
    }

    private static bool GenericPostalCode(string input)
    {
        if (input.Length is < 3 or > 10)
        {
            return false;
        }

        foreach (var c in input)
        {
            if (!char.IsLetterOrDigit(c) && c != ' ' && c != '-')
            {
                return false;
            }
        }

        return true;
    }
}
