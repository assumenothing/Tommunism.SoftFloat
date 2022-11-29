#region Copyright
/*============================================================================

This is a C# port of the SoftFloat library release 3e by Thomas Kaiser (2022).
The copyright from the original source code is listed below.

This C source file is part of the SoftFloat IEEE Floating-Point Arithmetic
Package, Release 3e, by John R. Hauser.

Copyright 2011, 2012, 2013, 2014, 2015, 2016, 2017, 2018 The Regents of the
University of California.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

 1. Redistributions of source code must retain the above copyright notice,
    this list of conditions, and the following disclaimer.

 2. Redistributions in binary form must reproduce the above copyright notice,
    this list of conditions, and the following disclaimer in the documentation
    and/or other materials provided with the distribution.

 3. Neither the name of the University nor the names of its contributors may
    be used to endorse or promote products derived from this software without
    specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS "AS IS", AND ANY
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE, ARE
DISCLAIMED.  IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

=============================================================================*/
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Tommunism.SoftFloat;

using static Primitives;
using static Internals;

// Improve Visual Studio's readability a little bit by "redefining" the standard integer types to C99 stdint types.

using int8_t = SByte;
using int16_t = Int16;
using int32_t = Int32;
using int64_t = Int64;

using uint8_t = Byte;
using uint16_t = UInt16;
using uint32_t = UInt32;
using uint64_t = UInt64;

// C# only has 32-bit & 64-bit integer operators by default, so just make these "fast" types 32 or 64 bits.
using int_fast8_t = Int32;
using int_fast16_t = Int32;
using int_fast32_t = Int32;
using int_fast64_t = Int64;
using uint_fast8_t = UInt32;
using uint_fast16_t = UInt32;
using uint_fast32_t = UInt32;
using uint_fast64_t = UInt64;

