#region Copyright
// This is a C# port of the SoftFloat library release 3e by Thomas Kaiser.

/*============================================================================

This C source file is part of the SoftFloat IEEE Floating-Point Arithmetic
Package, Release 3e, by John R. Hauser.

Copyright 2011, 2012, 2013, 2014, 2015 The Regents of the University of
California.  All rights reserved.

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
using static Specialize;
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

    // extF80_isSignalingNaN
    public bool IsSignalingNaN => IsSigNaNExtF80UI(_signExp, _signif);

    #endregion

    #region Methods

    public static ExtFloat80 FromUIntBits(ushort signExp, ulong significand) => new(signExp, significand);

    public (ushort signExp, ulong significant) ToUIntBits() => (_signExp, _signif);

    // TODO: Add support for .NET 7+ UInt128 bit conversions.

    // THIS IS THE INTERNAL CONSTRUCTOR FOR RAW BITS.
    internal static ExtFloat80 FromUI128(UInt128 v)
    {
        Debug.Assert((v.V64 & ~0xFFFFU) == 0);
        return FromUI80((ushort)v.V64, v.V00);
    }

    // THIS IS THE INTERNAL CONSTRUCTOR FOR RAW BITS.
    // TODO: Allow signExp to be a full 32-bit integer (reduces total number of "unnecessary" casts).
    internal static ExtFloat80 FromUI80(ushort signExp, ulong signif) => new(signExp, signif);

    #region Integer-to-floating-point Conversions

    // NOTE: These operators use the default software floating-point state (which doesn't matter currently for this type).
    public static explicit operator ExtFloat80(uint32_t a) => FromUInt32(a);
    public static explicit operator ExtFloat80(uint64_t a) => FromUInt64(a);
    public static explicit operator ExtFloat80(int32_t a) => FromInt32(a);
    public static explicit operator ExtFloat80(int64_t a) => FromInt64(a);

    // ui32_to_extF80
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static ExtFloat80 FromUInt32(uint32_t a, SoftFloatState? state = null)
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

        return FromUI80(signExp: (ushort)uiZ64, signif: (uint_fast64_t)a << 32);
    }

    // ui64_to_extF80
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static ExtFloat80 FromUInt64(uint64_t a, SoftFloatState? state = null)
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

        return FromUI80(signExp: (ushort)uiZ64, signif: a << 32);
    }

    // i32_to_extF80
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static ExtFloat80 FromInt32(int32_t a, SoftFloatState? state = null)
    {
        uint_fast16_t uiZ64;
        uint_fast32_t absA;
        if (a != 0)
        {
            var sign = a < 0;
            absA = (uint_fast32_t)(sign ? -a : a);
            var shiftDist = CountLeadingZeroes32(absA);
            uiZ64 = PackToExtF80UI64(sign, 0x401E - shiftDist);
            absA <<= shiftDist;
        }
        else
        {
            uiZ64 = 0;
            absA = 0;
        }

        return FromUI80(signExp: (ushort)uiZ64, signif: (uint_fast64_t)absA << 32);
    }

    // i64_to_extF80
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static ExtFloat80 FromInt64(int64_t a, SoftFloatState? state = null)
    {
        uint_fast16_t uiZ64;
        uint_fast64_t absA;
        if (a != 0)
        {
            var sign = a < 0;
            absA = (uint_fast64_t)(sign ? -a : a);
            var shiftDist = CountLeadingZeroes64(absA);
            uiZ64 = PackToExtF80UI64(sign, 0x403E - shiftDist);
            absA <<= shiftDist;
        }
        else
        {
            uiZ64 = 0;
            absA = 0;
        }

        return FromUI80(signExp: (ushort)uiZ64, signif: absA);
    }

    #endregion

    #region Floating-point-to-integer Conversions

    public uint32_t ToUInt32(bool exact, SoftFloatState state) => ToUInt32(state.RoundingMode, exact, state);

    public uint64_t ToUInt64(bool exact, SoftFloatState state) => ToUInt64(state.RoundingMode, exact, state);

    public int32_t ToInt32(bool exact, SoftFloatState state) => ToInt32(state.RoundingMode, exact, state);

    public int64_t ToInt64(bool exact, SoftFloatState state) => ToInt64(state.RoundingMode, exact, state);

    // extF80_to_ui32
    public uint32_t ToUInt32(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        uint_fast16_t uiA64;
        int_fast32_t exp, shiftDist;
        uint_fast64_t sig;
        bool sign;

        uiA64 = _signExp;
        sign = SignExtF80UI64(uiA64);
        exp = ExpExtF80UI64(uiA64);
        sig = _signif;

        if ((ui32_fromNaN != ui32_fromPosOverflow || ui32_fromNaN != ui32_fromNegOverflow) &&
            exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (ui32_fromNaN == ui32_fromPosOverflow)
            {
                sign = false;
            }
            else if (ui32_fromNaN == ui32_fromNegOverflow)
            {
                sign = true;
            }
            else
            {
                (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
                return ui32_fromNaN;
            }
#pragma warning restore CS0162 // Unreachable code detected
        }

        shiftDist = 0x4032 - exp;
        if (shiftDist <= 0)
            shiftDist = 1;

        sig = ShiftRightJam64(sig, shiftDist);
        return RoundToUI32(state ?? SoftFloatState.Default, sign, sig, roundingMode, exact);
    }

    // extF80_to_ui64
    public uint64_t ToUInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
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
            (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
            return (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
                ? ui64_fromNaN
                : (sign ? ui64_fromNegOverflow : ui64_fromPosOverflow);
        }

        sigExtra = 0;
        if (shiftDist != 0)
            (sigExtra, sig) = ShiftRightJam64Extra(sig, 0, shiftDist);

        return RoundToUI64(state ?? SoftFloatState.Default, sign, sig, sigExtra, roundingMode, exact);
    }

    // extF80_to_i32
    public int32_t ToInt32(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        uint_fast16_t uiA64;
        int_fast32_t exp, shiftDist;
        uint_fast64_t sig;
        bool sign;

        uiA64 = _signExp;
        sign = SignExtF80UI64(uiA64);
        exp = ExpExtF80UI64(uiA64);
        sig = _signif;

        if ((i32_fromNaN != i32_fromPosOverflow || i32_fromNaN != i32_fromNegOverflow) &&
            exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (i32_fromNaN == i32_fromPosOverflow)
            {
                sign = false;
            }
            else if (i32_fromNaN == i32_fromNegOverflow)
            {
                sign = true;
            }
            else
            {
                (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
                return i32_fromNaN;
            }
#pragma warning restore CS0162 // Unreachable code detected
        }

        shiftDist = 0x4032 - exp;
        if (shiftDist <= 0)
            shiftDist = 1;

        sig = ShiftRightJam64(sig, shiftDist);
        return RoundToI32(state ?? SoftFloatState.Default, sign, sig, roundingMode, exact);
    }

    // extF80_to_i64
    public int64_t ToInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
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
                (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
                return (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
                    ? i64_fromNaN
                    : (sign ? i64_fromNegOverflow : i64_fromPosOverflow);
            }

            sigExtra = 0;
        }
        else
        {
            (sigExtra, sig) = ShiftRightJam64Extra(sig, 0, shiftDist);
        }

        return RoundToI64(state ?? SoftFloatState.Default, sign, sig, sigExtra, roundingMode, exact);
    }

    // extF80_to_ui32_r_minMag
    public uint32_t ToUInt32RoundMinMag(bool exact, SoftFloatState? state = null)
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
                (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

            return 0;
        }

        sign = SignExtF80UI64(uiA64);
        if (sign || shiftDist < 32)
        {
            (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
            return (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
                ? ui32_fromNaN
                : (sign ? ui32_fromNegOverflow : ui32_fromPosOverflow);
        }

        z = (uint_fast32_t)(sig >> shiftDist);
        if (exact && ((uint_fast64_t)z << shiftDist) != sig)
            (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

        return z;
    }

    // extF80_to_ui64_r_minMag
    public uint64_t ToUInt64RoundMinMag(bool exact, SoftFloatState? state = null)
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
                (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

            return 0;
        }

        sign = SignExtF80UI64(uiA64);
        if (sign || shiftDist < 0)
        {
            (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
            return (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
                ? ui64_fromNaN
                : (sign ? ui64_fromNegOverflow : ui64_fromPosOverflow);
        }

        z = sig >> shiftDist;
        if (exact && (z << shiftDist) != sig)
            (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

        return z;
    }

    // extF80_to_i32_r_minMag
    public int32_t ToInt32RoundMinMag(bool exact, SoftFloatState? state = null)
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
                (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

            return 0;
        }

        sign = SignExtF80UI64(uiA64);
        if (shiftDist < 33)
        {
            if (uiA64 == PackToExtF80UI64(true, 0x401E) && sig < 0x8000000100000000)
            {
                if (exact && (sig & 0x00000000FFFFFFFF) != 0)
                    (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

                return -0x7FFFFFFF - 1;
            }

            (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
            return (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
                ? i32_fromNaN
                : (sign ? i32_fromNegOverflow : i32_fromPosOverflow);
        }

        absZ = (int_fast32_t)(sig >> shiftDist);
        if (exact && ((uint_fast64_t)(uint_fast32_t)absZ << shiftDist) != sig)
            (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

        return sign ? -absZ : absZ;
    }

    // extF80_to_i64_r_minMag
    public int64_t ToInt64RoundMinMag(bool exact, SoftFloatState? state = null)
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
                (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

            return 0;
        }

        sign = SignExtF80UI64(uiA64);
        if (shiftDist <= 0)
        {
            if (uiA64 == PackToExtF80UI64(true, 0x403E) && sig == 0x8000000000000000)
                return -0x7FFFFFFFFFFFFFFF - 1;

            (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
            return (exp == 0x7FFF && (sig & 0x7FFFFFFFFFFFFFFF) != 0)
                ? i64_fromNaN
                : (sign ? i64_fromNegOverflow : i64_fromPosOverflow);
        }

        absZ = sig >> shiftDist;
        if (exact && (sig << (-shiftDist)) != 0)
            (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

        return sign ? -(int_fast64_t)absZ : (int_fast64_t)absZ;
    }

    #endregion

    #region Floating-point-to-floating-point Conversions

    // extF80_to_f16
    public Float16 ToFloat16(SoftFloatState? state = null)
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
                ExtF80UIToCommonNaN(state ?? SoftFloatState.Default, uiA64, uiA0, out var commonNaN);
                return Float16.FromUI16((uint16_t)CommonNaNToF16UI(in commonNaN));
            }
            else
            {
                return Float16.FromUI16(PackToF16UI(sign, 0x1f, 0));
            }
        }

        sig16 = (uint_fast16_t)ShortShiftRightJam64(sig, 49);
        if (((uint_fast32_t)exp | sig16) == 0)
            return Float16.FromUI16(PackToF16UI(sign, 0, 0));

        exp -= 0x3FF1;
        if (sizeof(int_fast16_t) < sizeof(int_fast32_t) && exp < -0x40)
            exp = -0x40;

        return RoundPackToF16(state ?? SoftFloatState.Default, sign, exp, sig16);
    }

    // extF80_to_f32
    public Float32 ToFloat32(SoftFloatState? state = null)
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
                ExtF80UIToCommonNaN(state ?? SoftFloatState.Default, uiA64, uiA0, out var commonNaN);
                return Float32.FromUI32(CommonNaNToF32UI(in commonNaN));
            }
            else
            {
                return Float32.FromUI32(PackToF32UI(sign, 0xFF, 0));
            }
        }

        sig32 = (uint_fast32_t)ShortShiftRightJam64(sig, 33);
        if (((uint_fast32_t)exp | sig32) == 0)
            return Float32.FromUI32(PackToF32UI(sign, 0, 0));

        exp -= 0x3F81;
        if (sizeof(int_fast16_t) < sizeof(int_fast32_t) && exp < -0x1000)
            exp = -0x1000;

        return RoundPackToF32(state ?? SoftFloatState.Default, sign, exp, sig32);
    }

    // extF80_to_f64
    public Float64 ToFloat64(SoftFloatState? state = null)
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
            return Float64.FromUI64(PackToF64UI(sign, 0, 0));

        if (exp == 0x7FFF)
        {
            if ((sig & 0x7FFFFFFFFFFFFFFF) != 0)
            {
                ExtF80UIToCommonNaN(state ?? SoftFloatState.Default, uiA64, uiA0, out var commonNaN);
                return Float64.FromUI64(CommonNaNToF64UI(in commonNaN));
            }
            else
            {
                return Float64.FromUI64(PackToF64UI(sign, 0x7FF, 0));
            }
        }

        sig = ShortShiftRightJam64(sig, 1);
        exp -= 0x3C01;
        if (sizeof(int_fast16_t) < sizeof(int_fast32_t) && exp < -0x1000)
            exp = -0x1000;

        return RoundPackToF64(state ?? SoftFloatState.Default, sign, exp, sig);
    }

    // extF80_to_f128
    public Float128 ToFloat128(SoftFloatState? state = null)
    {
        uint_fast16_t uiA64, exp;
        uint_fast64_t uiA0, frac;
        UInt128 frac128;
        bool sign;

        uiA64 = _signExp;
        uiA0 = _signif;
        exp = (uint_fast16_t)ExpExtF80UI64(uiA64);
        frac = uiA0 & 0x7FFFFFFFFFFFFFFF;

        if (exp == 0x7FFF && frac != 0)
        {
            ExtF80UIToCommonNaN(state ?? SoftFloatState.Default, uiA64, uiA0, out var commonNaN);
            return Float128.FromUI128(CommonNaNToF128UI(in commonNaN));
        }
        else
        {
            sign = SignExtF80UI64(uiA64);
            frac128 = ShortShiftLeft128(0, frac, 49);
            return Float128.FromUI128(
                v64: PackToF128UI64(sign, (int_fast16_t)exp, frac128.V64),
                v0: frac128.V00
            );
        }
    }

    #endregion

    #region Arithmetic Operations

    // extF80_roundToInt
    public ExtFloat80 RoundToInt(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
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
                return FromUI80((ushort)signUI64, 0);

            (var expTmp, sigA) = NormSubnormalExtF80Sig(sigA);
            exp += expTmp;
        }

        if (0x403E <= exp)
        {
            if (exp == 0x7FFF)
            {
                if ((sigA & 0x7FFFFFFFFFFFFFFF) != 0)
                    return FromUI128(PropagateNaNExtF80UI(state ?? SoftFloatState.Default, uiA64, sigA, 0, 0));

                sigZ = 0x8000000000000000;
            }
            else
            {
                sigZ = sigA;
            }

            return FromUI80((ushort)(signUI64 | (uint_fast32_t)exp), sigZ);
        }
        else if (exp <= 0x3FFE)
        {
            if (exact)
                (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;

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
                        return FromUI80((ushort)(signUI64 | 0x3FFF), 0x8000000000000000);

                    break;
                }
                case RoundingMode.Min:
                {
                    if (signUI64 != 0)
                        return FromUI80((ushort)(signUI64 | 0x3FFF), 0x8000000000000000);

                    break;
                }
                case RoundingMode.Max:
                {
                    if (signUI64 == 0)
                        return FromUI80((ushort)(signUI64 | 0x3FFF), 0x8000000000000000);

                    break;
                }
                case RoundingMode.Odd:
                {
                    return FromUI80((ushort)(signUI64 | 0x3FFF), 0x8000000000000000);
                }
            }

            return FromUI80((ushort)signUI64, 0);
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
                (state ?? SoftFloatState.Default).ExceptionFlags |= ExceptionFlags.Inexact;
        }

        return FromUI80((ushort)uiZ64, sigZ);
    }

    // extF80_add
    public static ExtFloat80 Add(ExtFloat80 a, ExtFloat80 b, SoftFloatState? state = null)
    {
        var signA = SignExtF80UI64(a._signExp);
        var signB = SignExtF80UI64(b._signExp);
        state ??= SoftFloatState.Default;
        return signA == signB
            ? AddMagsExtF80(state, a._signExp, a._signif, b._signExp, b._signif, signA)
            : SubMagsExtF80(state, a._signExp, a._signif, b._signExp, b._signif, signA);
    }

    // extF80_sub
    public static ExtFloat80 Subtract(ExtFloat80 a, ExtFloat80 b, SoftFloatState? state = null)
    {
        var signA = SignExtF80UI64(a._signExp);
        var signB = SignExtF80UI64(b._signExp);
        state ??= SoftFloatState.Default;
        return (signA == signB)
            ? SubMagsExtF80(state, a._signExp, a._signif, b._signExp, b._signif, signA)
            : AddMagsExtF80(state, a._signExp, a._signif, b._signExp, b._signif, signA);
    }

    // extF80_mul
    public static ExtFloat80 Multiply(ExtFloat80 a, ExtFloat80 b, SoftFloatState? state = null)
    {
        uint_fast16_t uiA64, uiB64;
        uint_fast64_t uiA0, sigA, uiB0, sigB;
        int_fast32_t expA, expB, expZ;
        UInt128 sig128Z;
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
                return FromUI128(PropagateNaNExtF80UI(state ?? SoftFloatState.Default, uiA64, uiA0, uiB64, uiB0));

            if (((uint_fast32_t)expB | sigB) == 0)
            {
                (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
                return FromUI80(DefaultNaNExtF80UI64, DefaultNaNExtF80UI0);
            }
            else
            {
                return FromUI80(PackToExtF80UI64(signZ, 0x7FFF), 0x8000000000000000);
            }
        }
        else if (expB == 0x7FFF)
        {
            if ((sigB & 0x7FFFFFFFFFFFFFFF) != 0)
                return FromUI128(PropagateNaNExtF80UI(state ?? SoftFloatState.Default, uiA64, uiA0, uiB64, uiB0));

            if (((uint_fast32_t)expB | sigB) == 0)
            {
                (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
                return FromUI80(DefaultNaNExtF80UI64, DefaultNaNExtF80UI0);
            }
            else
            {
                return FromUI80(PackToExtF80UI64(signZ, 0x7FFF), 0x8000000000000000);
            }
        }

        if (expA == 0)
            expA = 1;

        if ((sigA & 0x8000000000000000) == 0)
        {
            if (sigA == 0)
                return FromUI80(PackToExtF80UI64(signZ, 0), 0);

            (var expTmp, sigA) = NormSubnormalExtF80Sig(sigA);
            expA += expTmp;
        }

        if (expB == 0)
            expB = 1;

        if ((sigB & 0x8000000000000000) == 0)
        {
            if (sigB == 0)
                return FromUI80(PackToExtF80UI64(signZ, 0), 0);

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

        state ??= SoftFloatState.Default;
        return RoundPackToExtF80(state, signZ, expZ, sig128Z.V64, sig128Z.V00, state.ExtFloat80RoundingPrecision);
    }

    // extF80_div
    public static ExtFloat80 Divide(ExtFloat80 a, ExtFloat80 b, SoftFloatState? state = null)
    {
        uint_fast16_t uiA64, uiB64, recip32, q;
        uint_fast64_t uiA0, sigA, uiB0, sigB, sigZ, q64, sigZExtra;
        int_fast32_t expA, expB, expZ;
        UInt128 rem, term;
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
                return FromUI128(PropagateNaNExtF80UI(state ?? SoftFloatState.Default, uiA64, uiA0, uiB64, uiB0));

            if (expB == 0x7FFF)
            {
                if ((sigB & 0x7FFFFFFFFFFFFFFF) != 0)
                    return FromUI128(PropagateNaNExtF80UI(state ?? SoftFloatState.Default, uiA64, uiA0, uiB64, uiB0));
            }

            return FromUI80(PackToExtF80UI64(signZ, 0x7FFF), 0x8000000000000000);
        }
        else if (expB == 0x7FFF)
        {
            if ((sigB & 0x7FFFFFFFFFFFFFFF) != 0)
                return FromUI128(PropagateNaNExtF80UI(state ?? SoftFloatState.Default, uiA64, uiA0, uiB64, uiB0));

            return FromUI80(PackToExtF80UI64(signZ, 0), 0);
        }

        if (expB == 0)
            expB = 1;

        if ((sigB & 0x8000000000000000) == 0)
        {
            if (sigB == 0)
            {
                if (sigA == 0)
                {
                    (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
                    return FromUI80(DefaultNaNExtF80UI64, DefaultNaNExtF80UI0);
                }

                (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Infinite);
                return FromUI80(PackToExtF80UI64(signZ, 0x7FFF), 0x8000000000000000);
            }

            (var expTmp, sigB) = NormSubnormalExtF80Sig(sigB);
            expB += expTmp;
        }

        if (expA == 0)
            expA = 1;

        if ((sigA & 0x8000000000000000) == 0)
        {
            if (sigA == 0)
                return FromUI80(PackToExtF80UI64(signZ, 0), 0);

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

            rem = ShortShiftLeft128(rem.V64, rem.V00, 29);
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
        return RoundPackToExtF80(state ?? SoftFloatState.Default, signZ, expZ, sigZ, sigZExtra,
            (state ?? SoftFloatState.Default).ExtFloat80RoundingPrecision);
    }

    // extF80_rem
    public static ExtFloat80 Modulus(ExtFloat80 a, ExtFloat80 b, SoftFloatState? state = null)
    {
        uint_fast16_t uiA64, uiB64, q, recip32;
        uint_fast64_t uiA0, sigA, uiB0, sigB, q64;
        int_fast32_t expA, expB, expDiff;
        UInt128 rem, shiftedSigB, term, altRem, meanRem;
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
                return FromUI128(PropagateNaNExtF80UI(state ?? SoftFloatState.Default, uiA64, uiA0, uiB64, uiB0));

            (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
            return FromUI80(DefaultNaNExtF80UI64, DefaultNaNExtF80UI0);
        }
        else if (expB == 0x7FFF)
        {
            if ((sigB & 0x7FFFFFFFFFFFFFFF) != 0)
                return FromUI128(PropagateNaNExtF80UI(state ?? SoftFloatState.Default, uiA64, uiA0, uiB64, uiB0));

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
                (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
                return FromUI80(DefaultNaNExtF80UI64, DefaultNaNExtF80UI0);
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

                return FromUI80(PackToExtF80UI64(signA, expA), sigA);
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

            return FromUI80(PackToExtF80UI64(signA, expA), sigA);
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

        return NormRoundPackToExtF80(state ?? SoftFloatState.Default, signRem, (rem.V64 | rem.V00) != 0 ? expB + 32 : 0, rem.V64, rem.V00, ExtFloat80RoundingPrecision._80);
    }

    // extF80_sqrt
    public ExtFloat80 SquareRoot(SoftFloatState? state = null)
    {
        uint_fast16_t uiA64, sig32A, recipSqrt32, sig32Z;
        uint_fast64_t uiA0, sigA, q, x64, sigZ, sigZExtra;
        int_fast32_t expA, expZ;
        UInt128 rem, y, term;
        bool signA;

        uiA64 = _signExp;
        uiA0 = _signif;
        signA = SignExtF80UI64(uiA64);
        expA = ExpExtF80UI64(uiA64);
        sigA = uiA0;

        if (expA == 0x7FFF)
        {
            if ((sigA & 0x7FFFFFFFFFFFFFFF) != 0)
                return FromUI128(PropagateNaNExtF80UI(state ?? SoftFloatState.Default, uiA64, uiA0, 0, 0));

            if (!signA)
                return this;

            (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
            return FromUI80(DefaultNaNExtF80UI64, DefaultNaNExtF80UI0);
        }

        if (signA)
        {
            if (sigA == 0)
                return FromUI80(PackToExtF80UI64(signA, 0), 0);

            (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);
            return FromUI80(DefaultNaNExtF80UI64, DefaultNaNExtF80UI0);
        }

        if (expA == 0)
            expA = 1;

        if ((sigA & 0x8000000000000000) == 0)
        {
            if (sigA == 0)
                return FromUI80(PackToExtF80UI64(signA, 0), 0);

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

        state ??= SoftFloatState.Default;
        return RoundPackToExtF80(state, false, expZ, sigZ, sigZExtra, state.ExtFloat80RoundingPrecision);
    }

    #endregion

    #region Comparison Operations

    // extF80_eq (quiet=true) & extF80_eq_signaling (quiet=false)
    public static bool CompareEqual(ExtFloat80 a, ExtFloat80 b, bool quiet, SoftFloatState? state = null)
    {
        uint_fast16_t uiA64, uiB64;
        uint_fast64_t uiA0, uiB0;

        uiA64 = a._signExp;
        uiA0 = a._signif;
        uiB64 = b._signExp;
        uiB0 = b._signif;

        if (IsNaNExtF80UI((int_fast16_t)uiA64, uiA0) || IsNaNExtF80UI((int_fast16_t)uiB64, uiB0))
        {
            if (!quiet || IsSigNaNExtF80UI(uiA64, uiA0) || IsSigNaNExtF80UI(uiB64, uiB0))
                (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);

            return false;
        }

        return uiA0 == uiB0 && (uiA64 == uiB64 || (uiA0 == 0 && ((uiA64 | uiB64) & 0x7FFF) == 0));
    }

    // extF80_le (quiet=false) & extF80_le_quiet (quiet=true)
    public static bool CompareLessThanOrEqual(ExtFloat80 a, ExtFloat80 b, bool quiet, SoftFloatState? state = null)
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
            if (!quiet || IsSigNaNExtF80UI(uiA64, uiA0) || IsSigNaNExtF80UI(uiB64, uiB0))
                (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);

            return false;
        }

        signA = SignExtF80UI64(uiA64);
        signB = SignExtF80UI64(uiB64);

        return (signA != signB)
            ? (signA || ((uiA64 | uiB64) & 0x7FFF) == 0 || (uiA0 | uiB0) != 0)
            : (uiA64 == uiB64 && uiA0 == uiB0) || (signA ^ LT128(uiA64, uiA0, uiB64, uiB0));
    }

    // extF80_lt (quiet=false) & extF80_lt_quiet (quiet=true)
    public static bool CompareLessThan(ExtFloat80 a, ExtFloat80 b, bool quiet, SoftFloatState? state = null)
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
            if (!quiet || IsSigNaNExtF80UI(uiA64, uiA0) || IsSigNaNExtF80UI(uiB64, uiB0))
                (state ?? SoftFloatState.Default).RaiseFlags(ExceptionFlags.Invalid);

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
