using System.Globalization;

namespace Tommunism.SoftFloat.Tests;

internal record struct TestRunnerArgument(UInt128 Value, TestRunnerArgumentKind Kind)
{
    private static ReadOnlySpan<byte> HexChars => "0123456789ABCDEF"u8;

    #region Additional Constructors

    public TestRunnerArgument(ExceptionFlags exceptionFlags) : this((byte)exceptionFlags, TestRunnerArgumentKind.Bits8) { }

    public TestRunnerArgument(bool value) : this(value ? 1U : 0, TestRunnerArgumentKind.Bits1) { }

    public TestRunnerArgument(byte value) : this(value, TestRunnerArgumentKind.Bits8) { }

    public TestRunnerArgument(ushort value) : this(value, TestRunnerArgumentKind.Bits16) { }

    public TestRunnerArgument(uint value) : this(value, TestRunnerArgumentKind.Bits32) { }

    public TestRunnerArgument(ulong value) : this(value, TestRunnerArgumentKind.Bits64) { }

    public TestRunnerArgument(sbyte value) : this((byte)value, TestRunnerArgumentKind.Bits8) { }

    public TestRunnerArgument(short value) : this((ushort)value, TestRunnerArgumentKind.Bits16) { }

    public TestRunnerArgument(int value) : this((uint)value, TestRunnerArgumentKind.Bits32) { }

    public TestRunnerArgument(long value) : this((ulong)value, TestRunnerArgumentKind.Bits64) { }

    public TestRunnerArgument(UInt128 value) : this(value, TestRunnerArgumentKind.Bits128) { }

    public TestRunnerArgument(Int128 value) : this((UInt128)value, TestRunnerArgumentKind.Bits128) { }

    public TestRunnerArgument(Float16 value) : this(value.ToUInt16Bits(), TestRunnerArgumentKind.Bits16) { }

    public TestRunnerArgument(Float32 value) : this(value.ToUInt32Bits(), TestRunnerArgumentKind.Bits32) { }

    public TestRunnerArgument(Float64 value) : this(value.ToUInt64Bits(), TestRunnerArgumentKind.Bits64) { }

    public TestRunnerArgument(ExtFloat80 value) : this(value.ToUInt128Bits(), TestRunnerArgumentKind.Bits80) { }

    public TestRunnerArgument(Float128 value) : this(value.ToUInt128Bits(), TestRunnerArgumentKind.Bits128) { }

    #endregion

    #region Convert To Methods (Checked)

    public ExceptionFlags ToExceptionFlags() => (ExceptionFlags)ToUInt8();

    public bool ToBoolean()
    {
        if (Kind != TestRunnerArgumentKind.Bits1)
            throw new InvalidOperationException($"Argument is not a {1}-bit value.");

        if ((Value >> 1) != 0)
            throw new InvalidOperationException("Value uses more than 1 bit.");

        return Value != UInt128.Zero;
    }

    public byte ToUInt8()
    {
        if (Kind != TestRunnerArgumentKind.Bits8)
            throw new InvalidOperationException($"Argument is not a {8}-bit value.");

        if ((Value >> 8) != 0)
            throw new InvalidOperationException($"Value uses more than {8} bits.");

        return (byte)Value;
    }

    public ushort ToUInt16()
    {
        if (Kind != TestRunnerArgumentKind.Bits16)
            throw new InvalidOperationException($"Argument is not a {16}-bit value.");

        if ((Value >> 16) != 0)
            throw new InvalidOperationException($"Value uses more than {16} bits.");

        return (ushort)Value;
    }

    public uint ToUInt32()
    {
        if (Kind != TestRunnerArgumentKind.Bits32)
            throw new InvalidOperationException($"Argument is not a {32}-bit value.");

        if ((Value >> 32) != 0)
            throw new InvalidOperationException($"Value uses more than {32} bits.");

        return (uint)Value;
    }

