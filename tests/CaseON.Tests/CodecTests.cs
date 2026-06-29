using CaseON;
using Xunit;

namespace CaseON.Tests;

public class CodecTests
{
    [Theory]
    [InlineData("abc", "900150983cd24fb0d6963f7d28e17f72")]
    public void Md5_KnownVector(string input, string expected) =>
        Assert.Equal(expected, Hash.Md5(input));

    [Theory]
    [InlineData("abc", "a9993e364706816aba3e25717850c26c9cd0d89d")]
    public void Sha1_KnownVector(string input, string expected) =>
        Assert.Equal(expected, Hash.Sha1(input));

    [Theory]
    [InlineData("abc", "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad")]
    public void Sha256_KnownVector(string input, string expected) =>
        Assert.Equal(expected, Hash.Sha256(input));

    [Fact]
    public void NonCryptoHashes_KnownVectors()
    {
        Assert.Equal("cbf43926", Hash.Crc32("123456789"));
        Assert.Equal("811c9dc5", Hash.Fnv1a32(string.Empty));
        Assert.Equal("ef46db3751d8e999", Hash.XxHash64(string.Empty));
    }

    [Theory]
    [InlineData("Hello, World! 🌍")]
    [InlineData("")]
    [InlineData("The quick brown fox")]
    public void Encode_Decode_RoundTrips(string input)
    {
        Assert.Equal(input, Decode.Base64(Encode.Base64(input)));
        Assert.Equal(input, Decode.Base64Url(Encode.Base64Url(input)));
        Assert.Equal(input, Decode.Base32(Encode.Base32(input)));
        Assert.Equal(input, Decode.Base58(Encode.Base58(input)));
        Assert.Equal(input, Decode.Base85(Encode.Base85(input)));
        Assert.Equal(input, Decode.Hex(Encode.Hex(input)));
        Assert.Equal(input, Decode.Url(Encode.Url(input)));
        Assert.Equal(input, Decode.Html(Encode.Html(input)));
        Assert.Equal(input, Decode.JavaScript(Encode.JavaScript(input)));
    }

    [Fact]
    public void JavaScript_Escapes_SpecialCharacters()
    {
        var input = "line1\nline2\t\"quoted\"";
        var encoded = Encode.JavaScript(input);
        Assert.DoesNotContain('\n', encoded);
        Assert.Equal(input, Decode.JavaScript(encoded));
    }

    [Fact]
    public void Mask_Works()
    {
        Assert.Equal("j***@example.com", Mask.Email("john@example.com"));
        Assert.Equal("************1111", Mask.CreditCard("4111111111111111"));
        Assert.Equal("****word", Mask.Custom("password", 0, 4));
        Assert.Equal("********", Mask.Password("12345678"));
    }

    [Fact]
    public void Generate_ProducesValidStrings()
    {
        Assert.Equal(20, Generate.AlphaNumeric(20).Length);
        Assert.True(Generate.Numeric(10).All(char.IsDigit));
        Assert.True(Generate.AlphaNumeric(30).All(char.IsLetterOrDigit));
        Assert.NotEqual(Generate.Token(), Generate.Token());
    }

    [Fact]
    public void Generate_Password_HasAllClasses()
    {
        var pwd = Generate.Password(16);
        Assert.Equal(16, pwd.Length);
        Assert.True(Password.HasUppercase(pwd));
        Assert.True(Password.HasLowercase(pwd));
        Assert.True(Password.HasDigit(pwd));
        Assert.True(Password.HasSymbol(pwd));
    }
}
