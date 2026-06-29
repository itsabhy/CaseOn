using CaseON;
using Xunit;

namespace CaseON.Tests;

public class ValidateAndCountryTests
{
    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("bad@", false)]
    [InlineData("no-at-sign", false)]
    [InlineData(null, false)]
    public void Email_Works(string? input, bool expected) =>
        Assert.Equal(expected, Validate.Email(input));

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("ftp://files.example.com/a", true)]
    [InlineData("notaurl", false)]
    public void Url_Works(string? input, bool expected) =>
        Assert.Equal(expected, Validate.Url(input));

    [Theory]
    [InlineData("192.168.0.1", true)]
    [InlineData("256.0.0.1", false)]
    [InlineData("::1", false)]
    public void IPv4_Works(string? input, bool expected) =>
        Assert.Equal(expected, Validate.IPv4(input));

    [Theory]
    [InlineData("4111111111111111", true)] // Visa test number
    [InlineData("1234567890123456", false)]
    public void CreditCard_Luhn_Works(string? input, bool expected) =>
        Assert.Equal(expected, Validate.CreditCard(input));

    [Theory]
    [InlineData("GB82WEST12345698765432", true)]
    [InlineData("GB82 WEST 1234 5698 7654 32", true)]
    [InlineData("GB00WEST12345698765432", false)]
    public void Iban_Works(string? input, bool expected) =>
        Assert.Equal(expected, Validate.Iban(input));

    [Theory]
    [InlineData("0306406152", true)]
    [InlineData("9780306406157", true)]
    [InlineData("1234567890", false)]
    public void Isbn_Works(string? input, bool expected) =>
        Assert.Equal(expected, Validate.Isbn(input));

    [Theory]
    [InlineData("1.2.3", true)]
    [InlineData("1.2.3-alpha.1+build", true)]
    [InlineData("1.2", false)]
    public void SemVer_Works(string? input, bool expected) =>
        Assert.Equal(expected, Validate.SemVer(input));

    [Theory]
    [InlineData("hello-world", true)]
    [InlineData("Hello World", false)]
    public void Slug_Works(string? input, bool expected) =>
        Assert.Equal(expected, Validate.Slug(input));

    [Fact]
    public void Misc_Validators_Work()
    {
        Assert.True(Validate.Guid("d2719a4e-5b5b-4f1a-9c0a-7d2b8d9e6f10"));
        Assert.True(Validate.Domain("example.com"));
        Assert.True(Validate.Port("8080"));
        Assert.False(Validate.Port("70000"));
        Assert.True(Validate.Coordinate("40.7128, -74.0060"));
        Assert.True(Validate.HexColor("#1a2B3c"));
        Assert.True(Validate.Jwt("aaa.bbb.ccc"));
    }

    [Fact]
    public void HtmlElement_IsReDoSSafe()
    {
        var malicious = "<a>" + new string('a', 50_000);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = Validate.HtmlElement(malicious);
        sw.Stop();

        Assert.False(result);
        Assert.True(sw.ElapsedMilliseconds < 2000, $"Validation took {sw.ElapsedMilliseconds} ms.");
    }

    [Fact]
    public void Country_Lookups_Work()
    {
        Assert.Equal(1, Country.GetCallingCode(CountryCode.UnitedStates));
        Assert.Equal(91, Country.GetCallingCode(CountryCode.India));
        Assert.Equal("INR", Country.GetInfo(CountryCode.India).CurrencyCode);
        Assert.True(Country.All.Count > 200);
    }

    [Theory]
    [InlineData("US", CountryCode.UnitedStates)]
    [InlineData("usa", CountryCode.UnitedStates)]
    [InlineData("IN", CountryCode.India)]
    public void Country_TryParse_Works(string iso, CountryCode expected)
    {
        Assert.True(Country.TryParse(iso, out var code));
        Assert.Equal(expected, code);
    }

    [Fact]
    public void Country_TryParse_Fails_OnUnknown() =>
        Assert.False(Country.TryParse("ZZ", out _));

    [Theory]
    [InlineData("90210", CountryCode.UnitedStates, true)]
    [InlineData("12345-6789", CountryCode.UnitedStates, true)]
    [InlineData("M5V 3L9", CountryCode.Canada, true)]
    [InlineData("110001", CountryCode.India, true)]
    [InlineData("ABCDE", CountryCode.UnitedStates, false)]
    public void PostalCode_Works(string input, CountryCode country, bool expected) =>
        Assert.Equal(expected, Validate.PostalCode(input, country));

    [Theory]
    [InlineData("+14155552671", CountryCode.UnitedStates, true)]
    [InlineData("4155552671", CountryCode.UnitedStates, true)]
    [InlineData("abc", CountryCode.UnitedStates, false)]
    public void PhoneNumber_ByCountry_Works(string input, CountryCode country, bool expected) =>
        Assert.Equal(expected, Validate.PhoneNumber(input, country));
}