    public ulong ToUInt64()
    {
        if (Kind != TestRunnerArgumentKind.Bits64)
            throw new InvalidOperationException($"Argument is not a {64}-bit value.");

        if ((Value >> 64) != 0)
            throw new InvalidOperationException($"Value uses more than {64} bits.");

        return (ulong)Value;
    }

    public UInt128 ToUInt128()
    {
        if (Kind != TestRunnerArgumentKind.Bits128)
            throw new InvalidOperationException($"Argument is not a {128}-bit value.");

        return Value;
    }

    public sbyte ToInt8()
    {
        if (Kind != TestRunnerArgumentKind.Bits8)
            throw new InvalidOperationException($"Argument is not a {8}-bit value.");

        if ((Value >> 8) != 0)
            throw new InvalidOperationException($"Value uses more than {8} bits.");

        return (sbyte)(byte)Value;
    }

    public short ToInt16()
    {
        if (Kind != TestRunnerArgumentKind.Bits16)
            throw new InvalidOperationException($"Argument is not a {16}-bit value.");

        if ((Value >> 16) != 0)
            throw new InvalidOperationException($"Value uses more than {16} bits.");

        return (short)(ushort)Value;
    }

    public int ToInt32()
    {
        if (Kind != TestRunnerArgumentKind.Bits32)
            throw new InvalidOperationException($"Argument is not a {32}-bit value.");

        if ((Value >> 32) != 0)
            throw new InvalidOperationException($"Value uses more than {32} bits.");

        return (int)(uint)Value;
    }

    public long ToInt64()
    {
        if (Kind != TestRunnerArgumentKind.Bits64)
            throw new InvalidOperationException($"Argument is not a {64}-bit value.");

        if ((Value >> 64) != 0)
            throw new InvalidOperationException($"Value uses more than {64} bits.");

        return (long)(ulong)Value;
    }

    public Int128 ToInt128()
    {
        if (Kind != TestRunnerArgumentKind.Bits128)
            throw new InvalidOperationException($"Argument is not a {128}-bit value.");

        return (Int128)Value;
    }

    public Float16 ToFloat16()
    {
        if (Kind != TestRunnerArgumentKind.Bits16)
            throw new InvalidOperationException($"Argument is not a {16}-bit value.");

        if ((Value >> 16) != 0)
            throw new InvalidOperationException($"Value uses more than {16} bits.");

        return Float16.FromUIntBits((ushort)Value);
    }

    public Float32 ToFloat32()
    {
        if (Kind != TestRunnerArgumentKind.Bits32)
            throw new InvalidOperationException($"Argument is not a {32}-bit value.");

        if ((Value >> 32) != 0)
            throw new InvalidOperationException($"Value uses more than {32} bits.");

        return Float32.FromUIntBits((uint)Value);
    }

    public Float64 ToFloat64()
    {
        if (Kind != TestRunnerArgumentKind.Bits64)
            throw new InvalidOperationException($"Argument is not a {64}-bit value.");

        if ((Value >> 64) != 0)
            throw new InvalidOperationException($"Value uses more than {64} bits.");

        return Float64.FromUIntBits((ulong)Value);
    }

    public ExtFloat80 ToExtFloat80()
    {
        if (Kind != TestRunnerArgumentKind.Bits80)
            throw new InvalidOperationException($"Argument is not a {80}-bit value.");

        if ((Value >> 80) != 0)
            throw new InvalidOperationException($"Value uses more than {80} bits.");

        return ExtFloat80.FromUIntBits(Value);
    }

    public Float128 ToFloat128()
    {
        if (Kind != TestRunnerArgumentKind.Bits128)
            throw new InvalidOperationException($"Argument is not a {128}-bit value.");

        return Float128.FromUIntBits(Value);
    }

    #endregion

    // Parse hexadecimal encoded argument (format always used by generator output).
    public static TestRunnerArgument Parse(ReadOnlySpan<char> span)
    {
        if (!TryParse(span, out var value))
            throw new FormatException();

        return value;
    }

