﻿#region Copyright
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

[StructLayout(LayoutKind.Sequential, Pack = sizeof(ulong), Size = sizeof(ulong))]
public readonly struct Float64
{
    #region Fields

    // WARNING: DO NOT ADD OR CHANGE ANY OF THESE FIELDS!!!
    private readonly ulong _v;

    #endregion

    #region Constructors

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used to avoid accidentally calling other overloads.")]
    private Float64(ulong v, bool dummy)
    {
        _v = v;
    }

    public Float64(double value)
    {
        _v = BitConverter.DoubleToUInt64Bits(value);
    }

    #endregion

    #region Properties

    // f64_isSignalingNaN
    public bool IsSignalingNaN => IsSigNaNF64UI(_v);

    #endregion

    #region Methods

    public static explicit operator Float64(double value) => new(value);
    public static implicit operator double(Float64 value) => BitConverter.UInt64BitsToDouble(value._v);

    public static Float64 FromUInt64Bits(ulong value) => FromBitsUI64(value);

    public ulong ToUInt64Bits() => _v;

    // THIS IS THE INTERNAL CONSTRUCTOR FOR RAW BITS.
    internal static Float64 FromBitsUI64(ulong v) => new(v, dummy: false);

    #region Integer-to-floating-point Conversions

    // NOTE: These operators use the default software floating-point state.
    public static explicit operator Float64(uint32_t a) => FromUInt32(a);
    public static explicit operator Float64(uint64_t a) => FromUInt64(a);
    public static explicit operator Float64(int32_t a) => FromInt32(a);
    public static explicit operator Float64(int64_t a) => FromInt64(a);

    // ui32_to_f64
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static Float64 FromUInt32(uint32_t a, SoftFloatState? state = null)
    {
        if (a == 0)
            return FromBitsUI64(0);

        var shiftDist = CountLeadingZeroes32(a) + 21;
        return FromBitsUI64(PackToF64UI(false, 0x432 - shiftDist, (uint_fast64_t)a << shiftDist));
    }

    // ui64_to_f64
    public static Float64 FromUInt64(uint64_t a, SoftFloatState? state = null)
    {
        if (a == 0)
            return FromBitsUI64(0);

        state ??= SoftFloatState.Default;
        return (a & 0x8000000000000000) != 0
            ? RoundPackToF64(state, false, 0x43D, ShortShiftRightJam64(a, 1))
            : NormRoundPackToF64(state, false, 0x43C, a);
    }

    // i32_to_f64
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static Float64 FromInt32(int32_t a, SoftFloatState? state = null)
    {
        if (a == 0)
            return FromBitsUI64(0);

        var sign = a < 0;
        var absA = (uint_fast32_t)(sign ? -a : a);
        var shiftDist = CountLeadingZeroes32(absA) + 21;
        return FromBitsUI64(PackToF64UI(sign, 0x432 - shiftDist, (uint_fast64_t)absA << shiftDist));
    }

    // i64_to_f64
    public static Float64 FromInt64(int64_t a, SoftFloatState? state = null)
    {
        var sign = a < 0;
        if ((a & 0x7FFFFFFFFFFFFFFF) == 0)
            return FromBitsUI64(sign ? PackToF64UI(true, 0x43E, 0UL) : 0UL);

        var absA = (uint_fast64_t)(sign ? -a : a);
        return NormRoundPackToF64(state ?? SoftFloatState.Default, sign, 0x43C, absA);
    }

    #endregion

    #region Floating-point-to-integer Conversions

    public uint32_t ToUInt32(bool exact, SoftFloatState state) => ToUInt32(state.RoundingMode, exact, state);

    public uint64_t ToUInt64(bool exact, SoftFloatState state) => ToUInt64(state.RoundingMode, exact, state);

    public int32_t ToInt32(bool exact, SoftFloatState state) => ToInt32(state.RoundingMode, exact, state);

    public int64_t ToInt64(bool exact, SoftFloatState state) => ToInt64(state.RoundingMode, exact, state);

    // f64_to_ui32
    public uint32_t ToUInt32(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        uint_fast64_t sig;
        int_fast16_t exp, shiftDist;
        bool sign;

        sign = SignF64UI(_v);
        exp = ExpF64UI(_v);
        sig = FracF64UI(_v);

#pragma warning disable CS0162 // Unreachable code detected
        if (((ui32_fromNaN != ui32_fromPosOverflow) || (ui32_fromNaN != ui32_fromNegOverflow)) && exp == 0x7FF && sig != 0)
        {
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
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
                return ui32_fromNaN;
            }
        }
#pragma warning restore CS0162 // Unreachable code detected

        if (exp != 0)
            sig |= 0x0010000000000000;

        shiftDist = 0x427 - exp;
        if (0 < shiftDist)
            sig = ShiftRightJam64(sig, shiftDist);

        state ??= SoftFloatState.Default;
        return RoundToUI32(state, sign, sig, roundingMode, exact);
    }

    // f64_to_ui64
    public uint64_t ToUInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        uint_fast64_t sig;
        int_fast16_t exp, shiftDist;
        UInt64Extra sigExtra;
        bool sign;

        sign = SignF64UI(_v);
        exp = ExpF64UI(_v);
        sig = FracF64UI(_v);

        if (exp != 0)
            sig |= 0x0010000000000000;

        shiftDist = 0x433 - exp;
        if (shiftDist <= 0)
        {
            if (shiftDist < -11)
            {
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
                return (exp == 0x7FF && FracF64UI(_v) != 0)
                    ? ui64_fromNaN
                    : (sign ? ui64_fromNegOverflow : ui64_fromPosOverflow);
            }

            sigExtra.V = sig << (-shiftDist);
            sigExtra.Extra = 0;
        }
        else
        {
            sigExtra = ShiftRightJam64Extra(sig, 0, shiftDist);
        }

        state ??= SoftFloatState.Default;
        return RoundToUI64(state, sign, sigExtra.V, sigExtra.Extra, roundingMode, exact);
    }

    // f64_to_i32
    public int32_t ToInt32(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        uint_fast64_t sig;
        int_fast16_t exp, shiftDist;
        bool sign;

        sign = SignF64UI(_v);
        exp = ExpF64UI(_v);
        sig = FracF64UI(_v);

#pragma warning disable CS0162 // Unreachable code detected
        if (((i32_fromNaN != i32_fromPosOverflow) || (i32_fromNaN != i32_fromNegOverflow)) && exp == 0x7FF && sig != 0)
        {
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
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
                return i32_fromNaN;
            }
        }
