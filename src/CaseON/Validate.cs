using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace CaseON;

/// <summary>
/// Validates common string formats: internet identifiers, security tokens, dates, geo coordinates,
/// financial identifiers, web values, encodings, files and free text. Call as
/// <c>CaseON.Validate.Email("x")</c> or, after <c>using CaseON;</c>, simply <c>Validate.Email("x")</c>.
/// </summary>
/// <remarks>
/// <para>
/// Every method returns <see langword="false"/> (rather than throwing) for <see langword="null"/>,
/// empty or white-space input, so validators are safe to call on untrusted data.
/// </para>
/// <para>
/// Regular expressions are compiled once via the source generator and guarded with a match timeout,
/// eliminating per-call compilation cost and protecting against catastrophic backtracking. Many checks
/// are <em>structural</em> (format) rather than authoritative; this is noted on the relevant members.
/// </para>
/// </remarks>
public static partial class Validate
{
    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9]+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AlphanumericRegex();

    [GeneratedRegex(@"^\+?[1-9]\d{1,14}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HexColorRegex();

    [GeneratedRegex(@"[^\w\s]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SpecialCharacterRegex();

    [GeneratedRegex(@"^\d{3}-\d{2}-\d{4}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SsnRegex();

    [GeneratedRegex(@"^[\w\s.,'\-/#&]+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AddressRegex();

    [GeneratedRegex(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex MacAddressRegex();

    [GeneratedRegex(@"^[13][a-km-zA-HJ-NP-Z1-9]{25,34}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex BitcoinRegex();

    [GeneratedRegex(@"^\d{3,4}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex CvvRegex();

    [GeneratedRegex(@"^[A-Z0-9]+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex IbanCharsetRegex();

    // Non-backtracking structure: a single open tag with matching close tag, or a self-closing tag.
    [GeneratedRegex(@"^\s*<([a-zA-Z][\w-]*)\b[^>]*>.*</\1>\s*$|^\s*<[a-zA-Z][\w-]*\b[^>]*/>\s*$",
        RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HtmlRegex();

    [GeneratedRegex(@"^(?=.{1,253}$)(?:(?!-)[A-Za-z0-9-]{1,63}(?<!-)\.)+[A-Za-z]{2,63}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex DomainRegex();

    [GeneratedRegex(@"^(?=.{1,253}$)[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?(?:\.[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HostnameRegex();

    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SlugRegex();

    [GeneratedRegex(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[A-Za-z-][0-9A-Za-z-]*)(?:\.(?:0|[1-9]\d*|\d*[A-Za-z-][0-9A-Za-z-]*))*))?(?:\+([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SemVerRegex();

    [GeneratedRegex(@"^[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex JwtRegex();

    [GeneratedRegex(@"^([01]\d|2[0-3]):[0-5]\d(:[0-5]\d)?$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TimeRegex();

    [GeneratedRegex(@"^[0-9a-fA-F]+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HexRegex();

    // ----- Internet -----

    /// <summary>Determines whether <paramref name="input"/> is a plausibly formatted email address.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value matches a basic email pattern; otherwise <see langword="false"/>.</returns>
    /// <remarks>Structural check only; not RFC 5322 verification and does not confirm deliverability.</remarks>
    public static bool Email(string? input) =>
        !string.IsNullOrWhiteSpace(input) && EmailRegex().IsMatch(input);

    /// <summary>Determines whether <paramref name="input"/> is an absolute HTTP, HTTPS or FTP URL.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is a well-formed absolute URL with a supported scheme; otherwise <see langword="false"/>.</returns>
    public static bool Url(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return Uri.TryCreate(input, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp
                || uri.Scheme == Uri.UriSchemeHttps
                || uri.Scheme == Uri.UriSchemeFtp);
    }

    /// <summary>Determines whether <paramref name="input"/> is a valid IPv4 address.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value parses as an IPv4 address; otherwise <see langword="false"/>.</returns>
    public static bool IPv4(string? input) =>
        !string.IsNullOrWhiteSpace(input)
        && input.Count(static c => c == '.') == 3
        && IPAddress.TryParse(input, out var address)
        && address.AddressFamily == AddressFamily.InterNetwork;

    /// <summary>Determines whether <paramref name="input"/> is a valid IPv6 address (including compressed forms).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value parses as an IPv6 address; otherwise <see langword="false"/>.</returns>
    public static bool IPv6(string? input) =>
        !string.IsNullOrWhiteSpace(input)
        && IPAddress.TryParse(input, out var address)
        && address.AddressFamily == AddressFamily.InterNetworkV6;

    /// <summary>Determines whether <paramref name="input"/> is a valid IPv4 or IPv6 address.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value parses as an IP address; otherwise <see langword="false"/>.</returns>
    public static bool IpAddress(string? input) =>
        !string.IsNullOrWhiteSpace(input) && IPAddress.TryParse(input, out _);

    /// <summary>Determines whether <paramref name="input"/> is a valid MAC address.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value matches the MAC address format; otherwise <see langword="false"/>.</returns>
    public static bool MacAddress(string? input) =>
        !string.IsNullOrWhiteSpace(input) && MacAddressRegex().IsMatch(input);

    /// <summary>Determines whether <paramref name="input"/> is a valid registrable domain name (e.g. <c>example.com</c>).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value matches a domain pattern; otherwise <see langword="false"/>.</returns>
    public static bool Domain(string? input) =>
        !string.IsNullOrWhiteSpace(input) && DomainRegex().IsMatch(input);

    /// <summary>Determines whether <paramref name="input"/> is a valid host name (RFC 1123).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is a valid host name; otherwise <see langword="false"/>.</returns>
    public static bool Hostname(string? input) =>
        !string.IsNullOrWhiteSpace(input) && HostnameRegex().IsMatch(input);

    /// <summary>Determines whether <paramref name="input"/> is a valid TCP/UDP port number (0-65535).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is an integer port in range; otherwise <see langword="false"/>.</returns>
    public static bool Port(string? input) =>
        !string.IsNullOrWhiteSpace(input)
        && int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out var port)
        && port is >= 0 and <= 65535;

    /// <summary>Determines whether <paramref name="input"/> is a URL-friendly slug (<c>lower-case-words</c>).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is a valid slug; otherwise <see langword="false"/>.</returns>
    public static bool Slug(string? input) =>
        !string.IsNullOrWhiteSpace(input) && SlugRegex().IsMatch(input);

    /// <summary>Determines whether <paramref name="input"/> is a structurally valid JSON Web Token (three Base64Url segments).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value has the <c>header.payload.signature</c> shape; otherwise <see langword="false"/>.</returns>
    /// <remarks>Structural check only; it does not verify the signature.</remarks>
    public static bool Jwt(string? input) =>
        !string.IsNullOrWhiteSpace(input) && JwtRegex().IsMatch(input);

    // ----- Programming / identifiers -----

    /// <summary>Determines whether <paramref name="input"/> is a valid GUID.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value parses as a <see cref="Guid"/>; otherwise <see langword="false"/>.</returns>
    public static bool Guid(string? input) => System.Guid.TryParse(input, out _);

    /// <summary>Determines whether <paramref name="input"/> is a valid semantic version (<c>semver.org</c>).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is a valid SemVer 2.0.0 string; otherwise <see langword="false"/>.</returns>
    public static bool SemVer(string? input) =>
        !string.IsNullOrWhiteSpace(input) && SemVerRegex().IsMatch(input);

    /// <summary>Determines whether <paramref name="input"/> is a valid identifier (letter or underscore, then letters, digits or underscores).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is a valid identifier; otherwise <see langword="false"/>.</returns>
    public static bool Identifier(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        if (input[0] != '_' && !char.IsLetter(input[0]))
        {
            return false;
        }

        for (var i = 1; i < input.Length; i++)
        {
            var c = input[i];
            if (c != '_' && !char.IsLetterOrDigit(c))
            {
                return false;
            }
        }

        return true;
    }

    // ----- Date / time -----

    /// <summary>Determines whether <paramref name="input"/> is a valid date in the given <paramref name="format"/>.</summary>
    /// <param name="input">The value to validate.</param>
    /// <param name="format">The exact expected date format. Defaults to <c>yyyy-MM-dd</c>.</param>
    /// <returns><see langword="true"/> if the value matches the format; otherwise <see langword="false"/>.</returns>
    public static bool Date(string? input, string format = "yyyy-MM-dd") =>
        !string.IsNullOrWhiteSpace(input)
        && DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    /// <summary>Determines whether <paramref name="input"/> is a valid 24-hour time (<c>HH:mm</c> or <c>HH:mm:ss</c>).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is a valid time; otherwise <see langword="false"/>.</returns>
    public static bool Time(string? input) =>
        !string.IsNullOrWhiteSpace(input) && TimeRegex().IsMatch(input);

    /// <summary>Determines whether <paramref name="input"/> is a valid ISO 8601 date/time.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value parses as a round-trippable ISO 8601 timestamp; otherwise <see langword="false"/>.</returns>
    public static bool Iso8601(string? input) =>
        !string.IsNullOrWhiteSpace(input)
        && DateTimeOffset.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _)
        && input.Contains('T', StringComparison.OrdinalIgnoreCase);

    // ----- Geo -----

    /// <summary>Determines whether <paramref name="input"/> is a valid latitude (-90 to 90).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is a latitude in range; otherwise <see langword="false"/>.</returns>
    public static bool Latitude(string? input) =>
        double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
        && value is >= -90 and <= 90;

    /// <summary>Determines whether <paramref name="input"/> is a valid longitude (-180 to 180).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is a longitude in range; otherwise <see langword="false"/>.</returns>
    public static bool Longitude(string? input) =>
        double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
        && value is >= -180 and <= 180;

    /// <summary>Determines whether <paramref name="input"/> is a valid <c>latitude,longitude</c> coordinate pair.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if both components are valid; otherwise <see langword="false"/>.</returns>
    public static bool Coordinate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var comma = input.IndexOf(',', StringComparison.Ordinal);
        if (comma <= 0 || comma == input.Length - 1)
        {
            return false;
        }

        return Latitude(input[..comma].Trim()) && Longitude(input[(comma + 1)..].Trim());
    }

    // ----- Finance / IDs -----

    /// <summary>Determines whether <paramref name="input"/> is a valid card number under the Luhn algorithm.</summary>
    /// <param name="input">The digit string to validate.</param>
    /// <returns><see langword="true"/> if the value passes the Luhn check; otherwise <see langword="false"/>.</returns>
    public static bool CreditCard(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var sum = 0;
        var alternate = false;
        for (var i = input.Length - 1; i >= 0; i--)
        {
            var c = input[i];
            if (!char.IsAsciiDigit(c))
            {
                return false;
            }

            var n = c - '0';
            if (alternate)
            {
                n *= 2;
                if (n > 9)
                {
                    n -= 9;
                }
            }

            sum += n;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }

    /// <summary>Determines whether <paramref name="input"/> is a valid IBAN under the mod-97 algorithm.</summary>
    /// <param name="input">The value to validate. Spaces are ignored.</param>
    /// <returns><see langword="true"/> if the value is a structurally valid IBAN; otherwise <see langword="false"/>.</returns>
    public static bool Iban(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var cleaned = input.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        if (cleaned.Length is < 15 or > 34 || !IbanCharsetRegex().IsMatch(cleaned))
        {
            return false;
        }

        var rearranged = string.Concat(cleaned.AsSpan(4), cleaned.AsSpan(0, 4));
        var converted = string.Concat(rearranged.Select(static c =>
            char.IsLetter(c) ? (c - 'A' + 10).ToString(CultureInfo.InvariantCulture) : c.ToString()));

        return BigInteger.TryParse(converted, out var value) && value % 97 == 1;
    }

    /// <summary>Determines whether <paramref name="input"/> is a valid CVV (three or four digits).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value matches the CVV format; otherwise <see langword="false"/>.</returns>
    public static bool Cvv(string? input) =>
        !string.IsNullOrWhiteSpace(input) && CvvRegex().IsMatch(input);

    /// <summary>Determines whether <paramref name="input"/> is a valid nine-digit ABA routing number.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value passes the routing-number checksum; otherwise <see langword="false"/>.</returns>
    public static bool RoutingNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length != 9 || !input.All(char.IsAsciiDigit))
        {
            return false;
        }

        var checksum = 0;
        for (var i = 0; i < 9; i += 3)
        {
            checksum += ((input[i] - '0') * 3) + ((input[i + 1] - '0') * 7) + (input[i + 2] - '0');
        }

        return checksum % 10 == 0;
    }

    /// <summary>Determines whether <paramref name="input"/> is a valid ISBN-10 or ISBN-13.</summary>
    /// <param name="input">The value to validate. Hyphens and spaces are ignored.</param>
    /// <returns><see langword="true"/> if the value passes the ISBN checksum; otherwise <see langword="false"/>.</returns>
    public static bool Isbn(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var cleaned = input
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        return cleaned.Length switch
        {
            10 => IsValidIsbn10(cleaned),
            13 => IsValidIsbn13(cleaned),
            _ => false,
        };
    }

    /// <summary>Determines whether <paramref name="input"/> is a valid US Social Security Number (<c>###-##-####</c>).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value matches the SSN format; otherwise <see langword="false"/>.</returns>
    public static bool Ssn(string? input) =>
        !string.IsNullOrWhiteSpace(input) && SsnRegex().IsMatch(input);

    /// <summary>Determines whether <paramref name="input"/> is a valid Bitcoin (P2PKH/P2SH) address.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value matches the legacy Bitcoin address format; otherwise <see langword="false"/>.</returns>
    public static bool BitcoinAddress(string? input) =>
        !string.IsNullOrWhiteSpace(input) && BitcoinRegex().IsMatch(input);

    /// <summary>Determines whether <paramref name="input"/> is a valid E.164 phone number.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value matches the E.164 format; otherwise <see langword="false"/>.</returns>
    public static bool PhoneNumber(string? input) =>
        !string.IsNullOrWhiteSpace(input) && PhoneRegex().IsMatch(input);

    // ----- Web / markup / encoding -----

    /// <summary>Determines whether <paramref name="input"/> is well-formed JSON.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value parses as JSON; otherwise <see langword="false"/>.</returns>
    public static bool Json(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        try
        {
            using var _ = JsonDocument.Parse(input);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>Determines whether <paramref name="input"/> is well-formed XML.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value parses as XML; otherwise <see langword="false"/>.</returns>
    public static bool Xml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        try
        {
            XDocument.Parse(input);
            return true;
        }
        catch (XmlException)
        {
            return false;
        }
    }

    /// <summary>Determines whether <paramref name="input"/> resembles a single well-formed HTML element.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value matches a basic HTML element structure; otherwise <see langword="false"/>.</returns>
    /// <remarks>Lightweight structural check; not a substitute for a full HTML parser.</remarks>
    public static bool HtmlElement(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        try
        {
            return HtmlRegex().IsMatch(input);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    /// <summary>Determines whether <paramref name="input"/> is a valid Base64-encoded string.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value decodes as Base64; otherwise <see langword="false"/>.</returns>
    public static bool Base64(string? input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length % 4 != 0)
        {
            return false;
        }

        Span<byte> buffer = input.Length <= 1024 ? stackalloc byte[input.Length] : new byte[input.Length];
        return Convert.TryFromBase64String(input, buffer, out _);
    }

    /// <summary>Determines whether <paramref name="input"/> contains only hexadecimal digits.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is a hex string; otherwise <see langword="false"/>.</returns>
    public static bool Hex(string? input) =>
        !string.IsNullOrWhiteSpace(input) && HexRegex().IsMatch(input);

    // ----- Color -----

    /// <summary>Determines whether <paramref name="input"/> is a valid hex color (<c>#RGB</c>, <c>#RGBA</c>, <c>#RRGGBB</c> or <c>#RRGGBBAA</c>).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is a valid hex color; otherwise <see langword="false"/>.</returns>
    public static bool HexColor(string? input) =>
        !string.IsNullOrWhiteSpace(input) && HexColorRegex().IsMatch(input);

    // ----- File -----

    /// <summary>Determines whether <paramref name="input"/> contains no characters that are invalid in a file path.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value contains no invalid path characters; otherwise <see langword="false"/>.</returns>
    public static bool FilePath(string? input) =>
        !string.IsNullOrWhiteSpace(input) && input.IndexOfAny(Path.GetInvalidPathChars()) == -1;

    /// <summary>Determines whether <paramref name="input"/> is a valid file name (no directory separators or invalid characters).</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is a valid file name; otherwise <see langword="false"/>.</returns>
    public static bool FileName(string? input) =>
        !string.IsNullOrWhiteSpace(input) && input.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;

    /// <summary>Determines whether <paramref name="input"/> matches a permissive physical-address pattern.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value looks like an address; otherwise <see langword="false"/>.</returns>
    public static bool Address(string? input) =>
        !string.IsNullOrWhiteSpace(input) && AddressRegex().IsMatch(input);

    // ----- Free text -----

    /// <summary>Determines whether <paramref name="input"/> contains only letters and digits.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is non-empty and alphanumeric; otherwise <see langword="false"/>.</returns>
    public static bool Alphanumeric(string? input) =>
        !string.IsNullOrWhiteSpace(input) && AlphanumericRegex().IsMatch(input);

    /// <summary>Determines whether <paramref name="input"/> contains only decimal digits.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is non-empty and numeric; otherwise <see langword="false"/>.</returns>
    public static bool Numeric(string? input) =>
        !string.IsNullOrWhiteSpace(input) && input.All(char.IsAsciiDigit);

    /// <summary>Determines whether <paramref name="input"/> contains only letters.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if the value is non-empty and alphabetic; otherwise <see langword="false"/>.</returns>
    public static bool Alphabetic(string? input) =>
        !string.IsNullOrWhiteSpace(input) && input.All(char.IsLetter);

    /// <summary>Determines whether <paramref name="input"/> contains only ASCII characters.</summary>
    /// <param name="input">The value to validate.</param>
    /// <returns><see langword="true"/> if every character is ASCII; otherwise <see langword="false"/>.</returns>
    public static bool Ascii(string? input) =>
        !string.IsNullOrEmpty(input) && input.All(char.IsAscii);

    /// <summary>Determines whether <paramref name="input"/> contains at least one special (non letter/digit/whitespace) character.</summary>
    /// <param name="input">The value to inspect.</param>
    /// <returns><see langword="true"/> if the value contains a special character; otherwise <see langword="false"/>.</returns>
    public static bool ContainsSpecialCharacters(string? input) =>
        !string.IsNullOrWhiteSpace(input) && SpecialCharacterRegex().IsMatch(input);

    private static bool IsValidIsbn10(string value)
    {
        var sum = 0;
        for (var i = 0; i < 9; i++)
        {
            if (!char.IsAsciiDigit(value[i]))
            {
                return false;
            }

            sum += (value[i] - '0') * (10 - i);
        }

        var checkChar = value[9];
        var checkValue = checkChar switch
        {
            'X' or 'x' => 10,
            >= '0' and <= '9' => checkChar - '0',
            _ => -1,
        };

        return checkValue >= 0 && (sum + checkValue) % 11 == 0;
    }

    private static bool IsValidIsbn13(string value)
    {
        if (!value.All(char.IsAsciiDigit))
        {
            return false;
        }

        var sum = 0;
        for (var i = 0; i < 13; i++)
        {
            sum += (value[i] - '0') * (i % 2 == 0 ? 1 : 3);
        }

        return sum % 10 == 0;
    }
}