    // Parse encoded exception flags (format returned by verifier).
    public static TestRunnerArgument ParseExceptionFlags(ReadOnlySpan<char> span)
    {
        if (!TryParseExceptionFlags(span, out var value))
            throw new FormatException();

        return value;
    }

    // Parse encoded 16-bit floating point number (format returned by verifier).
    public static TestRunnerArgument ParseFloat16(ReadOnlySpan<char> span)
    {
        if (!TryParseFloat16(span, out var value))
            throw new FormatException();

        return value;
    }

    // Parse encoded 32-bit floating point number (format returned by verifier).
    public static TestRunnerArgument ParseFloat32(ReadOnlySpan<char> span)
    {
        if (!TryParseFloat32(span, out var value))
            throw new FormatException();

        return value;
    }

    // Parse encoded 64-bit floating point number (format returned by verifier).
    public static TestRunnerArgument ParseFloat64(ReadOnlySpan<char> span)
    {
        if (!TryParseFloat64(span, out var value))
            throw new FormatException();

        return value;
    }

    // Parse encoded 80-bit floating point number (format returned by verifier).
    public static TestRunnerArgument ParseExtFloat80(ReadOnlySpan<char> span)
    {
        if (!TryParseExtFloat80(span, out var value))
            throw new FormatException();

        return value;
    }

    // Parse encoded 128-bit floating point number (format returned by verifier).
    public static TestRunnerArgument ParseFloat128(ReadOnlySpan<char> span)
    {
        if (!TryParseFloat128(span, out var value))
            throw new FormatException();

        return value;
    }

    // Parse hexadecimal encoded argument (format always used by generator output).
    public static bool TryParse(ReadOnlySpan<char> span, out TestRunnerArgument value)
    {
        // Remove any white space.
        span = span.Trim();

        // TODO: How is boolean encoded?

        // Try to parse UInt128 value as hexadecimal.
        if (!UInt128.TryParse(span, NumberStyles.AllowHexSpecifier, null, out var bits))
        {
            value = default;
            return false;
        }

        // All inputs are hexadecimal, so figure out the argument kind based on the span length.
        var kind = span.Length switch
        {
            0 => TestRunnerArgumentKind.None, // probably not possible, currently
            1 => TestRunnerArgumentKind.Bits1,
            2 => TestRunnerArgumentKind.Bits8,
            4 => TestRunnerArgumentKind.Bits16,
            8 => TestRunnerArgumentKind.Bits32,
            16 => TestRunnerArgumentKind.Bits64,
            20 => TestRunnerArgumentKind.Bits80,
            32 => TestRunnerArgumentKind.Bits128,
            _ => (TestRunnerArgumentKind)(-1) // invalid
        };

        // Check for invalid kind.
        if (kind < 0)
        {
            value = default;
            return false;
        }

        // Extra boolean check.
        if (kind is TestRunnerArgumentKind.Bits1 && bits != UInt128.Zero && bits != UInt128.One)
        {
            value = default;
            return false;
        }

        value = new TestRunnerArgument(bits, kind);
        return true;
    }

    // Parse encoded exception flags (format returned by verifier).
    public static bool TryParseExceptionFlags(ReadOnlySpan<char> span, out TestRunnerArgument value)
    {
        var flags = ExceptionFlags.None;

        static bool TestFlag(ref ReadOnlySpan<char> span, ref ExceptionFlags flags, char code, ExceptionFlags flag)
        {
            span = span.TrimStart();
            if (span.Length <= 0)
                return false;

            if (span[0] == '.')
            {
                // flag not set
            }
            else if (span[0] == 'v')
            {
                // flag set
                flags |= flag;
            }
            else
            {
                // unexpected/invalid flag character
                return false;
            }

            span = span[1..];
            return true;
        }

        // Test flags in a specific order.
        if (!TestFlag(ref span, ref flags, 'v', ExceptionFlags.Invalid)) goto InvalidFormat;
        if (!TestFlag(ref span, ref flags, 'i', ExceptionFlags.Infinite)) goto InvalidFormat;
        if (!TestFlag(ref span, ref flags, 'o', ExceptionFlags.Overflow)) goto InvalidFormat;
        if (!TestFlag(ref span, ref flags, 'u', ExceptionFlags.Underflow)) goto InvalidFormat;
        if (!TestFlag(ref span, ref flags, 'x', ExceptionFlags.Inexact)) goto InvalidFormat;

        // Make sure nothing remains.
        span = span.TrimStart();
        if (span.Length <= 0)
            goto InvalidFormat;

        value = new TestRunnerArgument(flags);
        return true;

    InvalidFormat:
        value = default;
        return false;
    }

