using CaseON;
using Xunit;

namespace CaseON.Tests;

public class TextTests
{
    [Theory]
    [InlineData("ThisIsAString", "this_is_a_string")]
    [InlineData("hello world", "hello_world")]
    [InlineData("alreadysnake", "alreadysnake")]
    public void ToSnakeCase_Converts(string input, string expected) =>
        Assert.Equal(expected, Text.ToSnakeCase(input));

    [Fact]
    public void ToSnakeCase_PreserveAcronyms_KeepsAcronymTogether()
    {
        Assert.Equal("http_server", Text.ToSnakeCase("HTTPServer", preserveAcronyms: true));
        Assert.Equal("h_t_t_p_server", Text.ToSnakeCase("HTTPServer"));
    }

    [Theory]
    [InlineData("ThisIsAString", "this-is-a-string")]
    public void ToKebabCase_Converts(string input, string expected) =>
        Assert.Equal(expected, Text.ToKebabCase(input));

    [Theory]
    [InlineData("this is a string", "ThisIsAString")]
    [InlineData("hello_world", "HelloWorld")]
    public void ToPascalCase_Converts(string input, string expected) =>
        Assert.Equal(expected, Text.ToPascalCase(input));

    [Theory]
    [InlineData("this is a string", "thisIsAString")]
    public void ToCamelCase_Converts(string input, string expected) =>
        Assert.Equal(expected, Text.ToCamelCase(input));

    [Fact]
    public void Casing_Variants_Work()
    {
        Assert.Equal("THIS_IS_A_STRING", Text.ToConstantCase("ThisIsAString"));
        Assert.Equal("this.is.a.string", Text.ToDotCase("ThisIsAString"));
        Assert.Equal("this/is/a/string", Text.ToSlashCase("ThisIsAString"));
        Assert.Equal("This-Is-A-String", Text.ToTrainCase("this is a string"));
        Assert.Equal("Content-Type", Text.ToHeaderCase("content type"));
        Assert.Equal("THIS-IS-A-STRING", Text.ToCobolCase("this is a string"));
        Assert.Equal("thisisastring", Text.ToFlatCase("This Is A String"));
        Assert.Equal("THISISASTRING", Text.ToUpperFlatCase("This Is A String"));
        Assert.Equal("This is a string", Text.ToSentenceCase("this IS a STRING"));
        Assert.Equal("This Is A String", Text.ToTitleCase("this is a string"));
    }

    [Fact]
    public void AlternatingAndInverse_Work()
    {
        Assert.Equal("aBcD", Text.ToAlternatingCase("abcd"));
        Assert.Equal("hELLO wORLD", Text.ToInverseCase("Hello World"));
    }

    [Theory]
    [InlineData("first_name", "First Name")]
    [InlineData("XMLHttpRequest", "XML Http Request")]
    public void Humanize_Works(string input, string expected) =>
        Assert.Equal(expected, Text.Humanize(input));

    [Fact]
    public void ToIdentifier_Works()
    {
        Assert.Equal("FirstName", Text.ToIdentifier("First Name"));
        Assert.Equal("_123Abc", Text.ToIdentifier("123 abc"));
    }

    [Fact]
    public void Truncate_Works()
    {
        Assert.Equal("Hello w\u2026", Text.Truncate("Hello world", 8));
        Assert.Equal("Hello world", Text.Truncate("Hello world", 50));
        Assert.Equal("one two\u2026", Text.TruncateWords("one two three four", 2));
        Assert.Equal("abc\u2026hij", Text.TruncateMiddle("abcdefghij", 7));
    }

    [Fact]
    public void Convert_Dispatcher_Works()
    {
        Assert.Equal("this_is_a_string", Text.Convert("ThisIsAString", CaseStyle.Snake));
        Assert.Equal("THISISASTRING", Text.Convert("This Is A String", CaseStyle.UpperFlat));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Casing_Throws_OnNullOrWhitespace(string? input) =>
        Assert.ThrowsAny<ArgumentException>(() => Text.ToSnakeCase(input!));
}
