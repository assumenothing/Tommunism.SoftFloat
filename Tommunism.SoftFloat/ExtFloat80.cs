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
using System.Diagnostics;
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

[StructLayout(LayoutKind.Sequential)]
public readonly struct ExtFloat80
{
    #region Fields

    // WARNING: DO NOT ADD OR CHANGE ANY OF THESE FIELDS!!!

    /// <summary>
    /// The complete 64-bit significand of the floating-point value.
    /// </summary>
    private readonly ulong _signif; // _v0

    /// <summary>
    /// The sign and exponent of the floating-point value, with the sign in the most significant bit (bit 15) and the encoded exponent in the other 15 bits.
    /// </summary>
    private readonly ushort _signExp; // _v64

    #endregion

    #region Constructors

    internal ExtFloat80(ushort signExp, ulong signif)
    {
        _signif = signif;
        _signExp = signExp;
    }

    #endregion

    #region Properties

    internal uint64_t Significand => _signif;

    internal uint16_t SignAndExponent => _signExp;

    #endregion

    #region Methods

    public static ExtFloat80 FromUIntBits(ushort signExp, ulong significand) => new(signExp, significand);

    public (ushort signExp, ulong significant) ToUIntBits() => (_signExp, _signif);

#if NET7_0_OR_GREATER
    public static ExtFloat80 FromUIntBits(UInt128 value)
    {
        if ((value >> 80) != UInt128.Zero)
            throw new ArgumentOutOfRangeException(nameof(value), "ExtFloat80 cannot exceed an 80-bit integer.");

        return new((ushort)value.GetUpperUI64(), value.GetLowerUI64());
    }

    public UInt128 ToUInt128Bits() => new(_signExp, _signif);
#endif

    // THIS IS THE INTERNAL CONSTRUCTOR FOR RAW BITS.
    internal static ExtFloat80 FromBitsUI128(SFUInt128 v)
    {
        Debug.Assert((v.V64 & ~0xFFFFU) == 0);
        return FromBitsUI80((ushort)v.V64, v.V00);
    }

    // THIS IS THE INTERNAL CONSTRUCTOR FOR RAW BITS.
    // TODO: Allow signExp to be a full 32-bit integer (reduces total number of "unnecessary" casts).
    internal static ExtFloat80 FromBitsUI80(ushort signExp, ulong signif) => new(signExp, signif);

    #region Integer-to-floating-point Conversions

    // ui32_to_extF80
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static ExtFloat80 FromUInt32(SoftFloatContext context, uint32_t a)
    {
        uint_fast16_t uiZ64;
        if (a != 0)
        {
            var shiftDist = CountLeadingZeroes32(a);
            uiZ64 = 0x401E - (uint_fast16_t)shiftDist;
            a <<= shiftDist;
        }
        else
        {
            uiZ64 = 0;
        }

        return FromBitsUI80(signExp: (ushort)uiZ64, signif: (uint_fast64_t)a << 32);
    }

    // ui64_to_extF80
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static ExtFloat80 FromUInt64(SoftFloatContext context, uint64_t a)
    {
        uint_fast16_t uiZ64;
        if (a != 0)
        {
            var shiftDist = CountLeadingZeroes64(a);
            uiZ64 = 0x401E - (uint_fast16_t)shiftDist;
            a <<= shiftDist;
        }
        else
        {
            uiZ64 = 0;
        }

        return FromBitsUI80(signExp: (ushort)uiZ64, signif: a << 32);
    }

    // i32_to_extF80
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static ExtFloat80 FromInt32(SoftFloatContext context, int32_t a)
    {
        uint_fast32_t absA;
        if (a != 0)
        {
            var sign = a < 0;
            absA = (uint_fast32_t)(sign ? -a : a);
            var shiftDist = CountLeadingZeroes32(absA);
            return PackToExtF80(sign, 0x401E - shiftDist, (uint_fast64_t)absA << (shiftDist + 32));
        }

        return FromBitsUI80(0, 0);
    }

    // i64_to_extF80
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static ExtFloat80 FromInt64(SoftFloatContext context, int64_t a)
    {
        uint_fast64_t absA;
        if (a != 0)
        {
            var sign = a < 0;
            absA = (uint_fast64_t)(sign ? -a : a);
            var shiftDist = CountLeadingZeroes64(absA);
            return PackToExtF80(sign, 0x403E - shiftDist, absA << shiftDist);
        }

        return FromBitsUI80(0, 0);
    }

    #endregion

    #region Floating-point-to-integer Conversions

    public uint32_t ToUInt32(SoftFloatContext context, bool exact) => ToUInt32(context, context.Rounding, exact);

    public uint64_t ToUInt64(SoftFloatContext context, bool exact) => ToUInt64(context, context.Rounding, exact);

    public int32_t ToInt32(SoftFloatContext context, bool exact) => ToInt32(context, context.Rounding, exact);

    public int64_t ToInt64(SoftFloatContext context, bool exact) => ToInt64(context, context.Rounding, exact);

    // extF80_to_ui32
    public uint32_t ToUInt32(SoftFloatContext context, RoundingMode roundingMode, bool exact)
    {
        uint_fast16_t uiA64;
        int_fast32_t exp, shiftDist;
        uint_fast64_t sig;
        bool sign;

        uiA64 = _signExp;
        sign = SignExtF80UI64(uiA64);
        exp = ExpExtF80UI64(uiA64);
        sig = _signif;

        if (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
        {
            switch (context.Specialize.UInt32NaNKind)
            {
                case SpecializeNaNIntegerKind.NaNIsPosOverflow:
                    sign = false;
                    break;

                case SpecializeNaNIntegerKind.NaNIsNegOverflow:
                    sign = true;
                    break;

                case SpecializeNaNIntegerKind.NaNIsUnique:
                    context.RaiseFlags(ExceptionFlags.Invalid);
                    return context.UInt32FromNaN;
            }
        }

        shiftDist = 0x4032 - exp;
        if (shiftDist <= 0)
            shiftDist = 1;

        sig = ShiftRightJam64(sig, shiftDist);
        return RoundToUI32(context, sign, sig, roundingMode, exact);
    }

    // extF80_to_ui64
    public uint64_t ToUInt64(SoftFloatContext context, RoundingMode roundingMode, bool exact)
    {
        uint_fast16_t uiA64;
        bool sign;
        int_fast32_t exp, shiftDist;
        uint_fast64_t sig, sigExtra;

        uiA64 = _signExp;
        sign = SignExtF80UI64(uiA64);
        exp = ExpExtF80UI64(uiA64);
        sig = _signif;

        shiftDist = 0x403E - exp;
        if (shiftDist < 0)
        {
            context.RaiseFlags(ExceptionFlags.Invalid);
            return (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
                ? context.UInt64FromNaN
                : context.UInt64FromOverflow(sign);
        }

        sigExtra = 0;
        if (shiftDist != 0)
            (sigExtra, sig) = ShiftRightJam64Extra(sig, 0, shiftDist);

        return RoundToUI64(context, sign, sig, sigExtra, roundingMode, exact);
    }

    // extF80_to_i32
    public int32_t ToInt32(SoftFloatContext context, RoundingMode roundingMode, bool exact)
    {
        uint_fast16_t uiA64;
        int_fast32_t exp, shiftDist;
        uint_fast64_t sig;
        bool sign;

        uiA64 = _signExp;
        sign = SignExtF80UI64(uiA64);
        exp = ExpExtF80UI64(uiA64);
        sig = _signif;

        if (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
        {
            switch (context.Specialize.Int32NaNKind)
            {
                case SpecializeNaNIntegerKind.NaNIsPosOverflow:
                    sign = false;
                    break;

                case SpecializeNaNIntegerKind.NaNIsNegOverflow:
                    sign = true;
                    break;

                case SpecializeNaNIntegerKind.NaNIsUnique:
                    context.RaiseFlags(ExceptionFlags.Invalid);
                    return context.Int32FromNaN;
            }
        }

        shiftDist = 0x4032 - exp;
        if (shiftDist <= 0)
            shiftDist = 1;

        sig = ShiftRightJam64(sig, shiftDist);
        return RoundToI32(context, sign, sig, roundingMode, exact);
    }

    // extF80_to_i64
    public int64_t ToInt64(SoftFloatContext context, RoundingMode roundingMode, bool exact)
    {
        uint_fast16_t uiA64;
        int_fast32_t exp, shiftDist;
        uint_fast64_t sig, sigExtra;
        bool sign;

        uiA64 = _signExp;
        sign = SignExtF80UI64(uiA64);
        exp = ExpExtF80UI64(uiA64);
        sig = _signif;

        shiftDist = 0x403E - exp;
        if (shiftDist <= 0)
        {
            if (shiftDist != 0)
            {
                context.RaiseFlags(ExceptionFlags.Invalid);
                return (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
                    ? context.Int64FromNaN
                    : context.Int64FromOverflow(sign);
            }

            sigExtra = 0;
        }
        else
        {
            (sigExtra, sig) = ShiftRightJam64Extra(sig, 0, shiftDist);
        }

        return RoundToI64(context, sign, sig, sigExtra, roundingMode, exact);
    }

    // extF80_to_ui32_r_minMag
    public uint32_t ToUInt32RoundMinMag(SoftFloatContext context, bool exact)
    {
        uint_fast16_t uiA64, z;
        int_fast32_t exp, shiftDist;
        uint_fast64_t sig;
        bool sign;

        uiA64 = _signExp;
        exp = ExpExtF80UI64(uiA64);
        sig = _signif;

        shiftDist = 0x403E - exp;
        if (64 <= shiftDist)
        {
            if (exact && ((uint_fast32_t)exp | sig) != 0)
                context.ExceptionFlags |= ExceptionFlags.Inexact;

            return 0;
        }

        sign = SignExtF80UI64(uiA64);
        if (sign || shiftDist < 32)
        {
            context.RaiseFlags(ExceptionFlags.Invalid);
            return (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
                ? context.UInt32FromNaN
                : context.UInt32FromOverflow(sign);
        }

        z = (uint_fast32_t)(sig >> shiftDist);
        if (exact && ((uint_fast64_t)z << shiftDist) != sig)
            context.ExceptionFlags |= ExceptionFlags.Inexact;

        return z;
    }

    // extF80_to_ui64_r_minMag
    public uint64_t ToUInt64RoundMinMag(SoftFloatContext context, bool exact)
    {
        uint_fast16_t uiA64;
        int_fast32_t exp, shiftDist;
        uint_fast64_t sig, z;
        bool sign;

        uiA64 = _signExp;
        exp = ExpExtF80UI64(uiA64);
        sig = _signif;

        shiftDist = 0x403E - exp;
        if (64 <= shiftDist)
        {
            if (exact && ((uint_fast32_t)exp | sig) != 0)
                context.ExceptionFlags |= ExceptionFlags.Inexact;

            return 0;
        }

        sign = SignExtF80UI64(uiA64);
        if (sign || shiftDist < 0)
        {
            context.RaiseFlags(ExceptionFlags.Invalid);
            return (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
                ? context.UInt64FromNaN
                : context.UInt64FromOverflow(sign);
        }

        z = sig >> shiftDist;
        if (exact && (z << shiftDist) != sig)
            context.ExceptionFlags |= ExceptionFlags.Inexact;

        return z;
    }

    // extF80_to_i32_r_minMag
    public int32_t ToInt32RoundMinMag(SoftFloatContext context, bool exact)
    {
        uint_fast16_t uiA64;
        int_fast32_t exp, shiftDist, absZ;
        uint_fast64_t sig;
        bool sign;

        uiA64 = _signExp;
        exp = ExpExtF80UI64(uiA64);
        sig = _signif;

        shiftDist = 0x403E - exp;
        if (64 <= shiftDist)
        {
            if (exact && ((uint_fast32_t)exp | sig) != 0)
                context.ExceptionFlags |= ExceptionFlags.Inexact;

            return 0;
        }

        sign = SignExtF80UI64(uiA64);
        if (shiftDist < 33)
        {
            if (uiA64 == PackToExtF80UI64(true, 0x401E) && sig < 0x8000000100000000)
            {
                if (exact && (sig & 0x00000000FFFFFFFF) != 0)
                    context.ExceptionFlags |= ExceptionFlags.Inexact;

                return -0x7FFFFFFF - 1;
            }

            context.RaiseFlags(ExceptionFlags.Invalid);
            return (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
                ? context.Int32FromNaN
                : context.Int32FromOverflow(sign);
        }

        absZ = (int_fast32_t)(sig >> shiftDist);
        if (exact && ((uint_fast64_t)(uint_fast32_t)absZ << shiftDist) != sig)
            context.ExceptionFlags |= ExceptionFlags.Inexact;

        return sign ? -absZ : absZ;
    }

    // extF80_to_i64_r_minMag
    public int64_t ToInt64RoundMinMag(SoftFloatContext context, bool exact)
    {
        uint_fast16_t uiA64;
        int_fast32_t exp, shiftDist;
        uint_fast64_t sig, absZ;
        bool sign;

        uiA64 = _signExp;
        exp = ExpExtF80UI64(uiA64);
        sig = _signif;

        shiftDist = 0x403E - exp;
        if (64 <= shiftDist)
        {
            if (exact && ((uint_fast32_t)exp | sig) != 0)
                context.ExceptionFlags |= ExceptionFlags.Inexact;

            return 0;
        }

        sign = SignExtF80UI64(uiA64);
        if (shiftDist <= 0)
        {
            if (uiA64 == PackToExtF80UI64(true, 0x403E) && sig == 0x8000000000000000)
                return -0x7FFFFFFFFFFFFFFF - 1;

            context.RaiseFlags(ExceptionFlags.Invalid);
            return (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
                ? context.Int64FromNaN
                : context.Int64FromOverflow(sign);
        }

        absZ = sig >> shiftDist;
        if (exact && (sig << (-shiftDist)) != 0)
            context.ExceptionFlags |= ExceptionFlags.Inexact;

        return sign ? -(int_fast64_t)absZ : (int_fast64_t)absZ;
    }

    #endregion

    #region Floating-point-to-floating-point Conversions

    // extF80_to_f16
    public Float16 ToFloat16(SoftFloatContext context)
    {
        uint_fast16_t uiA64, sig16;
        uint_fast64_t uiA0, sig;
        int_fast32_t exp;
        bool sign;

        uiA64 = _signExp;
        uiA0 = _signif;
        sign = SignExtF80UI64(uiA64);
        exp = ExpExtF80UI64(uiA64);
        sig = uiA0;

        if (exp == 0x7FFF)
        {
            if ((sig & 0x7FFFFFFFFFFFFFFF) != 0)
            {
                context.ExtFloat80BitsToCommonNaN(uiA64, uiA0, out var commonNaN);
                return context.CommonNaNToFloat16(in commonNaN);
            }
            else
            {
                return PackToF16(sign, 0x1f, 0);
            }
        }

        sig16 = (uint_fast16_t)ShortShiftRightJam64(sig, 49);
        if (((uint_fast32_t)exp | sig16) == 0)
            return PackToF16(sign, 0, 0);

        exp -= 0x3FF1;
        if (sizeof(int_fast16_t) < sizeof(int_fast32_t) && exp < -0x40)
            exp = -0x40;

        return RoundPackToF16(context, sign, exp, sig16);
    }

    // extF80_to_f32
    public Float32 ToFloat32(SoftFloatContext context)
    {
        uint_fast16_t uiA64;
        uint_fast64_t uiA0, sig;
        int_fast32_t exp;
        uint_fast32_t sig32;
        bool sign;

        uiA64 = _signExp;
        uiA0 = _signif;
        sign = SignExtF80UI64(uiA64);
        exp = ExpExtF80UI64(uiA64);
        sig = uiA0;

        if (exp == 0x7FFF)
        {
            if ((sig & 0x7FFFFFFFFFFFFFFF) != 0)
            {
                context.ExtFloat80BitsToCommonNaN(uiA64, uiA0, out var commonNaN);
                return context.CommonNaNToFloat32(in commonNaN);
            }
            else
            {
                return PackToF32(sign, 0xFF, 0);
            }
        }

        sig32 = (uint_fast32_t)ShortShiftRightJam64(sig, 33);
        if (((uint_fast32_t)exp | sig32) == 0)
            return PackToF32(sign, 0, 0);

        exp -= 0x3F81;
        if (sizeof(int_fast16_t) < sizeof(int_fast32_t) && exp < -0x1000)
            exp = -0x1000;

        return RoundPackToF32(context, sign, exp, sig32);
    }

    // extF80_to_f64
    public Float64 ToFloat64(SoftFloatContext context)
    {
        uint_fast16_t uiA64;
        uint_fast64_t uiA0, sig;
        int_fast32_t exp;
        bool sign;

        uiA64 = _signExp;
        uiA0 = _signif;
        sign = SignExtF80UI64(uiA64);
        exp = ExpExtF80UI64(uiA64);
        sig = uiA0;

        if (((uint_fast32_t)exp | sig) == 0)
            return PackToF64(sign, 0, 0);

        if (exp == 0x7FFF)
        {
            if ((sig & 0x7FFFFFFFFFFFFFFF) != 0)
            {
                context.ExtFloat80BitsToCommonNaN(uiA64, uiA0, out var commonNaN);
                return context.CommonNaNToFloat64(in commonNaN);
            }
            else
            {
                return PackToF64(sign, 0x7FF, 0);
            }
        }

        sig = ShortShiftRightJam64(sig, 1);
        exp -= 0x3C01;
        if (sizeof(int_fast16_t) < sizeof(int_fast32_t) && exp < -0x1000)
            exp = -0x1000;

        return RoundPackToF64(context, sign, exp, sig);
    }

    // extF80_to_f128
    public Float128 ToFloat128(SoftFloatContext context)
    {
        uint_fast16_t uiA64, exp;
        uint_fast64_t uiA0, frac;
        SFUInt128 frac128;
        bool sign;

        uiA64 = _signExp;
        uiA0 = _signif;
        exp = (uint_fast16_t)ExpExtF80UI64(uiA64);
        frac = uiA0 & 0x7FFFFFFFFFFFFFFF;

        if (exp == 0x7FFF && frac != 0)
        {
            context.ExtFloat80BitsToCommonNaN(uiA64, uiA0, out var commonNaN);
            return context.CommonNaNToFloat128(in commonNaN);
        }
        else
        {
            sign = SignExtF80UI64(uiA64);
            frac128 = ShortShiftLeft128(0, frac, 49);
            return PackToF128(sign, (int_fast16_t)exp, frac128.V64, frac128.V00);
        }
    }

    #endregion

    #region Arithmetic Operations

    // extF80_roundToInt
    public ExtFloat80 RoundToInt(SoftFloatContext context, RoundingMode roundingMode, bool exact)
    {
        uint_fast16_t uiA64, signUI64, uiZ64;
        int_fast32_t exp;
        uint_fast64_t sigA, sigZ, lastBitMask, roundBitsMask;

        uiA64 = _signExp;
        signUI64 = uiA64 & PackToExtF80UI64(true, 0);
        exp = ExpExtF80UI64(uiA64);
        sigA = _signif;

        if ((sigA & 0x8000000000000000) == 0 && exp != 0x7FFF)
        {
            if (sigA == 0)
                return FromBitsUI80((ushort)signUI64, 0);

            (var expTmp, sigA) = NormSubnormalExtF80Sig(sigA);
            exp += expTmp;
        }

        if (0x403E <= exp)
        {
            if (exp == 0x7FFF)
            {
                if ((sigA & 0x7FFFFFFFFFFFFFFF) != 0)
                    return context.PropagateNaNExtFloat80Bits(uiA64, sigA, 0, 0);

                sigZ = 0x8000000000000000;
            }
            else
            {
                sigZ = sigA;
            }

            return FromBitsUI80((ushort)(signUI64 | (uint_fast32_t)exp), sigZ);
        }
        else if (exp <= 0x3FFE)
        {
            if (exact)
                context.ExceptionFlags |= ExceptionFlags.Inexact;

            switch (roundingMode)
            {
                case RoundingMode.NearEven:
                {
                    if ((sigA & 0x7FFFFFFFFFFFFFFF) != 0)
                        goto case RoundingMode.NearMaxMag;

                    break;
                }
                case RoundingMode.NearMaxMag:
                {
                    if (exp == 0x3FFE)
                        return FromBitsUI80((ushort)(signUI64 | 0x3FFF), 0x8000000000000000);

                    break;
                }
                case RoundingMode.Min:
                {
                    if (signUI64 != 0)
                        return FromBitsUI80((ushort)(signUI64 | 0x3FFF), 0x8000000000000000);

                    break;
                }
                case RoundingMode.Max:
                {
                    if (signUI64 == 0)
                        return FromBitsUI80((ushort)(signUI64 | 0x3FFF), 0x8000000000000000);

                    break;
                }
                case RoundingMode.Odd:
                {
                    return FromBitsUI80((ushort)(signUI64 | 0x3FFF), 0x8000000000000000);
                }
            }

            return FromBitsUI80((ushort)signUI64, 0);
        }

        uiZ64 = signUI64 | (uint_fast32_t)exp;
        lastBitMask = 1UL << (0x403E - exp);
        roundBitsMask = lastBitMask - 1;
        sigZ = sigA;
        if (roundingMode == RoundingMode.NearMaxMag)
        {
            sigZ += lastBitMask >> 1;
        }
        else if (roundingMode == RoundingMode.NearEven)
        {
            sigZ += lastBitMask >> 1;
            if ((sigZ & roundBitsMask) == 0)
                sigZ &= ~lastBitMask;
        }
        else if (roundingMode == (signUI64 != 0 ? RoundingMode.Min : RoundingMode.Max))
        {
            sigZ += roundBitsMask;
        }

        sigZ &= ~roundBitsMask;
        if (sigZ == 0)
        {
            ++uiZ64;
            sigZ = 0x8000000000000000;
        }

        if (sigZ != sigA)
        {
            if (roundingMode == RoundingMode.Odd)
                sigZ |= lastBitMask;

            if (exact)
                context.ExceptionFlags |= ExceptionFlags.Inexact;
        }

        return FromBitsUI80((ushort)uiZ64, sigZ);
    }

    // extF80_add
    public static ExtFloat80 Add(SoftFloatContext context, ExtFloat80 a, ExtFloat80 b)
    {
        var signA = SignExtF80UI64(a._signExp);
        var signB = SignExtF80UI64(b._signExp);

        return signA == signB
            ? AddMagsExtF80(context, a._signExp, a._signif, b._signExp, b._signif, signA)
            : SubMagsExtF80(context, a._signExp, a._signif, b._signExp, b._signif, signA);
    }

    // extF80_sub
    public static ExtFloat80 Subtract(SoftFloatContext context, ExtFloat80 a, ExtFloat80 b)
    {
        var signA = SignExtF80UI64(a._signExp);
        var signB = SignExtF80UI64(b._signExp);

        return (signA == signB)
            ? SubMagsExtF80(context, a._signExp, a._signif, b._signExp, b._signif, signA)
            : AddMagsExtF80(context, a._signExp, a._signif, b._signExp, b._signif, signA);
    }

    // extF80_mul
    public static ExtFloat80 Multiply(SoftFloatContext context, ExtFloat80 a, ExtFloat80 b)
    {
        uint_fast16_t uiA64, uiB64;
        uint_fast64_t uiA0, sigA, uiB0, sigB;
        int_fast32_t expA, expB, expZ;
        SFUInt128 sig128Z;
        bool signA, signB, signZ;

        uiA64 = a._signExp;
        uiA0 = a._signif;
        signA = SignExtF80UI64(uiA64);
        expA = ExpExtF80UI64(uiA64);
        sigA = uiA0;
        uiB64 = b._signExp;
        uiB0 = b._signif;
        signB = SignExtF80UI64(uiB64);
        expB = ExpExtF80UI64(uiB64);
        sigB = uiB0;
        signZ = signA ^ signB;

        if (expA == 0x7FFF)
        {
            if ((sigA & 0x7FFFFFFFFFFFFFFF) != 0 || (expB == 0x7FFF && (sigB & 0x7FFFFFFFFFFFFFFF) != 0))
                return context.PropagateNaNExtFloat80Bits(uiA64, uiA0, uiB64, uiB0);

            if (((uint_fast32_t)expB | sigB) == 0)
            {
                context.RaiseFlags(ExceptionFlags.Invalid);
                return context.DefaultNaNExtFloat80;
            }
            else
            {
                return PackToExtF80(signZ, 0x7FFF, 0x8000000000000000);
            }
        }
        else if (expB == 0x7FFF)
        {
            if ((sigB & 0x7FFFFFFFFFFFFFFF) != 0)
                return context.PropagateNaNExtFloat80Bits(uiA64, uiA0, uiB64, uiB0);

            if (((uint_fast32_t)expB | sigB) == 0)
            {
                context.RaiseFlags(ExceptionFlags.Invalid);
                return context.DefaultNaNExtFloat80;
            }
            else
            {
                return PackToExtF80(signZ, 0x7FFF, 0x8000000000000000);
            }
        }

        if (expA == 0)
            expA = 1;

        if ((sigA & 0x8000000000000000) == 0)
        {
            if (sigA == 0)
                return PackToExtF80(signZ, 0, 0);

            (var expTmp, sigA) = NormSubnormalExtF80Sig(sigA);
            expA += expTmp;
        }

        if (expB == 0)
            expB = 1;

        if ((sigB & 0x8000000000000000) == 0)
        {
            if (sigB == 0)
                return PackToExtF80(signZ, 0, 0);

            (var expTmp, sigA) = NormSubnormalExtF80Sig(sigB);
            expB += expTmp;
        }

        expZ = expA + expB - 0x3FFE;
        sig128Z = Mul64To128(sigA, sigB);
        if (sig128Z.V64 < 0x8000000000000000)
        {
            --expZ;
            sig128Z += sig128Z; // shift left by one instead?
        }

        return RoundPackToExtF80(context, signZ, expZ, sig128Z.V64, sig128Z.V00, context.RoundingPrecisionExtFloat80);
    }

    // extF80_div
    public static ExtFloat80 Divide(SoftFloatContext context, ExtFloat80 a, ExtFloat80 b)
    {
        uint_fast16_t uiA64, uiB64, recip32, q;
        uint_fast64_t uiA0, sigA, uiB0, sigB, sigZ, q64, sigZExtra;
        int_fast32_t expA, expB, expZ;
        SFUInt128 rem, term;
        int ix;
        bool signA, signB, signZ;

        uiA64 = a._signExp;
        uiA0 = a._signif;
        signA = SignExtF80UI64(uiA64);
        expA = ExpExtF80UI64(uiA64);
        sigA = uiA0;
        uiB64 = b._signExp;
        uiB0 = b._signif;
        signB = SignExtF80UI64(uiB64);
        expB = ExpExtF80UI64(uiB64);
        sigB = uiB0;
        signZ = signA ^ signB;

        if (expA == 0x7FFF)
        {
            if ((sigA & 0x7FFFFFFFFFFFFFFF) != 0)
                return context.PropagateNaNExtFloat80Bits(uiA64, uiA0, uiB64, uiB0);

            if (expB == 0x7FFF && (sigB & 0x7FFFFFFFFFFFFFFF) != 0)
                return context.PropagateNaNExtFloat80Bits(uiA64, uiA0, uiB64, uiB0);

            return PackToExtF80(signZ, 0x7FFF, 0x8000000000000000);
        }
        else if (expB == 0x7FFF)
        {
            if ((sigB & 0x7FFFFFFFFFFFFFFF) != 0)
                return context.PropagateNaNExtFloat80Bits(uiA64, uiA0, uiB64, uiB0);

            return PackToExtF80(signZ, 0, 0);
        }

        if (expB == 0)
            expB = 1;

        if ((sigB & 0x8000000000000000) == 0)
        {
            if (sigB == 0)
            {
                if (sigA == 0)
                {
                    context.RaiseFlags(ExceptionFlags.Invalid);
                    return context.DefaultNaNExtFloat80;
                }

                context.RaiseFlags(ExceptionFlags.Infinite);
                return PackToExtF80(signZ, 0x7FFF, 0x8000000000000000);
            }

            (var expTmp, sigB) = NormSubnormalExtF80Sig(sigB);
            expB += expTmp;
        }

        if (expA == 0)
            expA = 1;

        if ((sigA & 0x8000000000000000) == 0)
        {
            if (sigA == 0)
                return PackToExtF80(signZ, 0, 0);

            (var expTmp, sigA) = NormSubnormalExtF80Sig(sigA);
            expA += expTmp;
        }

        expZ = expA - expB + 0x3FFF;
        if (sigA < sigB)
        {
            --expZ;
            rem = ShortShiftLeft128(0, sigA, 32);
        }
        else
        {
            rem = ShortShiftLeft128(0, sigA, 31);
        }

        recip32 = ApproxRecip32_1((uint)(sigB >> 32));
        sigZ = 0;
        ix = 2;
        while (true)
        {
            q64 = (uint_fast64_t)(uint32_t)(rem.V64 >> 2) * recip32;
            q = (uint_fast16_t)((q64 + 0x80000000) >> 32);
            if (--ix < 0)
                break;

            rem <<= 29;
            term = Mul64ByShifted32To128(sigB, q);
            rem -= term;
            if ((rem.V64 & 0x8000000000000000) != 0)
            {
                --q;
                rem = Add128(rem.V64, rem.V00, sigB >> 32, sigB << 32);
            }

            sigZ = (sigZ << 29) + q;
        }

        if (((q + 1) & 0x3FFFFF) < 2)
        {
            rem >>= 29;
            term = Mul64ByShifted32To128(sigB, q);
            rem -= term;
            term = ShortShiftLeft128(0, sigB, 32);
            if ((rem.V64 & 0x8000000000000000) != 0)
            {
                --q;
                rem -= term;
            }
            else if (term < rem)
            {
                ++q;
                rem -= term;
            }

            if (!rem.IsZero)
                q |= 1;
        }

        sigZ = (sigZ << 6) + (q >> 23);
        sigZExtra = (uint_fast64_t)q << 41;

        return RoundPackToExtF80(context, signZ, expZ, sigZ, sigZExtra, context.RoundingPrecisionExtFloat80);
    }

    // extF80_rem
    public static ExtFloat80 Modulus(SoftFloatContext context, ExtFloat80 a, ExtFloat80 b)
    {
        uint_fast16_t uiA64, uiB64, q, recip32;
        uint_fast64_t uiA0, sigA, uiB0, sigB, q64;
        int_fast32_t expA, expB, expDiff;
        SFUInt128 rem, shiftedSigB, term, altRem, meanRem;
        bool signA, signRem;

        uiA64 = a._signExp;
        uiA0 = a._signif;
        signA = SignExtF80UI64(uiA64);
        expA = ExpExtF80UI64(uiA64);
        sigA = uiA0;
        uiB64 = b._signExp;
        uiB0 = b._signif;
        expB = ExpExtF80UI64(uiB64);
        sigB = uiB0;

        if (expA == 0x7FFF)
        {
            if ((sigA & 0x7FFFFFFFFFFFFFFF) != 0 || (expB == 0x7FFF && (sigB & 0x7FFFFFFFFFFFFFFF) != 0))
                return context.PropagateNaNExtFloat80Bits(uiA64, uiA0, uiB64, uiB0);

            context.RaiseFlags(ExceptionFlags.Invalid);
            return context.DefaultNaNExtFloat80;
        }
        else if (expB == 0x7FFF)
        {
            if ((sigB & 0x7FFFFFFFFFFFFFFF) != 0)
                return context.PropagateNaNExtFloat80Bits(uiA64, uiA0, uiB64, uiB0);

            // Argument b is an infinity. Doubling 'expB' is an easy way to ensure that 'expDiff' later is less than -1, which will result
            // in returning a canonicalized version of argument 'a'.
            expB += expB;
        }

        if (expB == 0)
            expB = 1;

        if ((sigB & 0x8000000000000000) == 0)
        {
            if (sigB == 0)
            {
                context.RaiseFlags(ExceptionFlags.Invalid);
                return context.DefaultNaNExtFloat80;
            }

            (var expTmp, sigB) = NormSubnormalExtF80Sig(sigB);
            expB += expTmp;
        }

        if (expA == 0)
            expA = 1;

        if ((sigA & 0x8000000000000000) == 0)
        {
            if (sigA == 0)
            {
                expA = 0;
                if (expA < 1)
                {
                    sigA >>= 1 - expA;
                    expA = 0;
                }

                return PackToExtF80(signA, expA, sigA);
            }

            (var expTmp, sigA) = NormSubnormalExtF80Sig(sigA);
            expA += expTmp;
        }

        expDiff = expA - expB;
        if (expDiff < -1)
        {
            if (expA < 1)
            {
                sigA >>= 1 - expA;
                expA = 0;
            }

            return PackToExtF80(signA, expA, sigA);
        }

        rem = ShortShiftLeft128(0, sigB, 32);
        shiftedSigB = ShortShiftLeft128(0, sigB, 32);

        if (expDiff < 1)
        {
            if (expDiff != 0)
            {
                //--expB;
                shiftedSigB = ShortShiftLeft128(0, sigB, 33);
                q = 0;
            }
            else
            {
                q = sigB <= sigA ? 1U : 0;
                if (q != 0)
                    rem -= shiftedSigB;
            }
        }
        else
        {
            recip32 = ApproxRecip32_1((uint)(sigB >> 32));
            expDiff -= 30;
            while (true)
            {
                q64 = (uint_fast64_t)(uint32_t)(rem.V64 >> 2) * recip32;
                if (expDiff < 0)
                    break;

                q = (uint_fast16_t)((q64 + 0x80000000) >> 32);
                rem >>= 29;
                term = Mul64ByShifted32To128(sigB, q);
                rem -= term;
                if ((rem.V64 & 0x8000000000000000) != 0)
                    rem += shiftedSigB;

                expDiff -= 29;
            }

            // ('expDiff' cannot be less than -29 here.)
            q = (uint32_t)(q64 >> 32) >> (~expDiff & 31);
            rem <<= expDiff + 30;
            term = Mul64ByShifted32To128(sigB, q);
            rem -= term;
            if ((rem.V64 & 0x8000000000000000) != 0)
            {
                altRem = rem + shiftedSigB;
                goto selectRem;
            }
        }

        do
        {
            altRem = rem;
            ++q;
            rem -= shiftedSigB;
        }
        while ((rem.V64 & 0x8000000000000000) == 0);

    selectRem:
        meanRem = rem + altRem;
        if ((meanRem.V64 & 0x8000000000000000) != 0 || (meanRem.IsZero && (q & 1) != 0))
            rem = altRem;

        signRem = signA;
        if ((rem.V64 & 0x8000000000000000) != 0)
        {
            signRem = !signRem;
            rem = -rem;
        }

        return NormRoundPackToExtF80(context, signRem, rem.IsZero ? expB + 32 : 0, rem.V64, rem.V00, ExtFloat80RoundingPrecision._80);
    }

    // extF80_sqrt
    public ExtFloat80 SquareRoot(SoftFloatContext context)
    {
        uint_fast16_t uiA64, sig32A, recipSqrt32, sig32Z;
        uint_fast64_t uiA0, sigA, q, x64, sigZ, sigZExtra;
        int_fast32_t expA, expZ;
        SFUInt128 rem, y, term;
        bool signA;

        uiA64 = _signExp;
        uiA0 = _signif;
        signA = SignExtF80UI64(uiA64);
        expA = ExpExtF80UI64(uiA64);
        sigA = uiA0;

        if (expA == 0x7FFF)
        {
            if ((sigA & 0x7FFFFFFFFFFFFFFF) != 0)
                return context.PropagateNaNExtFloat80Bits(uiA64, uiA0, 0, 0);

            if (!signA)
                return this;

            context.RaiseFlags(ExceptionFlags.Invalid);
            return context.DefaultNaNExtFloat80;
        }

        if (signA)
        {
            if (sigA == 0)
                return PackToExtF80(signA, 0, 0);

            context.RaiseFlags(ExceptionFlags.Invalid);
            return context.DefaultNaNExtFloat80;
        }

        if (expA == 0)
            expA = 1;

        if ((sigA & 0x8000000000000000) == 0)
        {
            if (sigA == 0)
                return PackToExtF80(signA, 0, 0);

            (var expTmp, sigA) = NormSubnormalExtF80Sig(sigA);
            expA += expTmp;
        }

        // ('sig32Z' is guaranteed to be a lower bound on the square root of 'sig32A', which makes 'sig32Z' also a lower bound on the
        // square root of 'sigA'.)

        expZ = ((expA - 0x3FFF) >> 1) + 0x3FFF;
        expA &= 1;
        sig32A = (uint_fast16_t)(sigA >> 32);
        recipSqrt32 = ApproxRecipSqrt32_1((uint_fast32_t)expA, sig32A);
        sig32Z = (uint_fast16_t)(((uint_fast64_t)sig32A * recipSqrt32) >> 32);
        if (expA != 0)
        {
            sig32Z >>= 1;
            rem = ShortShiftLeft128(0, sigA, 61);
        }
        else
        {
            rem = ShortShiftLeft128(0, sigA, 62);
        }

        rem.V64 -= (uint_fast64_t)sig32Z * sig32Z;
        q = ((uint32_t)(rem.V64 >> 2) * (uint_fast64_t)recipSqrt32) >> 32;
        x64 = (uint_fast64_t)sig32Z << 32;
        sigZ = x64 + (q << 3);
        y = rem << 29;

        // (Repeating this loop is a rare occurrence.)
        while (true)
        {
            term = Mul64ByShifted32To128(x64 + sigZ, (uint_fast32_t)q);
            rem = y - term;
            if ((rem.V64 & 0x8000000000000000) == 0)
                break;

            --q;
            sigZ -= 1U << 3;
        }

        q = (((rem.V64 >> 2) * recipSqrt32) >> 32) + 2;
        x64 = sigZ;
        sigZ = (sigZ << 1) + (q >> 25);
        sigZExtra = q << 39;

        if ((q & 0xFFFFFF) <= 2)
        {
            q &= ~0xFFFFUL;
            sigZExtra = q << 39;
            term = Mul64ByShifted32To128(x64 + (q >> 27), (uint_fast32_t)q);
            x64 = (q << 5) * (uint32_t)q;
            term += x64;
            rem <<= 28;
            rem -= term;
            if ((rem.V64 & 0x8000000000000000) != 0)
            {
                if (sigZExtra == 0)
                    --sigZ;

                --sigZExtra;
            }
            else
            {
                if (!rem.IsZero)
                    sigZExtra |= 1;
            }
        }

        return RoundPackToExtF80(context, false, expZ, sigZ, sigZExtra, context.RoundingPrecisionExtFloat80);
    }

    #endregion

    #region Comparison Operations

    // extF80_eq (signaling=false) & extF80_eq_signaling (signaling=true)
    public static bool CompareEqual(SoftFloatContext context, ExtFloat80 a, ExtFloat80 b, bool signaling)
    {
        uint_fast16_t uiA64, uiB64;
        uint_fast64_t uiA0, uiB0;

        uiA64 = a._signExp;
        uiA0 = a._signif;
        uiB64 = b._signExp;
        uiB0 = b._signif;

        if (IsNaNExtF80UI((int_fast16_t)uiA64, uiA0) || IsNaNExtF80UI((int_fast16_t)uiB64, uiB0))
        {
            if (signaling && (context.IsSignalingNaNExtFloat80Bits(uiA64, uiA0) || context.IsSignalingNaNExtFloat80Bits(uiB64, uiB0)))
                context.RaiseFlags(ExceptionFlags.Invalid);

            return false;
        }

        return uiA0 == uiB0 && (uiA64 == uiB64 || (uiA0 == 0 && ((uiA64 | uiB64) & 0x7FFF) == 0));
    }

    // extF80_le (signaling=true) & extF80_le_quiet (signaling=false)
    public static bool CompareLessThanOrEqual(SoftFloatContext context, ExtFloat80 a, ExtFloat80 b, bool signaling)
    {
        uint_fast16_t uiA64, uiB64;
        uint_fast64_t uiA0, uiB0;
        bool signA, signB;

        uiA64 = a._signExp;
        uiA0 = a._signif;
        uiB64 = b._signExp;
        uiB0 = b._signif;

        if (IsNaNExtF80UI((int_fast16_t)uiA64, uiA0) || IsNaNExtF80UI((int_fast16_t)uiB64, uiB0))
        {
            if (signaling && (context.IsSignalingNaNExtFloat80Bits(uiA64, uiA0) || context.IsSignalingNaNExtFloat80Bits(uiB64, uiB0)))
                context.RaiseFlags(ExceptionFlags.Invalid);

            return false;
        }

        signA = SignExtF80UI64(uiA64);
        signB = SignExtF80UI64(uiB64);

        return (signA != signB)
            ? (signA || ((uiA64 | uiB64) & 0x7FFF) == 0 || (uiA0 | uiB0) != 0)
            : (uiA64 == uiB64 && uiA0 == uiB0) || (signA ^ LT128(uiA64, uiA0, uiB64, uiB0));
    }

    // extF80_lt (signaling=true) & extF80_lt_quiet (signaling=false)
    public static bool CompareLessThan(SoftFloatContext context, ExtFloat80 a, ExtFloat80 b, bool signaling)
    {
        uint_fast16_t uiA64, uiB64;
        uint_fast64_t uiA0, uiB0;
        bool signA, signB;

        uiA64 = a._signExp;
        uiA0 = a._signif;
        uiB64 = b._signExp;
        uiB0 = b._signif;

        if (IsNaNExtF80UI((int_fast16_t)uiA64, uiA0) || IsNaNExtF80UI((int_fast16_t)uiB64, uiB0))
        {
            if (signaling && (context.IsSignalingNaNExtFloat80Bits(uiA64, uiA0) || context.IsSignalingNaNExtFloat80Bits(uiB64, uiB0)))
                context.RaiseFlags(ExceptionFlags.Invalid);

            return false;
        }

        signA = SignExtF80UI64(uiA64);
        signB = SignExtF80UI64(uiB64);

        return (signA != signB)
            ? (signA && ((uiA64 | uiB64) & 0x7FFF) != 0 && (uiA0 | uiB0) != 0)
            : ((uiA64 != uiB64 || uiA0 != uiB0) && (signA ^ LT128(uiA64, uiA0, uiB64, uiB0)));
    }

    #endregion

    #endregion
}