[StructLayout(LayoutKind.Sequential, Pack = sizeof(ushort), Size = sizeof(ushort))]
public readonly struct Float16
{
    #region Fields

    // WARNING: DO NOT ADD OR CHANGE ANY OF THESE FIELDS!!!
    private readonly ushort _v;

    #endregion

    #region Constructors

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used to avoid accidentally calling other overloads.")]
    private Float16(ushort v, bool dummy)
    {
        _v = v;
    }

    public Float16(Half value)
    {
        _v = BitConverter.HalfToUInt16Bits(value);
    }

    #endregion

    #region Methods

    public static explicit operator Float16(Half value) => new(value);
    public static implicit operator Half(Float16 value) => BitConverter.UInt16BitsToHalf(value._v);

    public static Float16 FromUIntBits(ushort value) => FromBitsUI16(value);

    public ushort ToUInt16Bits() => _v;

    // THIS IS THE INTERNAL CONSTRUCTOR FOR RAW BITS.
    // TODO: Allow value to be a full 32-bit integer (reduces total number of "unnecessary" casts).
    internal static Float16 FromBitsUI16(uint16_t v) => new(v, dummy: false);

    #region Integer-to-floating-point Conversions

    // NOTE: These operators use the default software floating-point state.
    public static explicit operator Float16(uint32_t a) => FromUInt32(a);
    public static explicit operator Float16(uint64_t a) => FromUInt64(a);
    public static explicit operator Float16(int32_t a) => FromInt32(a);
    public static explicit operator Float16(int64_t a) => FromInt64(a);

    // ui32_to_f16
    public static Float16 FromUInt32(uint32_t a, SoftFloatState? state = null)
    {
        var shiftDist = CountLeadingZeroes32(a) - 21;
        if (0 <= shiftDist)
            return FromBitsUI16(a != 0 ? PackToF16UI(false, 0x18 - shiftDist, a << shiftDist) : (uint16_t)0);

        shiftDist += 4;
        var sig = (shiftDist < 0)
            ? (a >> (-shiftDist) | (a << shiftDist))
            : (a << shiftDist);
        return RoundPackToF16(state ?? SoftFloatState.Default, false, 0x1C - shiftDist, sig);
    }

    // ui64_to_f16
    public static Float16 FromUInt64(uint64_t a, SoftFloatState? state = null)
    {
        var shiftDist = CountLeadingZeroes64(a) - 53;
        if (0 <= shiftDist)
            return FromBitsUI16(a != 0 ? PackToF16UI(false, 0x18 - shiftDist, (uint_fast16_t)a << shiftDist) : (uint16_t)0);

        shiftDist += 4;
        var sig = (shiftDist < 0)
            ? (uint_fast16_t)ShortShiftRightJam64(a, -shiftDist)
            : ((uint_fast16_t)a << shiftDist);
        return RoundPackToF16(state ?? SoftFloatState.Default, false, 0x1C - shiftDist, sig);

    }

    // i32_to_f16
    public static Float16 FromInt32(int32_t a, SoftFloatState? state = null)
    {
        var sign = a < 0;
        var absA = (uint_fast32_t)(sign ? -a : a);
        var shiftDist = CountLeadingZeroes32(absA) - 21;
        if (0 <= shiftDist)
            return FromBitsUI16(a != 0 ? PackToF16UI(sign, 0x18 - shiftDist, absA << shiftDist) : (uint16_t)0);

        shiftDist += 4;
        var sig = (shiftDist < 0)
            ? (absA >> (-shiftDist)) | ((absA << shiftDist) != 0 ? 1U : 0U)
            : (absA << shiftDist);
        return RoundPackToF16(state ?? SoftFloatState.Default, sign, 0x1C - shiftDist, sig);

    }

    // i64_to_f16
    public static Float16 FromInt64(int64_t a, SoftFloatState? state = null)
    {
        var sign = a < 0;
        var absA = (uint_fast64_t)(sign ? -a : a);
        var shiftDist = CountLeadingZeroes64(absA) - 53;
        if (0 <= shiftDist)
            return FromBitsUI16(a != 0 ? PackToF16UI(sign, 0x18 - shiftDist, (uint_fast16_t)absA << shiftDist) : (uint16_t)0);

        shiftDist += 4;
        var sig = (shiftDist < 0)
            ? (uint_fast16_t)ShortShiftRightJam64(absA, -shiftDist)
            : ((uint_fast16_t)absA << shiftDist);
        return RoundPackToF16(state ?? SoftFloatState.Default, sign, 0x1C - shiftDist, sig);
    }

    #endregion

    #region Floating-point-to-integer Conversions

    public uint32_t ToUInt32(bool exact, SoftFloatState state) => ToUInt32(state.RoundingMode, exact, state);

    public uint64_t ToUInt64(bool exact, SoftFloatState state) => ToUInt64(state.RoundingMode, exact, state);

    public int32_t ToInt32(bool exact, SoftFloatState state) => ToInt32(state.RoundingMode, exact, state);

    public int64_t ToInt64(bool exact, SoftFloatState state) => ToInt64(state.RoundingMode, exact, state);

    // f16_to_ui32
    public uint32_t ToUInt32(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, frac;
        bool sign;
        int_fast8_t exp, shiftDist;
        uint_fast32_t sig32;

        uiA = _v;
        sign = SignF16UI(uiA);
        exp = ExpF16UI(uiA);
        frac = FracF16UI(uiA);

        if (exp == 0x1F)
        {
            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return (frac != 0)
                ? state.UInt32FromNaN
                : state.UInt32FromOverflow(sign);
        }

        sig32 = frac;
        if (exp != 0)
        {
            sig32 |= 0x0400;
            shiftDist = exp - 0x19;
            if (0 <= shiftDist && !sign)
                return sig32 << shiftDist;

            shiftDist = exp - 0x0D;
            if (0 < shiftDist)
                sig32 <<= shiftDist;
        }

        state ??= SoftFloatState.Default;
        return RoundToUI32(state, sign, sig32, roundingMode, exact);
    }

    // f16_to_ui64
    public uint64_t ToUInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, frac;
        bool sign;
        int_fast8_t exp, shiftDist;
        uint_fast32_t sig32;

        uiA = _v;
        sign = SignF16UI(uiA);
        exp = ExpF16UI(uiA);
        frac = FracF16UI(uiA);

        if (exp == 0x1F)
        {
            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return (frac != 0)
                ? state.UInt64FromNaN
                : state.UInt64FromOverflow(sign);
        }

        sig32 = frac;
        if (exp != 0)
        {
            sig32 |= 0x0400;
            shiftDist = exp - 0x19;
            if (0 <= shiftDist && !sign)
                return sig32 << shiftDist;

            shiftDist = exp - 0x0D;
            if (0 < shiftDist)
                sig32 <<= shiftDist;
        }

        state ??= SoftFloatState.Default;
        return RoundToUI64(state, sign, sig32 >> 12, (uint_fast64_t)sig32 << 52, roundingMode, exact);
    }

    // f16_to_i32
    public int32_t ToInt32(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, frac;
        bool sign;
        int_fast8_t exp, shiftDist;
        int_fast32_t sig32;

        uiA = _v;
        sign = SignF16UI(uiA);
        exp = ExpF16UI(uiA);
        frac = FracF16UI(uiA);

        if (exp == 0x1F)
        {
            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return (frac != 0)
                ? state.Int32FromNaN
                : state.Int32FromOverflow(sign);
        }

        sig32 = (int_fast32_t)frac;
        if (exp != 0)
        {
            sig32 |= 0x0400;
            shiftDist = exp - 0x19;
            if (0 <= shiftDist)
            {
                sig32 <<= shiftDist;
                return sign ? -sig32 : sig32;
            }

            shiftDist = exp - 0x0D;
            if (0 < shiftDist)
                sig32 <<= shiftDist;
        }

        state ??= SoftFloatState.Default;
        return RoundToI32(state, sign, (uint_fast32_t)sig32, roundingMode, exact);
    }

    // f16_to_i64
    public int64_t ToInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, frac;
        bool sign;
        int_fast8_t exp, shiftDist;
        int_fast32_t sig32;

        uiA = _v;
        sign = SignF16UI(uiA);
        exp = ExpF16UI(uiA);
        frac = FracF16UI(uiA);

        if (exp == 0x1F)
        {
            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return (frac != 0)
                ? state.Int64FromNaN
                : state.Int64FromOverflow(sign);
        }

        sig32 = (int_fast32_t)frac;
        if (exp != 0)
        {
            sig32 |= 0x0400;
            shiftDist = exp - 0x19;
            if (0 <= shiftDist)
            {
                sig32 <<= shiftDist;
                return sign ? -sig32 : sig32;
            }

            shiftDist = exp - 0x0D;
            if (0 < shiftDist)
                sig32 <<= shiftDist;
        }

        state ??= SoftFloatState.Default;
        return RoundToI32(state, sign, (uint_fast32_t)sig32, roundingMode, exact);
    }

    // f16_to_ui32_r_minMag
    public uint32_t ToUInt32RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, frac;
        int_fast8_t exp, shiftDist;
        bool sign;
        uint_fast32_t alignedSig;

        uiA = _v;
        exp = ExpF16UI(uiA);
        frac = FracF16UI(uiA);

        shiftDist = exp - 0x0F;
        if (shiftDist < 0)
        {
            if (exact && ((uint_fast8_t)exp | frac) != 0)
                (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

            return 0;
        }

        sign = SignF16UI(uiA);
        if (sign || exp == 0x1F)
        {
            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return (frac != 0)
                ? state.UInt32FromNaN
                : state.UInt32FromOverflow(sign);
        }

        alignedSig = (frac | 0x0400) << shiftDist;
        if (exact && (alignedSig & 0x3FF) != 0)
            (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

        return alignedSig >> 10;
    }

    // f16_to_ui64_r_minMag
    public uint64_t ToUInt64RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, frac;
        int_fast8_t exp, shiftDist;
        bool sign;
        uint_fast32_t alignedSig;

        uiA = _v;
        exp = ExpF16UI(uiA);
        frac = FracF16UI(uiA);

        shiftDist = exp - 0x0F;
        if (shiftDist < 0)
        {
            if (exact && ((uint_fast8_t)exp | frac) != 0)
                (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

            return 0;
        }

        sign = SignF16UI(uiA);
        if (sign || exp == 0x1F)
        {
            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return (frac != 0)
                ? state.UInt64FromNaN
                : state.UInt64FromOverflow(sign);
        }

        alignedSig = (frac | 0x0400) << shiftDist;
        if (exact && (alignedSig & 0x3FF) != 0)
            (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

        return alignedSig >> 10;
    }

    // f16_to_i32_r_minMag
    public int32_t ToInt32RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, frac;
        bool sign;
        int_fast8_t exp, shiftDist;
        int_fast32_t alignedSig;

        uiA = _v;
        exp = ExpF16UI(uiA);
        frac = FracF16UI(uiA);

        shiftDist = exp - 0x0F;
        if (shiftDist < 0)
        {
            if (exact && ((uint_fast8_t)exp | frac) != 0)
                (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

            return 0;
        }

        sign = SignF16UI(uiA);
        if (exp == 0x1F)
        {
            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return (frac != 0)
                ? state.Int32FromNaN
                : state.Int32FromOverflow(sign);
        }

        alignedSig = (int_fast32_t)(frac | 0x0400) << shiftDist;
        if (exact && (alignedSig & 0x3FF) != 0)
            (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

        alignedSig >>= 10;
        return sign ? -alignedSig : alignedSig;
    }

    // f16_to_i64_r_minMag
    public int64_t ToInt64RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, frac;
        bool sign;
        int_fast8_t exp, shiftDist;
        int_fast32_t alignedSig;

        uiA = _v;
        exp = ExpF16UI(uiA);
        frac = FracF16UI(uiA);

        shiftDist = exp - 0x0F;
        if (shiftDist < 0)
        {
            if (exact && ((uint_fast8_t)exp | frac) != 0)
                (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

            return 0;
        }

        sign = SignF16UI(uiA);
        if (exp == 0x1F)
        {
            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return (frac != 0)
                ? state.Int64FromNaN
                : state.Int64FromOverflow(sign);
        }

        alignedSig = (int_fast32_t)(frac | 0x0400) << shiftDist;
        if (exact && (alignedSig & 0x3FF) != 0)
            (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

        alignedSig >>= 10;
        return sign ? -alignedSig : alignedSig;
    }

    #endregion

    #region Floating-point-to-floating-point Conversions

    // f16_to_f32
    public Float32 ToFloat32(SoftFloatState? state = null)
    {
        uint_fast16_t uiA, frac;
        bool sign;
        int_fast8_t exp;

        uiA = _v;
        sign = SignF16UI(uiA);
        exp = ExpF16UI(uiA);
        frac = FracF16UI(uiA);

        if (exp == 0x1F)
        {
            if (frac != 0)
            {
                state ??= SoftFloatState.Default;
                state.Float16BitsToCommonNaN(uiA, out var commonNaN);
                return state.CommonNaNToFloat32(in commonNaN);
            }

            return PackToF32(sign, 0xFF, 0);
        }
        else if (exp == 0)
        {
            if (frac == 0)
                return PackToF32(sign, 0, 0);

            (exp, frac) = NormSubnormalF16Sig(frac);
            exp--;
        }

        return PackToF32(sign, exp + 0x70, frac << 13);
    }

    // f16_to_f64
    public Float64 ToFloat64(SoftFloatState? state = null)
    {
        uint_fast16_t uiA, frac;
        bool sign;
        int_fast8_t exp;

        uiA = _v;
        sign = SignF16UI(uiA);
        exp = ExpF16UI(uiA);
        frac = FracF16UI(uiA);

        if (exp == 0x1F)
        {
            if (frac != 0)
            {
                state ??= SoftFloatState.Default;
                state.Float16BitsToCommonNaN(uiA, out var commonNaN);
                return state.CommonNaNToFloat64(in commonNaN);
            }

            return PackToF64(sign, 0x7FF, 0);
        }
        else if (exp == 0)
        {
            if (frac == 0)
                return PackToF64(sign, 0, 0);

            (exp, frac) = NormSubnormalF16Sig(frac);
            exp--;
        }

        return PackToF64(sign, exp + 0x3F0, (uint_fast64_t)frac << 42);
    }

    // f16_to_extF80
    public ExtFloat80 ToExtFloat80(SoftFloatState? state = null)
    {
        uint_fast16_t uiA, frac;
        bool sign;
        int_fast8_t exp;

        uiA = _v;
        sign = SignF16UI(uiA);
        exp = ExpF16UI(uiA);
        frac = FracF16UI(uiA);

        if (exp == 0x1F)
        {
            if (frac != 0)
            {
                state ??= SoftFloatState.Default;
                state.Float16BitsToCommonNaN(uiA, out var commonNaN);
                return state.CommonNaNToExtFloat80(in commonNaN);
            }

            return PackToExtF80(sign, 0x7FFF, 0x8000000000000000);
        }
        else if (exp == 0)
        {
            if (frac == 0)
                return PackToExtF80(sign, 0, 0);

            (exp, frac) = NormSubnormalF16Sig(frac);
        }

        return PackToExtF80(sign, exp + 0x3FF0, (uint_fast64_t)(frac | 0x0400) << 53);
    }

    // f16_to_f128
    public Float128 ToFloat128(SoftFloatState? state = null)
    {
        uint_fast16_t uiA, frac;
        bool sign;
        int_fast8_t exp;

        uiA = _v;
        sign = SignF16UI(uiA);
        exp = ExpF16UI(uiA);
        frac = FracF16UI(uiA);

        if (exp == 0x1F)
        {
            if (frac != 0)
            {
                state ??= SoftFloatState.Default;
                state.Float16BitsToCommonNaN(uiA, out var commonNaN);
                return state.CommonNaNToFloat128(in commonNaN);
            }

            return PackToF128(sign, 0x7FFF, 0, 0);
        }
        else if (exp == 0)
        {
            if (frac == 0)
                return PackToF128(sign, 0, 0, 0);

            (exp, frac) = NormSubnormalF16Sig(frac);
            exp--;
        }

        return PackToF128(sign, exp + 0x3FF0, (uint_fast64_t)frac << 38, 0);
    }

    #endregion

    #region Arithmetic Operations

    public Float16 RoundToInt(bool exact, SoftFloatState state) => RoundToInt(state.RoundingMode, exact, state);

    // f16_roundToInt
    public Float16 RoundToInt(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, uiZ, lastBitMask, roundBitsMask;
        int_fast8_t exp;

        uiA = _v;
        exp = ExpF16UI(uiA);

        if (exp <= 0xE)
        {
            if ((uint16_t)(uiA << 1) == 0)
                return this;

            if (exact)
                (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

            uiZ = uiA & PackToF16UI(true, 0, 0);
            switch (roundingMode)
            {
                case RoundingMode.NearEven:
                {
                    if (FracF16UI(uiA) != 0)
                        goto case RoundingMode.NearMaxMag;

                    break;
                }
                case RoundingMode.NearMaxMag:
                {
                    if (exp == 0xE)
                        uiZ |= PackToF16UI(false, 0xF, 0);

                    break;
                }
                case RoundingMode.Min:
                {
                    if (uiZ != 0)
                        uiZ = PackToF16UI(true, 0xF, 0);

                    break;
                }
                case RoundingMode.Max:
                {
                    if (uiZ == 0)
                        uiZ = PackToF16UI(false, 0xF, 0);

                    break;
                }
                case RoundingMode.Odd:
                {
                    uiZ |= PackToF16UI(false, 0xF, 0);
                    break;
                }
            }

            return FromBitsUI16((ushort)uiZ);
        }

        if (0x19 <= exp)
        {
            return exp == 0x1F && FracF16UI(uiA) != 0
                ? (state ?? SoftFloatState.Default).PropagateNaNFloat16(uiA, 0)
                : this;
        }

        uiZ = uiA;
        lastBitMask = 1U << (0x19 - exp);
        roundBitsMask = lastBitMask - 1;
        if (roundingMode == RoundingMode.NearMaxMag)
        {
            uiZ += lastBitMask >> 1;
        }
        else if (roundingMode == RoundingMode.NearEven)
        {
            uiZ += lastBitMask >> 1;
            if ((uiZ & roundBitsMask) == 0)
                uiZ &= ~lastBitMask;
        }
        else if (roundingMode == (SignF16UI(uiZ) ? RoundingMode.Min : RoundingMode.Max))
        {
            uiZ += roundBitsMask;
        }

        uiZ &= ~roundBitsMask;
        if (uiZ != uiA)
        {
            if (roundingMode == RoundingMode.Odd)
                uiZ |= lastBitMask;

            if (exact)
                (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;
        }

        return FromBitsUI16((ushort)uiZ);
    }

    // f16_add
    public static Float16 Add(Float16 a, Float16 b, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, uiB;

        uiA = a._v;
        uiB = b._v;

        state ??= SoftFloatState.Default;
        return SignF16UI(uiA ^ uiB)
            ? SubMagsF16(state, uiA, uiB)
            : AddMagsF16(state, uiA, uiB);
    }

    // f16_sub
    public static Float16 Subtract(Float16 a, Float16 b, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, uiB;

        uiA = a._v;
        uiB = b._v;

        state ??= SoftFloatState.Default;
        return SignF16UI(uiA ^ uiB)
            ? AddMagsF16(state, uiA, uiB)
            : AddMagsF16(state, uiA, uiB);
    }

    // f16_mul
    public static Float16 Multiply(Float16 a, Float16 b, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, sigA, uiB, sigB, sigZ;
        int_fast8_t expA, expB, expZ;
        uint_fast32_t sig32Z;
        bool signA, signB, signZ;

        uiA = a._v;
        signA = SignF16UI(uiA);
        expA = ExpF16UI(uiA);
        sigA = FracF16UI(uiA);
        uiB = b._v;
        signB = SignF16UI(uiB);
        expB = ExpF16UI(uiB);
        sigB = FracF16UI(uiB);
        signZ = signA ^ signB;

        if (expA == 0x1F)
        {
            if (sigA != 0 || ((expB == 0x1F) && sigB != 0))
            {
                state ??= SoftFloatState.Default;
                return state.PropagateNaNFloat16(uiA, uiB);
            }

            if (((uint_fast8_t)expB | sigB) == 0)
            {
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
                return state.DefaultNaNFloat16;
            }

            return PackToF16(signZ, 0x1F, 0);
        }
        else if (expB == 0x1F)
        {
            if (sigB != 0)
            {
                state ??= SoftFloatState.Default;
                return state.PropagateNaNFloat16(uiA, uiB);
            }

            if (((uint_fast8_t)expA | sigA) == 0)
            {
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
                return state.DefaultNaNFloat16;
            }

            return PackToF16(signZ, 0x1F, 0);
        }

        if (expA == 0)
        {
            if (sigA == 0)
                return PackToF16(signZ, 0, 0);

            (expA, sigA) = NormSubnormalF16Sig(sigA);
        }

        if (expB == 0)
        {
            if (sigB == 0)
                return PackToF16(signZ, 0, 0);

            (expB, sigB) = NormSubnormalF16Sig(sigB);
        }

        expZ = expA + expB - 0xF;
        sigA = (sigA | 0x0400) << 4;
        sigB = (sigB | 0x0400) << 5;
        sig32Z = sigA * sigB;
        sigZ = sig32Z >> 16;
        if ((sig32Z & 0xFFFF) != 0)
            sigZ |= 1;

        if (sigZ < 0x4000)
        {
            --expZ;
            sigZ <<= 1;
        }

        return RoundPackToF16(state ?? SoftFloatState.Default, signZ, expZ, sigZ);
    }

    // f16_mulAdd
    public static Float16 MultiplyAndAdd(Float16 a, Float16 b, Float16 c, SoftFloatState? state = null)
    {
        state ??= SoftFloatState.Default;
        return MulAddF16(state, a._v, b._v, c._v, MulAdd.None);
    }

    // f16_div
    public static Float16 Divide(Float16 a, Float16 b, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, sigA, uiB, sigB, sig32A, sigZ;
        int_fast8_t expA, expB, expZ;
        bool signA, signB, signZ;

        uiA = a._v;
        signA = SignF16UI(uiA);
        expA = ExpF16UI(uiA);
        sigA = FracF16UI(uiA);
        uiB = b._v;
        signB = SignF16UI(uiB);
        expB = ExpF16UI(uiB);
        sigB = FracF16UI(uiB);
        signZ = signA ^ signB;

        if (expA == 0x1F)
        {
            if (sigA != 0)
            {
                state ??= SoftFloatState.Default;
                return state.PropagateNaNFloat16(uiA, uiB);
            }

            if (expB == 0x1F)
            {
                state ??= SoftFloatState.Default;
                if (sigB != 0)
                    return state.PropagateNaNFloat16(uiA, uiB);

                state.RaiseFlags(ExceptionFlags.Invalid);
                return state.DefaultNaNFloat16;
            }

            return PackToF16(signZ, 0x1F, 0);
        }
        else if (expB == 0x1F)
        {
            if (sigB != 0)
            {
                state ??= SoftFloatState.Default;
                return state.PropagateNaNFloat16(uiA, uiB);
            }

            return PackToF16(signZ, 0, 0);
        }

        if (expB == 0)
        {
            if (sigB == 0)
            {
                state ??= SoftFloatState.Default;
                if (((uint_fast8_t)expA | sigA) == 0)
                {
                    state.RaiseFlags(ExceptionFlags.Invalid);
                    return state.DefaultNaNFloat16;
                }

                state.RaiseFlags(ExceptionFlags.Infinite);
                return PackToF16(signZ, 0x1F, 0);
            }

            (expB, sigB) = NormSubnormalF16Sig(sigB);
        }

        if (expA == 0)
        {
            if (sigA == 0)
                return PackToF16(signZ, 0, 0);

            (expA, sigA) = NormSubnormalF16Sig(sigA);
        }

        expZ = expA - expB + 0xE;
        sigA |= 0x0400;
        sigB |= 0x0400;

        if (sigA < sigB)
        {
            --expZ;
            sig32A = sigA << 15;
        }
        else
        {
            sig32A = sigA << 14;
        }

        sigZ = sig32A / sigB;
        if ((sigZ & 7) == 0)
            sigZ |= (sigB * sigZ != sig32A) ? 1U : 0;

        state ??= SoftFloatState.Default;
        return RoundPackToF16(state, signZ, expZ, sigZ);
    }

    // f16_rem
    public static Float16 Modulus(Float16 a, Float16 b, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, sigA, uiB, sigB, q;
        int_fast8_t expA, expB, expDiff;
        uint16_t rem, altRem, meanRem;
        uint32_t recip32, q32;
        bool signA, signRem;

        uiA = a._v;
        signA = SignF16UI(uiA);
        expA = ExpF16UI(uiA);
        sigA = FracF16UI(uiA);
        uiB = b._v;
        expB = ExpF16UI(uiB);
        sigB = FracF16UI(uiB);

        if (expA == 0x1F)
        {
            state ??= SoftFloatState.Default;
            if (sigA != 0 || (expB == 0x1F && sigB != 0))
                return state.PropagateNaNFloat16(uiA, uiB);

            state.RaiseFlags(ExceptionFlags.Invalid);
            return state.DefaultNaNFloat16;
        }
        else if (expB == 0x1F)
        {
            if (sigB != 0)
            {
                state ??= SoftFloatState.Default;
                return state.PropagateNaNFloat16(uiA, uiB);
            }

            return a;
        }

        if (expB == 0)
        {
            if (sigB == 0)
            {
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
                return state.DefaultNaNFloat16;
            }

            (expB, sigB) = NormSubnormalF16Sig(sigB);
        }

        if (expA == 0)
        {
            if (sigA == 0)
                return a;

            (expA, sigA) = NormSubnormalF16Sig(sigA);
        }

        rem = (ushort)(sigA | 0x0400);
        sigB |= 0x0400;
        expDiff = expA - expB;
        if (expDiff < 0)
        {
            if (expDiff < -1)
                return a;

            sigB <<= 3;
            if (expDiff != 0)
            {
                rem <<= 2;
                q = 0;
            }
            else
            {
                rem <<= 3;
                q = sigB <= rem ? 1U : 0;
                if (q != 0)
                    rem -= (uint16_t)sigB;
            }
        }
        else
        {
            recip32 = ApproxRecip32_1(sigB << 21);

            // Changing the shift of 'rem' here requires also changing the initial subtraction from 'expDiff'.
            rem <<= 4;
            expDiff -= 31;

            // The scale of 'sigB' affects how many bits are obtained during each cycle of the loop. Currently this is 29 bits per loop
            // iteration, which is believed to be the maximum possible.
            sigB <<= 3;
            while (true)
            {
                q32 = (uint32_t)((rem * (uint_fast64_t)recip32) >> 16);
                if (expDiff < 0)
                    break;

                rem = (uint16_t)(-(int32_t)(q32 * sigB));
                expDiff -= 29;
            }

            // ('expDiff' cannot be less than -30 here.)
            q32 >>= ~expDiff & 31;
            q = q32;
            rem = (uint16_t)((uint32_t)(rem << (expDiff + 30)) - q * sigB);
        }

        do
        {
            altRem = rem;
            ++q;
            rem -= (uint16_t)sigB;
        }
        while ((rem & 0x8000) == 0);

        meanRem = (uint16_t)(rem + altRem);
        if ((meanRem & 0x8000) != 0 || (meanRem == 0 && (q & 1) != 0))
            rem = altRem;

        signRem = signA;
        if (0x8000 <= rem)
        {
            signRem = !signRem;
            rem = (uint16_t)(-(int16_t)rem);
        }

        state ??= SoftFloatState.Default;
        return NormRoundPackToF16(state, signRem, expB, rem);
    }

    // f16_sqrt
    public Float16 SquareRoot(SoftFloatState? state = null)
    {
        uint_fast16_t uiA, sigA, r0, recipSqrt16, sigZ, shiftedSigZ;
        int_fast8_t expA, expZ;
        int index;
        uint_fast32_t ESqrR0;
        uint16_t sigma0, negRem;
        bool signA;

        uiA = _v;
        signA = SignF16UI(uiA);
        expA = ExpF16UI(uiA);
        sigA = FracF16UI(uiA);

        if (expA == 0x1F)
        {
            if (sigA != 0)
            {
                state ??= SoftFloatState.Default;
                return state.PropagateNaNFloat16(uiA, 0);
            }

            if (!signA)
                return this;

            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return state.DefaultNaNFloat16;
        }

        if (signA)
        {
            if (((uint_fast8_t)expA | sigA) == 0)
                return this;

            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return state.DefaultNaNFloat16;
        }

        if (expA == 0)
        {
            if (sigA == 0)
                return this;

            (expA, sigA) = NormSubnormalF16Sig(sigA);
        }

        expZ = ((expA - 0xF) >> 1) + 0xE;
        expA &= 1;
        sigA |= 0x0400;
        index = (int_fast8_t)(sigA >> 6 & 0xE) + expA;
        r0 = ApproxRecipSqrt_1k0s[index] - ((ApproxRecipSqrt_1k1s[index] * (sigA & 0x7F)) >> 11);
        ESqrR0 = (r0 * r0) >> 1;
        if (expA != 0)
            ESqrR0 >>= 1;

        sigma0 = (uint16_t)~((ESqrR0 * sigA) >> 16);
        recipSqrt16 = r0 + ((r0 * sigma0) >> 25);
        if ((recipSqrt16 & 0x8000) == 0)
            recipSqrt16 = 0x8000;

        sigZ = ((sigA << 5) * recipSqrt16) >> 16;
        if (expA != 0)
            sigZ >>= 1;

        ++sigZ;
        if ((sigZ & 7) == 0)
        {
            shiftedSigZ = sigZ >> 1;
            negRem = (uint16_t)(shiftedSigZ * shiftedSigZ);
            sigZ &= ~1U;
            if ((negRem & 0x8000) != 0)
            {
                sigZ |= 1;
            }
            else
            {
                if (negRem != 0)
                    --sigZ;
            }
        }

        state ??= SoftFloatState.Default;
        return RoundPackToF16(state, false, expZ, sigZ);
    }

    #endregion

    #region Comparison Operations

    // f16_eq (quiet=true) & f16_eq_signaling (quiet=false)
    public static bool CompareEqual(Float16 a, Float16 b, bool quiet, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, uiB;

        uiA = a._v;
        uiB = b._v;

        if (IsNaNF16UI(uiA) || IsNaNF16UI(uiB))
        {
            state ??= SoftFloatState.Default;
            if (!quiet || state.IsSignalNaNFloat16Bits(uiA) || state.IsSignalNaNFloat16Bits(uiB))
                state.RaiseFlags(ExceptionFlags.Invalid);

            return false;
        }

        return (uiA == uiB) || (uint16_t)((uiA | uiB) << 1) == 0;
    }

    // f16_le (quiet=false) & f16_le_quiet (quiet=true)
    public static bool CompareLessThanOrEqual(Float16 a, Float16 b, bool quiet, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, uiB;
        bool signA, signB;

        uiA = a._v;
        uiB = b._v;

        if (IsNaNF16UI(uiA) || IsNaNF16UI(uiB))
        {
            state ??= SoftFloatState.Default;
            if (!quiet || state.IsSignalNaNFloat16Bits(uiA) || state.IsSignalNaNFloat16Bits(uiB))
                state.RaiseFlags(ExceptionFlags.Invalid);

            return false;
        }

        signA = SignF16UI(uiA);
        signB = SignF16UI(uiB);

        return (signA != signB)
            ? (signA || (uint16_t)((uiA | uiB) << 1) == 0)
            : (uiA == uiB || (signA ^ (uiA < uiB)));
    }

    // f16_lt (quiet=false) & f16_lt_quiet (quiet=true)
    public static bool CompareLessThan(Float16 a, Float16 b, bool quiet, SoftFloatState? state = null)
    {
        uint_fast16_t uiA, uiB;
        bool signA, signB;

        uiA = a._v;
        uiB = b._v;

        if (IsNaNF16UI(uiA) || IsNaNF16UI(uiB))
        {
            state ??= SoftFloatState.Default;
            if (!quiet || state.IsSignalNaNFloat16Bits(uiA) || state.IsSignalNaNFloat16Bits(uiB))
                state.RaiseFlags(ExceptionFlags.Invalid);

            return false;
        }

        signA = SignF16UI(uiA);
        signB = SignF16UI(uiB);

        return (signA != signB)
            ? (signA && (uint16_t)((uiA | uiB) << 1) != 0)
            : (uiA != uiB && (signA ^ (uiA < uiB)));
    }

    #endregion

    #endregion
}