    // Parse encoded 16-bit floating point number (format returned by verifier).
    public static bool TryParseFloat16(ReadOnlySpan<char> span, out TestRunnerArgument value)
    {
        const int exponentBits = 5;
        const int significandBits = 10;

        var floatParts = TryParseFloat(span);
        if (!floatParts.HasValue)
            goto InvalidFormat;

        // Is the exponent too large?
        if ((floatParts.Value.Exponent >> exponentBits) != UInt128.Zero)
            goto InvalidFormat;

        // Is the significand too large?
        if ((floatParts.Value.Significand >> significandBits) != UInt128.Zero)
            goto InvalidFormat;

        // Combine parts into float bits.
        value = new TestRunnerArgument(floatParts.Value.Significand | (floatParts.Value.Exponent << significandBits)
            | ((floatParts.Value.Sign ? UInt128.One : UInt128.Zero) << (exponentBits + significandBits)),
            TestRunnerArgumentKind.Bits16);
        return true;

    InvalidFormat:
        value = default;
        return false;
    }

    // Parse encoded 32-bit floating point number (format returned by verifier).
    public static bool TryParseFloat32(ReadOnlySpan<char> span, out TestRunnerArgument value)
    {
        const int exponentBits = 8;
        const int significandBits = 23;

        var floatParts = TryParseFloat(span);
        if (!floatParts.HasValue)
            goto InvalidFormat;

        // Is the exponent too large?
        if ((floatParts.Value.Exponent >> exponentBits) != UInt128.Zero)
            goto InvalidFormat;

        // Is the significand too large?
        if ((floatParts.Value.Significand >> significandBits) != UInt128.Zero)
            goto InvalidFormat;

        // Combine parts into float bits.
        value = new TestRunnerArgument(floatParts.Value.Significand | (floatParts.Value.Exponent << significandBits)
            | ((floatParts.Value.Sign ? UInt128.One : UInt128.Zero) << (exponentBits + significandBits)),
            TestRunnerArgumentKind.Bits32);
        return true;

    InvalidFormat:
        value = default;
        return false;
    }

    // Parse encoded 64-bit floating point number (format returned by verifier).
    public static bool TryParseFloat64(ReadOnlySpan<char> span, out TestRunnerArgument value)
    {
        const int exponentBits = 11;
        const int significandBits = 52;

        var floatParts = TryParseFloat(span);
        if (!floatParts.HasValue)
            goto InvalidFormat;

        // Is the exponent too large?
        if ((floatParts.Value.Exponent >> exponentBits) != UInt128.Zero)
            goto InvalidFormat;

        // Is the significand too large?
        if ((floatParts.Value.Significand >> significandBits) != UInt128.Zero)
            goto InvalidFormat;

        // Combine parts into float bits.
        value = new TestRunnerArgument(floatParts.Value.Significand | (floatParts.Value.Exponent << significandBits)
            | ((floatParts.Value.Sign ? UInt128.One : UInt128.Zero) << (exponentBits + significandBits)),
            TestRunnerArgumentKind.Bits64);
        return true;

    InvalidFormat:
        value = default;
        return false;
    }