#pragma warning restore CS0162 // Unreachable code detected

        if (exp != 0)
            sig |= 0x0010000000000000;

        shiftDist = 0x427 - exp;
        if (0 < shiftDist)
            sig = ShiftRightJam64(sig, shiftDist);

        state ??= SoftFloatState.Default;
        return RoundToI32(state, sign, sig, roundingMode, exact);
    }

    // f64_to_i64
    public int64_t ToInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        uint_fast64_t sig;
        int_fast16_t exp, shiftDist;
        UInt64Extra sigExtra;
        bool sign;

        sign = SignF64UI(_v);
        exp = ExpF64UI(_v);
        sig = FracF64UI(_v);

        if (exp != 0)
            sig |= 0x0010000000000000;

        shiftDist = 0x433 - exp;
        if (shiftDist <= 0)
        {
            if (shiftDist < -11)
            {
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
                return (exp == 0x7FF && FracF64UI(_v) != 0)
                    ? i64_fromNaN
                    : (sign ? i64_fromNegOverflow : i64_fromPosOverflow);
            }

            sigExtra.V = sig << (-shiftDist);
            sigExtra.Extra = 0;
        }
        else
        {
            sigExtra = ShiftRightJam64Extra(sig, 0, shiftDist);
        }

        state ??= SoftFloatState.Default;
        return RoundToI64(state, sign, sigExtra.V, sigExtra.Extra, roundingMode, exact);
    }

    // f64_to_ui32_r_minMag
    public uint32_t ToUInt32RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        uint_fast64_t sig;
        int_fast16_t exp, shiftDist;
        uint_fast32_t z;
        bool sign;

        exp = ExpF64UI(_v);
        sig = FracF64UI(_v);

        shiftDist = 0x433 - exp;
        if (53 <= shiftDist)
        {
            if (exact && ((uint_fast16_t)exp | sig) != 0)
            {
                state ??= SoftFloatState.Default;
                state.ExceptionFlags |= ExceptionFlags.Inexact;
            }

            return 0;
        }

        sign = SignF64UI(_v);
        if (sign || shiftDist < 21)
        {
            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return (exp == 0x7FF && sig != 0)
                ? ui32_fromNaN
                : (sign ? ui32_fromNegOverflow : ui32_fromPosOverflow);
        }

        sig |= 0x0010000000000000;
        z = (uint_fast32_t)(sig >> shiftDist);
        if (exact && ((uint_fast64_t)z << shiftDist) != sig)
        {
            state ??= SoftFloatState.Default;
            state.ExceptionFlags |= ExceptionFlags.Inexact;
        }

        return z;
    }

    // f64_to_ui64_r_minMag
    public uint64_t ToUInt64RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        uint_fast64_t sig, z;
        int_fast16_t exp, shiftDist;
        bool sign;

        exp = ExpF64UI(_v);
        sig = FracF64UI(_v);

        shiftDist = 0x433 - exp;
        if (53 <= shiftDist)
        {
            if (exact && ((uint_fast16_t)exp | sig) != 0)
            {
                state ??= SoftFloatState.Default;
                state.ExceptionFlags |= ExceptionFlags.Inexact;
            }

            return 0;
        }

        sign = SignF64UI(_v);
        if (sign)
        {
            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return (exp == 0x7FF && sig != 0)
                ? ui64_fromNaN
                : (sign ? ui64_fromNegOverflow : ui64_fromPosOverflow);
        }

        if (shiftDist <= 0)
        {
            if (shiftDist < -11)
            {
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
                return (exp == 0x7FF && sig != 0)
                    ? ui64_fromNaN
                    : (sign ? ui64_fromNegOverflow : ui64_fromPosOverflow);
            }

            z = (sig | 0x0010000000000000) << (-shiftDist);
        }
        else
        {
            sig |= 0x0010000000000000;
            z = sig >> shiftDist;
            if (exact && (uint64_t)(sig << (-shiftDist)) != 0)
            {
                state ??= SoftFloatState.Default;
                state.ExceptionFlags |= ExceptionFlags.Inexact;
            }
        }

        return z;
    }

    // f64_to_i32_r_minMag
    public int32_t ToInt32RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        uint_fast64_t sig;
        int_fast16_t exp, shiftDist;
        int_fast32_t absZ;
        bool sign;

        exp = ExpF64UI(_v);
        sig = FracF64UI(_v);

        shiftDist = 0x433 - exp;
        if (53 <= shiftDist)
        {
            if (exact && ((uint_fast16_t)exp | sig) != 0)
            {
                state ??= SoftFloatState.Default;
                state.ExceptionFlags |= ExceptionFlags.Inexact;
            }

            return 0;
        }

        sign = SignF64UI(_v);
        if (shiftDist < 22)
        {
            if (sign && exp == 0x41E && sig < 0x0000000000200000)
            {
                if (exact && sig != 0)
                {
                    state ??= SoftFloatState.Default;
                    state.ExceptionFlags |= ExceptionFlags.Inexact;
                }

                return -0x7FFFFFFF - 1;
            }

            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return (exp == 0x7FF && sig != 0)
                ? i32_fromNaN
                : (sign ? i32_fromNegOverflow : i32_fromPosOverflow);
        }

        sig |= 0x0010000000000000;
        absZ = (int_fast32_t)(sig >> shiftDist);
        if (exact && ((uint_fast64_t)(uint_fast32_t)absZ << shiftDist) != sig)
        {
            state ??= SoftFloatState.Default;
            state.ExceptionFlags |= ExceptionFlags.Inexact;
        }

        return sign ? -absZ : absZ;
    }

    // f64_to_i64_r_minMag
    public int64_t ToInt64RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        uint_fast64_t sig;
        int_fast16_t exp, shiftDist, absZ;
        bool sign;

        sign = SignF64UI(_v);
        exp = ExpF64UI(_v);
        sig = FracF64UI(_v);

        shiftDist = 0x433 - exp;
        if (shiftDist <= 0)
        {
            if (shiftDist < -10)
            {
                if (_v == PackToF64UI(true, 0x43E, 0))
                    return -0x7FFFFFFFFFFFFFFF - 1;

                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
                return (exp == 0x7FF && sig != 0)
                    ? i64_fromNaN
                    : (sign ? i64_fromNegOverflow : i64_fromPosOverflow);
            }

            sig |= 0x0010000000000000;
            absZ = (int_fast16_t)(sig << (-shiftDist));
        }
        else
        {
            if (53 <= shiftDist)
            {
                if (exact && ((uint_fast16_t)exp | sig) != 0)
                {
                    state ??= SoftFloatState.Default;
                    state.ExceptionFlags |= ExceptionFlags.Inexact;
                }

                return 0;
            }

            sig |= 0x0010000000000000;
            absZ = (int_fast16_t)(sig >> shiftDist);
            if (exact && ((uint_fast64_t)absZ << shiftDist) != sig)
            {
                state ??= SoftFloatState.Default;
                state.ExceptionFlags |= ExceptionFlags.Inexact;
            }
        }

        return sign ? -absZ : absZ;
    }

    #endregion

    #region Floating-point-to-floating-point Conversions

    // f64_to_f16
    public Float16 ToFloat16(SoftFloatState? state = null)
    {
        uint_fast64_t frac;
        int_fast16_t exp;
        uint_fast16_t frac16;
        bool sign;

        sign = SignF64UI(_v);
        exp = ExpF64UI(_v);
        frac = FracF64UI(_v);

        if (exp == 0x7FF)
        {
            if (frac != 0)
            {
                state ??= SoftFloatState.Default;
                F64UIToCommonNaN(state, _v, out var commonNaN);
                return Float16.FromBitsUI16((uint16_t)CommonNaNToF16UI(in commonNaN));
            }

            return Float16.FromBitsUI16(PackToF16UI(sign, 0x1F, 0));
        }

        frac16 = (uint_fast16_t)ShortShiftRightJam64(frac, 38);
        if (((uint_fast16_t)exp | frac16) == 0)
            return Float16.FromBitsUI16(PackToF16UI(sign, 0, 0));

        state ??= SoftFloatState.Default;
        return RoundPackToF16(state, sign, exp - 0x3F1, frac16 | 0x4000);
    }

    // f64_to_f32
    public Float32 ToFloat32(SoftFloatState? state = null)
    {
        uint_fast64_t frac;
        int_fast16_t exp;
        uint_fast32_t frac32;
        bool sign;

        sign = SignF64UI(_v);
        exp = ExpF64UI(_v);
        frac = FracF64UI(_v);

        if (exp == 0x7FF)
        {
            if (frac != 0)
            {
                state ??= SoftFloatState.Default;
                F64UIToCommonNaN(state, _v, out var commonNaN);
                return Float32.FromBitsUI32(CommonNaNToF32UI(in commonNaN));
            }

            return Float32.FromBitsUI32(PackToF32UI(sign, 0xFF, 0));
        }

        frac32 = (uint_fast32_t)ShortShiftRightJam64(frac, 22);
        if (((uint_fast16_t)exp | frac32) == 0)
            return Float32.FromBitsUI32(PackToF32UI(sign, 0, 0));

        state ??= SoftFloatState.Default;
        return RoundPackToF32(state, sign, exp - 0x381, frac32 | 0x40000000);
    }

    // f64_to_extF80
    public ExtFloat80 ToExtFloat80(SoftFloatState? state = null)
    {
        uint_fast64_t frac;
        int_fast16_t exp;
        bool sign;

        sign = SignF64UI(_v);
        exp = ExpF64UI(_v);
        frac = FracF64UI(_v);

        if (exp == 0x7FF)
        {
            if (frac != 0)
            {
                state ??= SoftFloatState.Default;
                F64UIToCommonNaN(state, _v, out var commonNaN);
                return ExtFloat80.FromBitsUI128(CommonNaNToExtF80UI(in commonNaN));
            }

            return ExtFloat80.FromBitsUI80(PackToExtF80UI64(sign, 0x7FF), 0x8000000000000000);
        }

        if (exp == 0)
        {
            if (frac == 0)
                return ExtFloat80.FromBitsUI80(PackToExtF80UI64(sign, 0), 0);

            (exp, frac) = NormSubnormalF64Sig(frac);
        }

        return ExtFloat80.FromBitsUI80(
            PackToExtF80UI64(sign, exp + 0x3C00),
            (frac | 0x0010000000000000) << 11);
    }

    // f64_to_f128
    public Float128 ToFloat128(SoftFloatState? state = null)
    {
        uint_fast64_t frac;
        int_fast16_t exp;
        UInt128 frac128;
        bool sign;

        sign = SignF64UI(_v);
        exp = ExpF64UI(_v);
        frac = FracF64UI(_v);

        if (exp == 0x7FF)
        {
            if (frac != 0)
            {
                state ??= SoftFloatState.Default;
                F64UIToCommonNaN(state, _v, out var commonNaN);
                return Float128.FromBitsUI128(CommonNaNToF128UI(in commonNaN));
            }

            return Float128.FromBitsUI128(PackToF128UI64(sign, 0x7FFF, 0), 0);
        }

        if (exp == 0)
        {
            if (frac == 0)
                return Float128.FromBitsUI128(PackToF128UI64(sign, 0, 0), 0);

            (exp, frac) = NormSubnormalF64Sig(frac);
            exp--;
        }

        frac128 = ShortShiftLeft128(0, frac, 60);
        return Float128.FromBitsUI128(
            PackToF128UI64(sign, exp + 0x3C00, frac128.V64),
            frac128.V00);
    }

    #endregion

    #region Arithmetic Operations

    public Float64 RoundToInt(bool exact, SoftFloatState state) => RoundToInt(state.RoundingMode, exact, state);

    // f64_roundToInt
    public Float64 RoundToInt(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        uint_fast64_t uiZ, lastBitMask, roundBitsMask;
        int_fast16_t exp;

        exp = ExpF64UI(_v);
        if (exp <= 0x3FE)
        {
            if ((_v & 0x7FFFFFFFFFFFFFFF) == 0)
                return this;

            if (exact)
            {
                state ??= SoftFloatState.Default;
                state.ExceptionFlags |= ExceptionFlags.Inexact;
            }

            uiZ = _v & PackToF64UI(true, 0, 0);
            switch (roundingMode)
            {
                case RoundingMode.NearEven:
                {
                    if (FracF64UI(_v) == 0)
                        break;

                    goto case RoundingMode.NearMaxMag;
                }
                case RoundingMode.NearMaxMag:
                {
                    if (exp == 0x3FE)
                        uiZ |= PackToF64UI(false, 0x3FF, 0);

                    break;
                }
                case RoundingMode.Min:
                {
                    if (uiZ != 0)
                        uiZ = PackToF64UI(true, 0x3FF, 0);

                    break;
                }
                case RoundingMode.Max:
                {
                    if (uiZ == 0)
                        uiZ = PackToF64UI(false, 0x3FF, 0);

                    break;
                }
                case RoundingMode.Odd:
                {
                    uiZ |= PackToF64UI(false, 0x3FF, 0);
                    break;
                }
            }

            return Float64.FromBitsUI64(uiZ);
        }

        if (0x433 <= exp)
        {
            if (exp == 0x7FF && FracF64UI(_v) != 0)
            {
                state ??= SoftFloatState.Default;
                return Float64.FromBitsUI64(PropagateNaNF64UI(state, _v, 0));
            }

            return this;
        }

        uiZ = _v;
        lastBitMask = (uint_fast64_t)1 << (0x433 - exp);
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
        else if (roundingMode == (SignF64UI(_v) ? RoundingMode.Min : RoundingMode.Max))
        {
            uiZ += roundBitsMask;
        }

        uiZ &= ~roundBitsMask;
        if (uiZ != _v)
        {
            if (roundingMode == RoundingMode.Odd)
                uiZ |= lastBitMask;

            if (exact)
            {
                state ??= SoftFloatState.Default;
                state.ExceptionFlags |= ExceptionFlags.Inexact;
            }
        }

        return Float64.FromBitsUI64(uiZ);
    }

    // f64_add
    public static Float64 Add(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        uint_fast64_t uiA, uiB;
        bool signA, signB;

        uiA = a._v;
        signA = SignF64UI(uiA);
        uiB = b._v;
        signB = SignF64UI(uiB);

        state ??= SoftFloatState.Default;
        return (signA == signB)
            ? AddMagsF64(state, uiA, uiB, signA)
            : SubMagsF64(state, uiA, uiB, signA);
    }

    // f64_sub
    public static Float64 Subtract(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        uint_fast64_t uiA, uiB;
        bool signA, signB;

        uiA = a._v;
        signA = SignF64UI(uiA);
        uiB = b._v;
        signB = SignF64UI(uiB);

        state ??= SoftFloatState.Default;
        return (signA == signB)
            ? SubMagsF64(state, uiA, uiB, signA)
            : AddMagsF64(state, uiA, uiB, signA);
    }

    // f64_mul
    public static Float64 Multiply(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        uint_fast64_t uiA, sigA, uiB, sigB, magBits, sigZ;
        int_fast16_t expA, expB, expZ;
        bool signA, signB, signZ;
        UInt128 sig128Z;

        uiA = a._v;
        signA = SignF64UI(uiA);
        expA = ExpF64UI(uiA);
        sigA = FracF64UI(uiA);
        uiB = b._v;
        signB = SignF64UI(uiB);
        expB = ExpF64UI(uiB);
        sigB = FracF64UI(uiB);
        signZ = signA ^ signB;

        if (expA == 0x7FF)
        {
            if (sigA != 0 || (expB == 0x7FF && sigB != 0))
            {
                state ??= SoftFloatState.Default;
                return Float64.FromBitsUI64(PropagateNaNF64UI(state, uiA, uiB));
            }

            magBits = (uint_fast16_t)expB | sigB;
            if (magBits == 0)
            {
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
                return Float64.FromBitsUI64(DefaultNaNF64UI);
            }
            else
            {
                return Float64.FromBitsUI64(PackToF64UI(signZ, 0x7FF, 0));
            }
        }

        if (expB == 0x7FF)
        {
            if (sigB != 0)
            {
                state ??= SoftFloatState.Default;
                return Float64.FromBitsUI64(PropagateNaNF64UI(state, uiA, uiB));
            }

            magBits = (uint_fast16_t)expA | sigA;
            if (magBits == 0)
            {
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
                return Float64.FromBitsUI64(DefaultNaNF64UI);
            }
            else
            {
                return Float64.FromBitsUI64(PackToF64UI(signZ, 0x7FF, 0));
            }
        }

        if (expA == 0)
        {
            if (sigA == 0)
                return Float64.FromBitsUI64(PackToF64UI(signZ, 0, 0));

            (expA, sigA) = NormSubnormalF64Sig(sigA);
        }

        if (expB == 0)
        {
            if (sigB == 0)
                return Float64.FromBitsUI64(PackToF64UI(signZ, 0, 0));

            (expB, sigB) = NormSubnormalF64Sig(sigB);
        }

        expZ = expA + expB - 0x3FF;
        sigA = (sigA | 0x0010000000000000) << 10;
        sigB = (sigB | 0x0010000000000000) << 11;
        sig128Z = Mul64To128(sigA, sigB);
        sigZ = sig128Z.V64 | (sig128Z.V00 != 0 ? 1U : 0);

        if (sigZ < 0x4000000000000000)
        {
            --expZ;
            sigZ <<= 1;
        }

        state ??= SoftFloatState.Default;
        return RoundPackToF64(state, signZ, expZ, sigZ);
    }

    // f64_mulAdd
    public static Float64 MultiplyAndAdd(Float64 a, Float64 b, Float64 c, SoftFloatState? state = null)
    {
        state ??= SoftFloatState.Default;
        return MulAddF64(state, a._v, b._v, c._v, MulAdd.None);
    }

    // f64_div
    public static Float64 Divide(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        uint_fast64_t uiA, sigA, uiB, sigB, rem, sigZ;
        int_fast16_t expA, expB, expZ;
        uint32_t recip32, sig32Z, doubleTerm, q;
        bool signA, signB, signZ;

        uiA = a._v;
        signA = SignF64UI(uiA);
        expA = ExpF64UI(uiA);
        sigA = FracF64UI(uiA);
        uiB = b._v;
        signB = SignF64UI(uiB);
        expB = ExpF64UI(uiB);
        sigB = FracF64UI(uiB);
        signZ = signA ^ signB;

        if (expA == 0x7FF)
        {
            if (sigA != 0)
            {
                state ??= SoftFloatState.Default;
                return Float64.FromBitsUI64(PropagateNaNF64UI(state, uiA, uiB));
            }

            if (expB == 0x7FF)
            {
                state ??= SoftFloatState.Default;
                if (sigB != 0)
                    return Float64.FromBitsUI64(PropagateNaNF64UI(state, uiA, uiB));

                state.RaiseFlags(ExceptionFlags.Invalid);
                return Float64.FromBitsUI64(DefaultNaNF64UI);
            }

            return Float64.FromBitsUI64(PackToF64UI(signZ, 0x7FF, 0));
        }

        if (expB == 0x7FF)
        {
            if (sigB != 0)
            {
                state ??= SoftFloatState.Default;
                return Float64.FromBitsUI64(PropagateNaNF64UI(state, uiA, uiB));
            }

            return Float64.FromBitsUI64(PackToF64UI(signZ, 0, 0));
        }

        if (expB == 0)
        {
            if (sigB == 0)
            {
                state ??= SoftFloatState.Default;
                if (((uint_fast16_t)expA | sigA) == 0)
                {
                    state.RaiseFlags(ExceptionFlags.Invalid);
                    return Float64.FromBitsUI64(DefaultNaNF64UI);
                }

                state.RaiseFlags(ExceptionFlags.Infinite);
                return Float64.FromBitsUI64(PackToF64UI(signZ, 0x7FF, 0));
            }

            (expB, sigB) = NormSubnormalF64Sig(sigB);
        }

        if (expA == 0)
        {
            if (sigA == 0)
                return Float64.FromBitsUI64(PackToF64UI(signZ, 0, 0));

            (expA, sigA) = NormSubnormalF64Sig(sigA);
        }

        expZ = expA - expB + 0x3FE;
        sigA |= 0x0010000000000000;
        sigB |= 0x0010000000000000;
        if (sigA < sigB)
        {
            --expZ;
            sigA <<= 11;
        }
        else
        {
            sigA <<= 10;
        }

        sigB <<= 11;
        recip32 = ApproxRecip32_1((uint32_t)(sigB >> 32)) - 2;
        sig32Z = (uint32_t)(((uint32_t)(sigA >> 32) * (uint_fast64_t)recip32) >> 32);
        doubleTerm = sig32Z << 1;
        rem = ((sigA - (uint_fast64_t)doubleTerm * (uint32_t)(sigB >> 32)) << 28)
            - (uint_fast64_t)doubleTerm * ((uint32_t)sigB >> 4);
        q = (uint32_t)((((uint32_t)(rem >> 32) * (uint_fast64_t)recip32) >> 32) + 4);
        sigZ = ((uint_fast64_t)sig32Z << 32) + ((uint_fast64_t)q << 4);

        if ((sigZ & 0x1FF) < (4 << 4))
        {
            q &= ~7U;
            sigZ &= ~(uint_fast64_t)0x7F;
            doubleTerm = q << 1;
            rem = ((rem - (uint_fast64_t)doubleTerm * (uint32_t)(sigB >> 32)) << 28)
                - (uint_fast64_t)doubleTerm * ((uint32_t)sigB >> 4);
            if ((rem & 0x8000000000000000) != 0)
                sigZ -= 1 << 7;
            else if (rem != 0)
                sigZ |= 1;
        }

        state ??= SoftFloatState.Default;
        return RoundPackToF64(state, signZ, expZ, sigZ);
    }

    // f64_rem
    public static Float64 Modulus(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        uint_fast64_t uiA, sigA, uiB, sigB, q64;
        int_fast16_t expA, expB, expDiff;
        uint64_t rem, altRem, meanRem;
        uint32_t q, recip32;
        bool signA, signRem;

        uiA = a._v;
        signA = SignF64UI(uiA);
        expA = ExpF64UI(uiA);
        sigA = FracF64UI(uiA);
        uiB = b._v;
        expB = ExpF64UI(uiB);
        sigB = FracF64UI(uiB);

        if (expA == 0x7FF)
        {
            state ??= SoftFloatState.Default;
            if (sigA != 0 || (expB == 0x7FF && sigB != 0))
                return Float64.FromBitsUI64(PropagateNaNF64UI(state, uiA, uiB));

            state.RaiseFlags(ExceptionFlags.Invalid);
            return Float64.FromBitsUI64(DefaultNaNF64UI);
        }

        if (expB == 0x7FF)
        {
            if (sigB != 0)
            {
                state ??= SoftFloatState.Default;
                return Float64.FromBitsUI64(PropagateNaNF64UI(state, uiA, uiB));
            }

            return a;
        }

        if (expA < expB - 1)
            return a;

        if (expB == 0)
        {
            if (sigB == 0)
            {
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
                return Float64.FromBitsUI64(DefaultNaNF64UI);
            }

            (expB, sigB) = NormSubnormalF64Sig(sigB);
        }

        if (expA == 0)
        {
            if (sigA == 0)
                return a;

            (expA, sigA) = NormSubnormalF64Sig(sigA);
        }

        rem = sigA | 0x0010000000000000;
        sigB |= 0x0010000000000000;
        expDiff = expA - expB;
        if (expDiff < 1)
        {
            if (expDiff < -1)
                return a;

            sigB <<= 9;
            if (expDiff != 0)
            {
                rem <<= 8;
                q = 0;
            }
            else
            {
                rem <<= 9;
                q = sigB <= rem ? 1U : 0;
                if (q != 0)
                    rem -= sigB;
            }
        }
        else
        {
            recip32 = ApproxRecip32_1((uint32_t)(sigB >> 21));

            // Changing the shift of 'rem' here requires also changing the initial subtraction from 'expDiff'.
            rem <<= 9;
            expDiff -= 30;

            // The scale of 'sigB' affects how many bits are obtained during each cycle of the loop. Currently this is 29 bits per loop
            // iteration, the maximum possible.
            sigB <<= 9;
            while (true)
            {
                q64 = (uint32_t)(rem >> 32) * (uint_fast64_t)recip32;
                if (expDiff < 0)
                    break;

                q = (uint32_t)((q64 + 0x80000000) >> 32);
                rem <<= 29;
                rem -= q * (uint64_t)sigB;
                if ((rem & 0x8000000000000000) != 0)
                    rem += sigB;

                expDiff -= 29;
            }

            // ('expDiff' cannot be less than -29 here.)
            q = (uint32_t)(q64 >> 32) >> (~expDiff & 31);
            rem = (rem << (expDiff + 30)) - q * (uint64_t)sigB;
            if ((rem & 0x8000000000000000) != 0)
            {
                altRem = rem + sigB;
                goto selectRem;
            }
        }

        do
        {
            altRem = rem;
            ++q;
            rem -= sigB;
        }
        while ((rem & 0x8000000000000000) == 0);

    selectRem:
        meanRem = rem + altRem;
        if ((meanRem & 0x8000000000000000) != 0 || (meanRem == 0 && (q & 1) != 0))
            rem = altRem;

        signRem = signA;
        if ((rem & 0x8000000000000000) != 0)
        {
            signRem = !signRem;
            rem = (uint64_t)(-(int64_t)rem);
        }

        state ??= SoftFloatState.Default;
        return NormRoundPackToF64(state, signRem, expB, rem);
    }

    // f64_sqrt
    public Float64 SquareRoot(SoftFloatState? state = null)
    {
        uint_fast64_t uiA, sigA, rem, sigZ, shiftedSigZ;
        int_fast16_t expA, expZ;
        uint32_t sig32A, recipSqrt32, sig32Z, q;
        bool signA;

        uiA = _v;
        signA = SignF64UI(uiA);
        expA = ExpF64UI(uiA);
        sigA = FracF64UI(uiA);

        if (expA == 0x7FF)
        {
            if (sigA != 0)
            {
                state ??= SoftFloatState.Default;
                return Float64.FromBitsUI64(PropagateNaNF64UI(state, uiA, 0));
            }

            if (!signA)
                return this;

            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return Float64.FromBitsUI64(DefaultNaNF64UI);
        }

        if (signA)
        {
            if (((uint_fast16_t)expA | sigA) == 0)
                return this;

            state ??= SoftFloatState.Default;
            state.RaiseFlags(ExceptionFlags.Invalid);
            return Float64.FromBitsUI64(DefaultNaNF64UI);
        }

        if (expA == 0)
        {
            if (sigA == 0)
                return this;

            (expA, sigA) = NormSubnormalF64Sig(sigA);
        }

        // ('sig32Z' is guaranteed to be a lower bound on the square root of 'sig32A', which makes 'sig32Z' also a lower bound on the
        // square root of 'sigA'.)
        expZ = ((expA - 0x3FF) >> 1) + 0x3FE;
        expA &= 1;
        sigA |= 0x0010000000000000;
        sig32A = (uint32_t)(sigA >> 21);
        recipSqrt32 = ApproxRecipSqrt32_1((uint32_t)expA, sig32A);
        sig32Z = (uint32_t)(((uint_fast64_t)sig32A * recipSqrt32) >> 32);
        if (expA != 0)
        {
            sigA <<= 8;
            sig32Z >>= 1;
        }
        else
        {
            sigA <<= 9;
        }

        rem = sigA - (uint_fast64_t)sig32Z * sig32Z;
        q = (uint32_t)(((uint32_t)(rem >> 2) * (uint_fast64_t)recipSqrt32) >> 32);
        sigZ = ((uint_fast64_t)sig32Z << 32 | 1 << 5) + ((uint_fast64_t)q << 3);

        if ((sigZ & 0x1FF) < 0x22)
        {
            sigZ &= ~(uint_fast64_t)0x3F;
            shiftedSigZ = sigZ >> 6;
            rem = (sigA << 52) - shiftedSigZ * shiftedSigZ;
            if ((rem & 0x8000000000000000) != 0)
                --sigZ;
            else if (rem != 0)
                sigZ |= 1;
        }

        state ??= SoftFloatState.Default;
        return RoundPackToF64(state, false, expZ, sigZ);
    }

    #endregion

    #region Comparison Operations

    // f64_eq (quiet=true) & f64_eq_signaling (quiet=false)
    public static bool CompareEqual(Float64 a, Float64 b, bool quiet, SoftFloatState? state = null)
    {
        uint_fast64_t uiA, uiB;

        uiA = a._v;
        uiB = b._v;

        if (IsNaNF64UI(uiA) || IsNaNF64UI(uiB))
        {
            if (!quiet || IsSigNaNF64UI(uiA) || IsSigNaNF64UI(uiB))
            {
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
            }

            return false;
        }

        return uiA == uiB || ((uiA | uiB) & 0x7FFFFFFFFFFFFFFF) == 0;
    }

    // f64_le (quiet=false) & f64_le_quiet (quiet=true)
    public static bool CompareLessThanOrEqual(Float64 a, Float64 b, bool quiet, SoftFloatState? state = null)
    {
        uint_fast64_t uiA, uiB;
        bool signA, signB;

        uiA = a._v;
        uiB = b._v;

        if (IsNaNF64UI(uiA) || IsNaNF64UI(uiB))
        {
            if (!quiet || IsSigNaNF64UI(uiA) || IsSigNaNF64UI(uiB))
            {
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
            }

            return false;
        }

        signA = SignF64UI(uiA);
        signB = SignF64UI(uiB);

        return (signA != signB)
            ? (signA || ((uiA | uiB) & 0x7FFFFFFFFFFFFFFF) == 0)
            : (uiA == uiB || (signA ^ (uiA < uiB)));
    }

    // f64_lt (quiet=false) & f64_lt_quiet (quiet=true)
    public static bool CompareLessThan(Float64 a, Float64 b, bool quiet, SoftFloatState? state = null)
    {
        uint_fast64_t uiA, uiB;
        bool signA, signB;

        uiA = a._v;
        uiB = b._v;

        if (IsNaNF64UI(uiA) || IsNaNF64UI(uiB))
        {
            if (!quiet || IsSigNaNF64UI(uiA) || IsSigNaNF64UI(uiB))
            {
                state ??= SoftFloatState.Default;
                state.RaiseFlags(ExceptionFlags.Invalid);
            }

            return false;
        }

        signA = SignF64UI(uiA);
        signB = SignF64UI(uiB);

        return (signA != signB)
            ? (signA && ((uiA | uiB) & 0x7FFFFFFFFFFFFFFF) != 0)
            : (uiA != uiB && (signA ^ (uiA < uiB)));
    }

    #endregion

    #endregion
}
