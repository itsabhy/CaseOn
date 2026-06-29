using System.Numerics;
using System.Text;

namespace CaseON.Internal;

/// <summary>
/// Internal binary-to-text codecs (Base32, Base58 and Ascii85) shared by the public
/// <see cref="CaseON.Encode"/> and <see cref="CaseON.Decode"/> modules.
/// </summary>
internal static class BaseCodec
{
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    private const string Base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    public static string Base32Encode(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder((bytes.Length + 4) / 5 * 8);
        int buffer = bytes[0];
        var next = 1;
        var bitsLeft = 8;

        while (bitsLeft > 0 || next < bytes.Length)
        {
            if (bitsLeft < 5)
            {
                if (next < bytes.Length)
                {
                    buffer <<= 8;
                    buffer |= bytes[next++] & 0xFF;
                    bitsLeft += 8;
                }
                else
                {
                    var pad = 5 - bitsLeft;
                    buffer <<= pad;
                    bitsLeft += pad;
                }
            }

            var index = 0x1F & (buffer >> (bitsLeft - 5));
            bitsLeft -= 5;
            builder.Append(Base32Alphabet[index]);
        }

        while (builder.Length % 8 != 0)
        {
            builder.Append('=');
        }

        return builder.ToString();
    }

    public static byte[] Base32Decode(string value)
    {
        var trimmed = value.TrimEnd('=');
        if (trimmed.Length == 0)
        {
            return [];
        }

        var output = new List<byte>(trimmed.Length * 5 / 8);
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var raw in trimmed)
        {
            var c = char.ToUpperInvariant(raw);
            var index = Base32Alphabet.IndexOf(c, StringComparison.Ordinal);
            if (index < 0)
            {
                throw new FormatException($"Invalid Base32 character '{raw}'.");
            }

            buffer <<= 5;
            buffer |= index & 0x1F;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                output.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                bitsLeft -= 8;
            }
        }

        return [.. output];
    }

    public static string Base58Encode(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        var leadingZeros = 0;
        while (leadingZeros < bytes.Length && bytes[leadingZeros] == 0)
        {
            leadingZeros++;
        }

        // Interpret the bytes as a big-endian unsigned integer.
        var value = new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
        var builder = new StringBuilder();
        while (value > 0)
        {
            value = BigInteger.DivRem(value, 58, out var remainder);
            builder.Insert(0, Base58Alphabet[(int)remainder]);
        }

        for (var i = 0; i < leadingZeros; i++)
        {
            builder.Insert(0, '1');
        }

        return builder.ToString();
    }

    public static byte[] Base58Decode(string value)
    {
        if (value.Length == 0)
        {
            return [];
        }

        BigInteger result = 0;
        foreach (var c in value)
        {
            var index = Base58Alphabet.IndexOf(c, StringComparison.Ordinal);
            if (index < 0)
            {
                throw new FormatException($"Invalid Base58 character '{c}'.");
            }

            result = (result * 58) + index;
        }

        var bytes = result.ToByteArray(isUnsigned: true, isBigEndian: true);

        var leadingZeros = 0;
        while (leadingZeros < value.Length && value[leadingZeros] == '1')
        {
            leadingZeros++;
        }

        if (leadingZeros == 0)
        {
            return bytes;
        }

        var output = new byte[leadingZeros + bytes.Length];
        Array.Copy(bytes, 0, output, leadingZeros, bytes.Length);
        return output;
    }

    public static string Ascii85Encode(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(bytes.Length * 5 / 4 + 4);
        Span<char> group = stackalloc char[5];
        var i = 0;
        while (i < bytes.Length)
        {
            var blockLength = Math.Min(4, bytes.Length - i);
            uint chunk = 0;
            for (var j = 0; j < 4; j++)
            {
                chunk <<= 8;
                if (j < blockLength)
                {
                    chunk |= bytes[i + j];
                }
            }

            if (blockLength == 4 && chunk == 0)
            {
                builder.Append('z');
            }
            else
            {
                for (var k = 4; k >= 0; k--)
                {
                    group[k] = (char)('!' + (int)(chunk % 85));
                    chunk /= 85;
                }

                builder.Append(group[..(blockLength + 1)]);
            }

            i += 4;
        }

        return builder.ToString();
    }

    public static byte[] Ascii85Decode(string value)
    {
        var output = new List<byte>(value.Length * 4 / 5);
        Span<int> block = stackalloc int[5];
        var count = 0;

        foreach (var c in value)
        {
            if (char.IsWhiteSpace(c))
            {
                continue;
            }

            if (c == 'z' && count == 0)
            {
                output.Add(0);
                output.Add(0);
                output.Add(0);
                output.Add(0);
                continue;
            }

            if (c is < '!' or > 'u')
            {
                throw new FormatException($"Invalid Ascii85 character '{c}'.");
            }

            block[count++] = c - '!';
            if (count == 5)
            {
                AppendBlock(output, block, 5);
                count = 0;
            }
        }

        if (count > 0)
        {
            for (var i = count; i < 5; i++)
            {
                block[i] = 84;
            }

            AppendBlock(output, block, count);
        }

        return [.. output];
    }

    private static void AppendBlock(List<byte> output, ReadOnlySpan<int> block, int count)
    {
        uint chunk = 0;
        for (var i = 0; i < 5; i++)
        {
            chunk = (chunk * 85) + (uint)block[i];
        }

        Span<byte> decoded = stackalloc byte[4];
        decoded[0] = (byte)(chunk >> 24);
        decoded[1] = (byte)(chunk >> 16);
        decoded[2] = (byte)(chunk >> 8);
        decoded[3] = (byte)chunk;

        for (var i = 0; i < count - 1; i++)
        {
            output.Add(decoded[i]);
        }
    }
}
