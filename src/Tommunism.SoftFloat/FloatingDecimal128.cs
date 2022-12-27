#region Copyright
// Ported to C# by Thomas Kaiser (2022).
// Note that this is under different licensing terms than the rest of the
// SoftFloat library, because it was ported from the Ryu source code.
// Original C Source Code: https://github.com/ulfjack/ryu

// Copyright 2018 Ulf Adams
//
// The contents of this file may be used under the terms of the Apache License,
// Version 2.0.
//
//    (See accompanying file LICENSE-Apache or copy at
//     http://www.apache.org/licenses/LICENSE-2.0)
//
// Alternatively, the contents of this file may be used under the terms of
// the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE-Boost or copy at
//     https://www.boost.org/LICENSE_1_0.txt)
//
// Unless required by applicable law or agreed to in writing, this software
// is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.
#endregion

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Tommunism.SoftFloat;

// NOTE: This requires .NET 7+, because it uses the UInt128 integer type.
// (This can be avoided, but currently SoftFloatCommonNaN uses it, so it's fine for now.)
// This also needs the UInt256M type, but only for the field storage (none of its operators are required).

/// <summary>
/// A floating decimal representing (-1)^s * m * 10^e.
/// </summary>
public readonly partial struct FloatingDecimal128 : ISpanFormattable, IEquatable<FloatingDecimal128>
{
    #region Fields

    /// <summary>
    /// If the exponent is equal to this value, then this represents a special floating-point value such as NaN or Infinity, which is
    /// determined by the mantissa.
    /// </summary>
    private const int ExceptionalExponent = int.MaxValue;

    private readonly UInt128 _mantissa;
    private readonly int _exponent;
    private readonly bool _sign;

    #endregion

    #region Constructors

    /// <summary>
    /// Converts the given bits into a generic floating point decimal representation.
    /// </summary>
    /// <param name="bits">The raw floating-point bits from the value.</param>
    /// <param name="mantissaBits">The number of bits required for the mantissa (significand or fraction) part of the value.</param>
    /// <param name="exponentBits">The number of bits required for the exponent part of the value.</param>
    /// <param name="explicitLeadingBit">Indicates whether there is an explicit leading bit in the mantissa. This is generally used for extended floating-point types (such as <see cref="ExtFloat80"/>).</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mantissaBits"/> is less than or equal to zero or greater than or equal to 128 or <paramref name="exponentBits"/> is less than or equal to zero or greater than 32.</exception>
    /// <exception cref="ArgumentException">The sum of <paramref name="exponentBits"/> and <paramref name="mantissaBits"/> is greater than or equal to 128.</exception>
    public FloatingDecimal128(UInt128 bits, int mantissaBits, int exponentBits, bool explicitLeadingBit)
    {
        if (mantissaBits is <= 0 or >= 128)
            throw new ArgumentOutOfRangeException(nameof(mantissaBits));
        if (exponentBits is <= 0 or > 32)
            throw new ArgumentOutOfRangeException(nameof(exponentBits));
        if (mantissaBits + exponentBits >= 128)
            throw new ArgumentException("Mantissa bits, exponent bits, and sign bit exceeds 128 bits.");

        uint bias = (1U << (exponentBits - 1)) - 1;
        bool ieeeSign = ((uint)(bits >> (mantissaBits + exponentBits)) & 1) != 0;
        UInt128 ieeeMantissa = bits & ((UInt128.One << mantissaBits) - 1);
        uint ieeeExponent = (uint)(bits >> mantissaBits) & (uint)((1UL << exponentBits) - 1);

        // Handle +/- zero.
        if (ieeeExponent == 0 && ieeeMantissa == 0)
        {
            _mantissa = 0;
            _exponent = 0;
            _sign = ieeeSign;
            return;
        }

        // Handle exceptional values (NaN and +/- Infinity).
        if (ieeeExponent == ((1UL << exponentBits) - 1))
        {
            _mantissa = explicitLeadingBit ? (ieeeMantissa & ((UInt128.One << (mantissaBits - 1)) - 1)) : ieeeMantissa;
            _exponent = ExceptionalExponent;
            _sign = ieeeSign;
            return;
        }

        int e2;
        UInt128 m2;

        // We subtract 2 in all cases so that the bounds computation has 2 additional bits.
        if (explicitLeadingBit)
        {
            // mantissaBits includes the explicit leading bit, so we need to correct for that here.
            e2 = (ieeeExponent == 0)
                ? (int)(1 - bias - mantissaBits + 1 - 2)
                : (int)(ieeeExponent - bias - mantissaBits + 1 - 2);
            m2 = ieeeMantissa;
        }
        else
        {
            if (ieeeExponent == 0)
            {
                e2 = (int)(1 - bias - mantissaBits - 2);
                m2 = ieeeMantissa;
            }
            else
            {
                e2 = (int)(ieeeExponent - bias - mantissaBits - 2);
                m2 = (UInt128.One << mantissaBits) | ieeeMantissa;
            }
        }

        bool even = ((uint)m2 & 1) == 0;
        bool acceptBounds = even;

        // Step 2: Determine the interval of legal decimal representations.
        UInt128 mv = m2 << 2; // multiply by 4
        uint mmShift = (ieeeMantissa != (explicitLeadingBit ? (UInt128.One << (mantissaBits - 1)) : 0) || ieeeExponent == 0) ? 1U : 0;

        // Step 3: Convert to a decimal power base using 128-bit arithmetic.
        UInt128 vr, vp, vm;
        int e10;
        bool vmIsTrailingZeros = false;
        bool vrIsTrailingZeros = false;
        if (e2 >= 0)
        {
            // I tried special-casing q == 0, but there was no effect on performance.
            // This expression is slightly faster than max(0, log10Pow2(e2) - 1).
            uint q = Log10Pow2(e2) - ((e2 > 3) ? 1U : 0);
            e10 = (int)q;
            int k = FLOAT_128_POW5_INV_BITCOUNT + (int)Pow5Bits((int)q) - 1;
            int i = -e2 + (int)q + k;
            UInt256M pow5 = ComputeInvPow5((int)q);
            vr = MultiplyAndShift(mv, pow5, i);
            vp = MultiplyAndShift(mv + 2, pow5, i);
            vm = MultiplyAndShift(mv - 1 - mmShift, pow5, i);

            // floor(log_5(2^128)) = 55, this is very conservative
            if (q <= 55)
            {
                // Only one of mp, mv, and mm can be a multiple of 5, if any.
                if ((mv % 5) == 0)
                {
                    vrIsTrailingZeros = MultipleOfPowerOf5(mv, q - 1);
                }
                else if (acceptBounds)
                {
                    // Same as min(e2 + (~mm & 1), pow5Factor(mm)) >= q
                    // <=> e2 + (~mm & 1) >= q && pow5Factor(mm) >= q
                    // <=> true && pow5Factor(mm) >= q, since e2 >= q.
                    vmIsTrailingZeros = MultipleOfPowerOf5(mv - 1 - mmShift, q);
                }
                else
                {
                    // Same as min(e2 + 1, pow5Factor(mp)) >= q.
                    if (MultipleOfPowerOf5(mv + 2, q))
                        vp--;
                }
            }
        }
        else
        {
            // This expression is slightly faster than max(0, log10Pow5(-e2) - 1).
            uint q = Log10Pow5(-e2) - ((-e2 > 1) ? 1U : 0);
            e10 = (int)q + e2;
            int i = -e2 - (int)q;
            int k = (int)Pow5Bits(i) - FLOAT_128_POW5_BITCOUNT;
            int j = (int)q - k;
            UInt256M pow5 = ComputePow5(i);
            vr = MultiplyAndShift(mv, pow5, j);
            vp = MultiplyAndShift(mv + 2, pow5, j);
            vm = MultiplyAndShift(mv - 1 - mmShift, pow5, j);

            if (q <= 1)
            {
                // {vr,vp,vm} is trailing zeros if {mv,mp,mm} has at least q trailing 0 bits.
                // mv = 4 m2, so it always has at least two trailing 0 bits.
                vrIsTrailingZeros = true;
                if (acceptBounds)
                {
                    // mm = mv - 1 - mmShift, so it has 1 trailing 0 bit iff mmShift == 1.
                    vmIsTrailingZeros = mmShift == 1;
                }
                else
                {
                    // mp = mv + 2, so it always has at least one trailing 0 bit.
                    --vp;
                }
            }
            else if (q < 127)
            {
                // We need to compute min(ntz(mv), pow5Factor(mv) - e2) >= q-1
                // <=> ntz(mv) >= q-1  &&  pow5Factor(mv) - e2 >= q-1
                // <=> ntz(mv) >= q-1    (e2 is negative and -e2 >= q)
                // <=> (mv & ((1 << (q-1)) - 1)) == 0
                // We also need to make sure that the left shift does not overflow.
                vrIsTrailingZeros = MultipleOfPowerOf2(mv, q - 1);
            }
        }

        // Step 4: Find the shortest decimal representation in the interval of legal representations.
        uint removed = 0;
        uint lastRemovedDigit = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static UInt128 DivRem(UInt128 left, UInt128 right, out uint remainder)
        {
            var (quotient, rem) = UInt128.DivRem(left, right);
            Debug.Assert(rem >= uint.MinValue && rem <= uint.MaxValue);
            remainder = (uint)rem;
            return quotient;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint DivRem2(UInt128 left, UInt128 right, out UInt128 quotient)
        {
            (quotient, var rem) = UInt128.DivRem(left, right);
            Debug.Assert(rem >= uint.MinValue && rem <= uint.MaxValue);
            return (uint)rem;
        }

        UInt128 ten = 10; // this is a constant
        UInt128 vpDiv10, vmDiv10, vrDiv10; // avoid performing the same division operations multiple times
        while ((vpDiv10 = vp / ten) > (vmDiv10 = DivRem(vm, ten, out uint vmMod10)))
        {
            vmIsTrailingZeros &= vmMod10 == 0;
            vrIsTrailingZeros &= lastRemovedDigit == 0;
            vrDiv10 = DivRem(vr, ten, out lastRemovedDigit);
            vr = vrDiv10;
            vp = vpDiv10;
            vm = vmDiv10;
            ++removed;
        }

        if (vmIsTrailingZeros)
        {
            // NOTE: The result of DivRem2 is the remainder, not the quotient!
            while (DivRem2(vm, ten, out vmDiv10) == 0)
            {
                vrIsTrailingZeros &= lastRemovedDigit == 0;
                vrDiv10 = DivRem(vr, ten, out lastRemovedDigit);
                vr = vrDiv10;
                vp /= ten;
                vm = vmDiv10;
                ++removed;
            }
        }

        if (vrIsTrailingZeros && lastRemovedDigit == 5 && ((uint)vr & 1) == 0)
        {
            // Round even if the exact numbers is .....50..0.
            lastRemovedDigit = 4;
        }

        _mantissa = vr;
        _exponent = e10 + (int)removed;
        _sign = ieeeSign;

        // We need to take vr+1 if vr is outside bounds or we need to round up.
        if ((vr == vm && (!acceptBounds || !vmIsTrailingZeros)) || lastRemovedDigit >= 5)
            _mantissa++;
    }

    /// <summary>
    /// Converts the given value into a generic floating point decimal representation.
    /// </summary>
    public FloatingDecimal128(Float16 value) : this(value.ToUInt16Bits(), 10, 5, false) { }

    /// <summary>
    /// Converts the given value into a generic floating point decimal representation.
    /// </summary>
    public FloatingDecimal128(Float32 value) : this(value.ToUInt32Bits(), 23, 8, false) { }

    /// <summary>
    /// Converts the given value into a generic floating point decimal representation.
    /// </summary>
    public FloatingDecimal128(Float64 value) : this(value.ToUInt64Bits(), 52, 11, false) { }

    /// <summary>
    /// Converts the given value into a generic floating point decimal representation.
    /// </summary>
    public FloatingDecimal128(ExtFloat80 value) : this(value.ToUInt128Bits(), 64, 15, true) { }

    /// <summary>
    /// Converts the given value into a generic floating point decimal representation.
    /// </summary>
    public FloatingDecimal128(Float128 value) : this(value.ToUInt128Bits(), 112, 15, false) { }

    /// <summary>
    /// Converts the given value into a generic floating point decimal representation.
    /// </summary>
    public FloatingDecimal128(Half value) : this(BitConverter.HalfToUInt16Bits(value), 10, 5, false) { }

    /// <summary>
    /// Converts the given value into a generic floating point decimal representation.
    /// </summary>
    public FloatingDecimal128(float value) : this(BitConverter.SingleToUInt32Bits(value), 23, 8, false) { }

    /// <summary>
    /// Converts the given value into a generic floating point decimal representation.
    /// </summary>
    public FloatingDecimal128(double value) : this(BitConverter.DoubleToUInt64Bits(value), 52, 11, false) { }

    #endregion

    #region Properties

    public UInt128 Mantissa => _mantissa;

    public int Exponent => _exponent;

    public bool IsFinite => _exponent != ExceptionalExponent;

    public bool IsNaN => _exponent == ExceptionalExponent && _mantissa != 0;

    public bool IsInfinity => _exponent == ExceptionalExponent && _mantissa == 0;

    #endregion

    #region Methods

    public override bool Equals(object? obj) => obj is FloatingDecimal128 value && Equals(value);

    public bool Equals(FloatingDecimal128 other) => _mantissa.Equals(other._mantissa) && _exponent == other._exponent && _sign == other._sign;

    public override int GetHashCode() => HashCode.Combine(_mantissa, _exponent, _sign);

    // NOTE: Only one format is currently supported here (an empty string/span or the exponential format code "E" or "e" with default precision).
    // NOTE: Even if it fails, there may have been characters written to the destination buffer.
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) =>
        TryFormat(destination, out charsWritten, format, new FormatInfoValues(provider));

    public override string ToString() => ToString(null, null);

    public string ToString(string? format) => ToString(format, null);

    public string ToString(IFormatProvider? formatProvider) => ToString(null, formatProvider);

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (format != null && !string.Equals(format, "E", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid/unsupported format specified.", nameof(format));

        var formatValues = new FormatInfoValues(formatProvider);

        // According to the Ryu source code, the maximum char buffer requirement is:
        // sign + mantissa digits + decimal dot + 'E' + exponent sign + exponent digits
        // = 1 + 39 + 1 + 1 + 1 + 10 = 53
        int maxStringLength =
            formatValues.NegativeSign.Length +
            formatValues.DecimalSeparator.Length +
            Math.Max(formatValues.NegativeSign.Length, formatValues.PositiveSign.Length) +
            39 + 1 + 10;

        // Just in case the format provider's symbols are longer than the max decimal string.
        if (maxStringLength < formatValues.NaNSymbol.Length)
            maxStringLength = formatValues.NaNSymbol.Length;
        if (maxStringLength < formatValues.PositiveInfinitySymbol.Length)
            maxStringLength = formatValues.PositiveInfinitySymbol.Length;
        if (maxStringLength < formatValues.NegativeInfinitySymbol.Length)
            maxStringLength = formatValues.NegativeInfinitySymbol.Length;

        Span<char> buffer = stackalloc char[maxStringLength];
        if (!TryFormat(buffer, out int length, format, formatValues))
            throw new FormatException();

        return new string(buffer[..length]);
    }

    private bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, FormatInfoValues formatValues)
    {
        // Default format code (if not specified).
        if (format.IsEmpty)
            format = "E";

        int length;
        switch (format[0])
        {
            case 'E':
            case 'e':
            {
                // This implementation currently does not support custom precision values.
                if (format.Length > 1)
                    goto default;

                length = FormatScientific(destination, formatValues, exponentSymbol: format[0]);
                break;
            }
            default:
            {
                throw new ArgumentException("Invalid/unsupported format specified.", nameof(format));
            }
        }

        if (length < 0)
        {
            charsWritten = default;
            return false;
        }

        charsWritten = length;
        return true;
    }

    // NOTE: If this returns -1, then it failed to format the value (likely means the destination buffer was too small).
    // NOTE: This always returns the equivalent of "Exponential (scientific)" format ("E" format). The exponent can be
    // omitted if optionalExponent is true and the calculated exponent value is zero.
    private int FormatScientific(Span<char> buffer, FormatInfoValues formatValues, bool optionalExponent = false, char exponentSymbol = 'E')
    {
        // Handle special floating-point values.
        if (_exponent == ExceptionalExponent)
        {
            var specialValue = (_mantissa != 0)
                ? formatValues.NaNSymbol
                : (_sign
                    ? formatValues.NegativeInfinitySymbol
                    : formatValues.PositiveInfinitySymbol);

            return specialValue.TryCopyTo(buffer) ? specialValue.Length : -1;
        }

        // Step 5: Print the decimal representation.
        int count = 0;
        if (_sign)
        {
            var negativeSign = formatValues.NegativeSign;
            if (!negativeSign.TryCopyTo(buffer))
                return -1;

            buffer = buffer[negativeSign.Length..];
            count += negativeSign.Length;
        }

        UInt128 output = _mantissa;
        int digitsLength = DecimalLength(output);
        Debug.Assert(digitsLength > 0);

        bool hasDecimalSeparator = digitsLength > 1;
        int outputLength = digitsLength;
        if (hasDecimalSeparator)
            outputLength += formatValues.DecimalSeparator.Length;

        if (buffer.Length < outputLength)
            return -1;

        UInt128 ten = 10; // constant
        for (int i = 1; i < digitsLength; ++i)
        {
            (output, var c) = UInt128.DivRem(output, ten);
            buffer[outputLength - i] = (char)('0' + (int)c);
        }

        Debug.Assert(output < ten, "Highest output digit is greater than or equal to 10!");
        buffer[0] = (char)('0' + (uint)output);

        // Print decimal point if needed (if there is more than one digit).
        if (hasDecimalSeparator)
        {
            formatValues.DecimalSeparator.CopyTo(buffer[1..]);
            buffer = buffer[outputLength..];
        }
        else
        {
            Debug.Assert(outputLength == 1);
            buffer = buffer[1..];
        }

        count += outputLength;

        // Print the exponent.
        int exp = _exponent + (digitsLength - 1);
        if (!optionalExponent || exp != 0)
        {
            if (buffer.Length <= 1)
                return -1;

            buffer[0] = exponentSymbol;
            buffer = buffer[1..];
            count++;

            if (!exp.TryFormat(buffer, out int expLength, default, formatValues.Info))
                return -1;

            count += expLength;
        }

        return count;
    }

    // Returns e == 0 ? 1 : ceil(log_2(5^e)); requires 0 <= e <= 32768.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Pow5Bits(int e)
    {
        Debug.Assert(e is >= 0 and <= (1 << 15));
        return (uint)((((uint)e * 163391164108059UL) >> 46) + 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 Multiply64x64(ulong a, ulong b)
    {
        // This should be faster than UInt128 multiplication especially if the internal hardware intrinsics can be used.
        ulong z64 = Math.BigMul(a, b, out ulong z0);
        return new(z64, z0);
    }

    private static UInt256M Multiply128By256Shift(UInt128 a, UInt256M b, int shift, uint correction)
    {
        Debug.Assert(shift is > 0 and < 256);

        ulong a0 = a.GetLowerUI64();
        UInt128 b00 = Multiply64x64(a0, b.V000); // 0
        UInt128 b01 = Multiply64x64(a0, b.V064); // 64
        UInt128 b02 = Multiply64x64(a0, b.V128); // 128
        UInt128 b03 = Multiply64x64(a0, b.V192); // 192
        ulong a1 = a.GetUpperUI64();
        UInt128 b10 = Multiply64x64(a1, b.V000); // 64
        UInt128 b11 = Multiply64x64(a1, b.V064); // 128
        UInt128 b12 = Multiply64x64(a1, b.V128); // 192
        UInt128 b13 = Multiply64x64(a1, b.V192); // 256

        UInt128 s0 = b00;                                       // 0   x
        UInt128 s1 = b01 + b10;                                 // 64  x
        UInt128 c1x = new((s1 < b01) ? 1U : 0, 0);              // 192 x
        UInt128 s2 = b02 + b11;                                 // 128 x
        UInt128 c2 = new(0, (s2 < b02) ? 1U : 0);               // 256 x
        UInt128 s3 = b03 + b12;                                 // 192 x
        UInt128 c3x = new((s3 < b03) ? 1U : 0, 0);              // 320

        UInt128 p0 = s0 + (s1 << 64);                               // 0
        UInt128 d0 = (p0 < b00) ? UInt128.One : UInt128.Zero;       // 128
        UInt128 q1 = s2 + (s1 >> 64) + (s3 << 64);                  // 128
        UInt128 d1 = (q1 < s2) ? UInt128.One : UInt128.Zero;        // 256
        UInt128 p1 = q1 + c1x + d0;                                 // 128
        UInt128 d2 = (p1 < q1) ? UInt128.One : UInt128.Zero;        // 256
        UInt128 p2 = b13 + (s3 >> 64) + c2 + c3x + d1 + d2;         // 256

        // Perform the shifts.
        UInt128 r0, r1;
        if (shift < 128)
        {
            r0 = (p0 >> shift) | (p1 << -shift);
            r1 = (p1 >> shift) | (p2 << -shift);
        }
        else if (shift == 128)
        {
            r0 = p1;
            r1 = p2;
        }
        else
        {
            r0 = (p1 >> (shift - 128)) | (p2 << -shift);
            r1 = p2 >> (shift - 128);
        }

        // Add the correction here instead of doing it above during the shift (no point inlining it, it's exactly the
        // same with the three different shift paths above).
        r0 += correction;
        if (r0 < correction)
            r1++;

        return new(r1, r0);
    }

    // Computes 5^i in the form required by Ryu, and returns the 256-bit result.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt256M ComputePow5(int i)
    {
        int base1 = i / POW5_TABLE_SIZE;
        int base2 = base1 * POW5_TABLE_SIZE;
        UInt256M mul = GENERIC_POW5_SPLIT(base1);

        if (i == base2)
            return mul;

        int offset = i - base2;
        UInt128 m = GENERIC_POW5_TABLE(offset);
        int delta = (int)Pow5Bits(i) - (int)Pow5Bits(base2);
        uint corr = GetPow5Error(i);
        return Multiply128By256Shift(m, mul, delta, corr);
    }

    // Computes 5^-i in the form required by Ryu, and returns the 256-bit result.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt256M ComputeInvPow5(int i)
    {
        int base1 = (i + POW5_TABLE_SIZE - 1) / POW5_TABLE_SIZE;
        int base2 = base1 * POW5_TABLE_SIZE;
        UInt256M mul = GENERIC_POW5_INV_SPLIT(base1); // 1/5^base2

        if (i == base2)
        {
            // None of the values in the split table have the first 64 bits set, so incrementing the "low" field is safe.
            mul.V000++;
            return mul;
        }

        int offset = base2 - i;
        UInt128 m = GENERIC_POW5_TABLE(offset); // 5^offset
        int delta = (int)Pow5Bits(base2) - (int)Pow5Bits(i);
        uint corr = GetInvPow5Error(i) + 1;
        return Multiply128By256Shift(m, mul, delta, corr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Pow5Factor(UInt128 value)
    {
        UInt128 five = 5; // constant
        for (uint count = 0; value > 0; ++count)
        {
            (value, var rem) = UInt128.DivRem(value, five);
            if (rem != 0)
                return count;
        }

        return 0;
    }

    // Returns true if value is divisible by 5^p.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MultipleOfPowerOf5(UInt128 value, uint p)
    {
        // I tried a case distinction on p, but there was no performance difference.
        return Pow5Factor(value) >= p;
    }

    // Returns true if value is divisible by 2^p.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MultipleOfPowerOf2(UInt128 value, uint p)
    {
        Debug.Assert(p is >= 0 and < 128);
        return (value & ((UInt128.One << (int)p) - 1)) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 MultiplyAndShift(UInt128 a, UInt256M b, int shift)
    {
        Debug.Assert(shift > 128);
        UInt256M result = Multiply128By256Shift(a, b, shift, 0);
        return new(result.V064, result.V000);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DecimalLength(UInt128 v)
    {
        UInt128 p10 = new(0x4B3B4CA85A86C47A, 0x98A224000000000); // LARGEST_POW10
        for (int i = 39; i > 0; i--)
        {
            if (v >= p10)
                return i;

            p10 /= 10;
        }

        return 1;
    }

    // Returns floor(log_10(2^e)).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Log10Pow2(int e)
    {
        // The first value this approximation fails for is 2^1651 which is just greater than 10^297.
        Debug.Assert(e is >= 0 and <= (1 << 15));
        return (uint)(((ulong)e * 169464822037455) >> 49);
    }

    // Returns floor(log_10(5^e)).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Log10Pow5(int e)
    {
        // The first value this approximation fails for is 5^2621 which is just greater than 10^1832.
        Debug.Assert(e is >= 0 and <= (1 << 15));
        return (uint)(((ulong)e * 196742565691928) >> 48);
    }

    public static bool operator ==(FloatingDecimal128 left, FloatingDecimal128 right) => left.Equals(right);

    public static bool operator !=(FloatingDecimal128 left, FloatingDecimal128 right) => !(left == right);

    #endregion

    #region Nested Types

    /// <summary>
    /// Used to cache common values from <see cref="NumberFormatInfo"/>.
    /// </summary>
    private sealed class FormatInfoValues
    {
        private readonly NumberFormatInfo _info;
        private string? _negativeSign;
        private string? _positiveSign;
        private string? _decimalSeparator;
        private string? _nanSymbol;
        private string? _positiveInfinitySymbol;
        private string? _negativeInfinitySymbol;

        public FormatInfoValues(IFormatProvider? provider) : this(NumberFormatInfo.GetInstance(provider)) { }

        public FormatInfoValues(NumberFormatInfo info) => _info = info;

        public NumberFormatInfo Info => _info;

        public string NegativeSign => _negativeSign ??= GetStringOrDefault(_info.NegativeSign, "-");
        public string PositiveSign => _positiveSign ??= GetStringOrDefault(_info.PositiveSign, "+");
        public string DecimalSeparator => _decimalSeparator ??= GetStringOrDefault(_info.NumberDecimalSeparator, ".");
        public string NaNSymbol => _nanSymbol ??= GetStringOrDefault(_info.NaNSymbol, "NaN");
        public string PositiveInfinitySymbol => _positiveInfinitySymbol ??= GetStringOrDefault(_info.PositiveInfinitySymbol, "Infinity");
        public string NegativeInfinitySymbol => _negativeInfinitySymbol ??= GetStringOrDefault(_info.NegativeInfinitySymbol, "-Infinity");

        // NOTE: This will use the default value is the given value is null or empty.
        private static string GetStringOrDefault(string? value, string defaultValue) =>
            string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    #endregion
}
