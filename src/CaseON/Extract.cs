using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CaseON;

/// <summary>
/// Extracts structured fragments (emails, URLs, numbers, hashtags and more) from free text. Call as
/// <c>CaseON.Extract.Emails(text)</c> or, after <c>using CaseON;</c>, simply <c>Extract.Emails(text)</c>.
/// </summary>
/// <remarks>
/// Every method returns an empty array for <see langword="null"/> or empty input. The underlying regular
/// expressions are source-generated, culture-invariant and guarded with a match timeout.
/// </remarks>
public static partial class Extract
{
    [GeneratedRegex(@"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\b(?:https?|ftp)://[^\s<>""']+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex UrlRegex();

    [GeneratedRegex(@"\+?\d[\d\s().\-]{6,}\d", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex GuidRegex();

    [GeneratedRegex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex IpRegex();

    [GeneratedRegex(@"(?<=^|\s)#(\w+)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HashtagRegex();

    [GeneratedRegex(@"(?<=^|\s)@(\w+)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex MentionRegex();

    [GeneratedRegex(@"-?\d+(?:\.\d+)?", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex NumberRegex();

    /// <summary>Extracts all email addresses from <paramref name="input"/>.</summary>
    /// <param name="input">The text to search.</param>
    /// <returns>An array of matched email addresses (possibly empty).</returns>
    public static string[] Emails(string? input) => Matches(input, EmailRegex());

    /// <summary>Extracts all HTTP, HTTPS and FTP URLs from <paramref name="input"/>.</summary>
    /// <param name="input">The text to search.</param>
    /// <returns>An array of matched URLs (possibly empty).</returns>
    public static string[] Urls(string? input) => Matches(input, UrlRegex());

    /// <summary>Extracts candidate phone numbers from <paramref name="input"/>.</summary>
    /// <param name="input">The text to search.</param>
    /// <returns>An array of matched phone-number-like fragments (possibly empty).</returns>
    public static string[] PhoneNumbers(string? input) => Matches(input, PhoneRegex());

    /// <summary>Extracts all GUIDs from <paramref name="input"/>.</summary>
    /// <param name="input">The text to search.</param>
    /// <returns>An array of matched GUIDs (possibly empty).</returns>
    public static string[] Guids(string? input) => Matches(input, GuidRegex());

    /// <summary>Extracts all IPv4 addresses from <paramref name="input"/>.</summary>
    /// <param name="input">The text to search.</param>
    /// <returns>An array of matched IPv4 addresses (possibly empty).</returns>
    public static string[] IpAddresses(string? input) => Matches(input, IpRegex());

    /// <summary>Extracts hashtags (without the leading <c>#</c>) from <paramref name="input"/>.</summary>
    /// <param name="input">The text to search.</param>
    /// <returns>An array of hashtag words (possibly empty).</returns>
    public static string[] Hashtags(string? input) => CapturedGroups(input, HashtagRegex());

    /// <summary>Extracts mentions (without the leading <c>@</c>) from <paramref name="input"/>.</summary>
    /// <param name="input">The text to search.</param>
    /// <returns>An array of mentioned handles (possibly empty).</returns>
    public static string[] Mentions(string? input) => CapturedGroups(input, MentionRegex());

    /// <summary>Extracts all numbers (integer or decimal) from <paramref name="input"/>.</summary>
    /// <param name="input">The text to search.</param>
    /// <returns>An array of matched numbers as strings (possibly empty).</returns>
    public static string[] Numbers(string? input) => Matches(input, NumberRegex());

    /// <summary>Extracts the registrable domains of all URLs found in <paramref name="input"/>.</summary>
    /// <param name="input">The text to search.</param>
    /// <returns>An array of host names (possibly empty).</returns>
    public static string[] Domains(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return [];
        }

        var domains = new List<string>();
        foreach (var url in Urls(input))
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                domains.Add(uri.Host);
            }
        }

        return [.. domains];
    }

    private static string[] Matches(string? input, Regex regex)
    {
        if (string.IsNullOrEmpty(input))
        {
            return [];
        }

        try
        {
            var matches = regex.Matches(input);
            if (matches.Count == 0)
            {
                return [];
            }

            var result = new string[matches.Count];
            for (var i = 0; i < matches.Count; i++)
            {
                result[i] = matches[i].Value;
            }

            return result;
        }
        catch (RegexMatchTimeoutException)
        {
            return [];
        }
    }

    private static string[] CapturedGroups(string? input, Regex regex)
    {
        if (string.IsNullOrEmpty(input))
        {
            return [];
        }

        try
        {
            var matches = regex.Matches(input);
            if (matches.Count == 0)
            {
                return [];
            }

            var result = new string[matches.Count];
            for (var i = 0; i < matches.Count; i++)
            {
                result[i] = matches[i].Groups[1].Value;
            }

            return result;
        }
        catch (RegexMatchTimeoutException)
        {
            return [];
        }
    }
}