    // Parse encoded 80-bit floating point number (format returned by verifier).
    public static bool TryParseExtFloat80(ReadOnlySpan<char> span, out TestRunnerArgument value)
    {
        const int exponentBits = 15;
        const int significandBits = 64;

        var floatParts = TryParseFloat(span);
        if (!floatParts.HasValue)
            goto InvalidFormat;

        // Is the exponent too large?
        if ((floatParts.Value.Exponent >> exponentBits) != UInt128.Zero)
            goto InvalidFormat;

        // Is the significand too large?
        if ((floatParts.Value.Significand >> significandBits) != UInt128.Zero)
            goto InvalidFormat;

        // Combine parts into float bits.
        value = new TestRunnerArgument(floatParts.Value.Significand | (floatParts.Value.Exponent << significandBits)
            | ((floatParts.Value.Sign ? UInt128.One : UInt128.Zero) << (exponentBits + significandBits)),
            TestRunnerArgumentKind.Bits80);
        return true;

    InvalidFormat:
        value = default;
        return false;
    }

    // Parse encoded 128-bit floating point number (format returned by verifier).
    public static bool TryParseFloat128(ReadOnlySpan<char> span, out TestRunnerArgument value)
    {
        const int exponentBits = 15;
        const int significandBits = 112;

        var floatParts = TryParseFloat(span);
        if (!floatParts.HasValue)
            goto InvalidFormat;

        // Is the exponent too large?
        if ((floatParts.Value.Exponent >> exponentBits) != UInt128.Zero)
            goto InvalidFormat;

        // Is the significand too large?
        if ((floatParts.Value.Significand >> significandBits) != UInt128.Zero)
            goto InvalidFormat;

        // Combine parts into float bits.
        value = new TestRunnerArgument(floatParts.Value.Significand | (floatParts.Value.Exponent << significandBits)
            | ((floatParts.Value.Sign ? UInt128.One : UInt128.Zero) << (exponentBits + significandBits)),
            TestRunnerArgumentKind.Bits128);
        return true;

    InvalidFormat:
        value = default;
        return false;
    }

    // Parse generic floating point number (format returned by verifier for all floating point numbers). Bit lengths of parts are not checked. White space is only allowed between parts and part separators.
    internal static (bool Sign, UInt128 Exponent, UInt128 Significand)? TryParseFloat(ReadOnlySpan<char> span)
    {
        span = span.Trim();
        if (span.Length <= 0)
            return null;

        // Make sure required sign character is present.
        var sign = span[0] switch
        {
            '-' => -1,
            '+' => +1,
            _ => 0
        };
        if (sign == 0)
            return null;

        // Make sure required decimal point character is present.
        var decimalPointIndex = span.IndexOf('.');
        if (decimalPointIndex < 0)
            return null;

        // Make sure exponent is not empty.
        var exponentSpan = span[1..decimalPointIndex].Trim();
        if (exponentSpan.Length <= 0)
            return null;

        // Make sure significand is not empty.
        var significandSpan = span[(decimalPointIndex + 1)..].TrimStart();
        if (significandSpan.Length <= 0)
            return null;

        // Try to parse exponent.
        if (!UInt128.TryParse(exponentSpan, NumberStyles.AllowHexSpecifier, null, out var exponent))
            return null;

        // Try to parse significand.
        if (!UInt128.TryParse(significandSpan, NumberStyles.AllowHexSpecifier, null, out var significand))
            return null;

        return (sign < 0, exponent, significand);
    }

    public override string ToString()
    {
        var builder = new ValueStringBuilder(stackalloc char[128]);
        WriteTo(ref builder);
        return builder.ToString();
    }

    internal void WriteTo(ref ValueStringBuilder builder)
    {
        // The "kind" enumeration's values are always represented as the number of bits.
        // Round bit count up to the next nibble and convert to number of nibbles (hex characters).
        var hexCharCount = ((int)Kind + 3) / 4;

        // Reserve characters in builder for encoded hex data.
        var buffer = builder.AppendSpan(hexCharCount);
        for (int shift = (hexCharCount - 1) * 4, i = 0; shift >= 0; shift -= 4, i++)
        {
            var c = (int)(Value >> shift) & 0xF;
            buffer[i] = (char)HexChars[c];
        }
    }
}
