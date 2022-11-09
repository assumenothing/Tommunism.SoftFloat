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

[StructLayout(LayoutKind.Sequential, Pack = sizeof(ulong), Size = sizeof(ulong) * 2)]
public readonly struct Float128
{
    #region Fields

    // WARNING: DO NOT ADD OR CHANGE ANY OF THESE FIELDS!!!
    private readonly ulong _v0;
    private readonly ulong _v64;

    #endregion

    #region Constructors

    internal Float128(UInt128 v)
    {
        _v0 = v.V00;
        _v64 = v.V64;
    }

    private Float128(ulong v64, ulong v0)
    {
        _v0 = v0;
        _v64 = v64;
    }

    #endregion

    #region Properties

    // f128_isSignalingNaN
    public bool IsSignalingNaN => IsSigNaNF128UI(_v64, _v0);

    #endregion

    #region Methods

    public static Float128 FromUInt64x2Bits(ulong valueHi, ulong valueLo) => new(v64: valueHi, v0: valueLo);

    public (ulong hi, ulong lo) ToUInt64x2Bits() => (_v64, _v0);

    // TODO: Add support for .NET 7+ UInt128 bit conversions.

    // THIS IS THE INTERNAL CONSTRUCTOR FOR RAW BITS.
    internal static Float128 FromUI128(UInt128 v) => new(v);

    // THIS IS THE INTERNAL CONSTRUCTOR FOR RAW BITS.
    internal static Float128 FromUI128(ulong v64, ulong v0) => new(v64, v0);

    #region Integer-to-floating-point Conversions

    // NOTE: These operators use the default software floating-point state (which doesn't matter currently for this type).
    public static explicit operator Float128(uint32_t a) => FromUInt32(a);
    public static explicit operator Float128(uint64_t a) => FromUInt64(a);
    public static explicit operator Float128(int32_t a) => FromInt32(a);
    public static explicit operator Float128(int64_t a) => FromInt64(a);

    // ui32_to_f128
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static Float128 FromUInt32(uint32_t a, SoftFloatState? state = null)
    {
        var uiZ64 = 0UL;
        if (a != 0)
        {
            var shiftDist = CountLeadingZeroes32(a) + 17;
            uiZ64 = PackToF128UI64(false, 0x402E - shiftDist, (uint_fast64_t)a << shiftDist);
        }

        return FromUI128(v64: uiZ64, v0: 0);
    }

    // ui64_to_f128
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static Float128 FromUInt64(uint64_t a, SoftFloatState? state = null)
    {
        uint_fast64_t uiZ64, uiZ0;
        if (a == 0)
        {
            uiZ64 = 0;
            uiZ0 = 0;
        }
        else
        {
            var shiftDist = CountLeadingZeroes64(a) + 49;
            var zSig = (64 <= shiftDist)
                ? new UInt128(v64: a << (shiftDist - 64), v0: 0)
                : ShortShiftLeft128(0, a, shiftDist);
            uiZ64 = PackToF128UI64(false, 0x406E - shiftDist, zSig.V64);
            uiZ0 = zSig.V00;
        }

        return FromUI128(v64: uiZ64, v0: uiZ0);
    }

    // i32_to_f128
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static Float128 FromInt32(int32_t a, SoftFloatState? state = null)
    {
        var uiZ64 = 0UL;
        if (a != 0)
        {
            var sign = a < 0;
            var absA = (uint_fast32_t)(sign ? -a : a);
            var shiftDist = CountLeadingZeroes32(absA) + 17;
            uiZ64 = PackToF128UI64(sign, 0x402E - shiftDist, (uint_fast64_t)absA << shiftDist);
        }

        return FromUI128(v64: uiZ64, v0: 0);
    }

    // i64_to_f128
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static Float128 FromInt64(int64_t a, SoftFloatState? state = null)
    {
        uint_fast64_t uiZ64, uiZ0;
        if (a == 0)
        {
            uiZ64 = 0;
            uiZ0 = 0;
        }
        else
        {
            var sign = a < 0;
            var absA = (uint_fast64_t)(sign ? -a : a);
            var shiftDist = CountLeadingZeroes64(absA) + 49;
            var zSig = (64 <= shiftDist)
                ? new UInt128(v64: absA << (shiftDist - 64), v0: 0)
                : ShortShiftLeft128(0, absA, shiftDist);
            uiZ64 = PackToF128UI64(sign, 0x406E - shiftDist, zSig.V64);
            uiZ0 = zSig.V00;
        }

        return FromUI128(v64: uiZ64, v0: uiZ0);
    }

    #endregion

    #region Floating-point-to-integer Conversions

    public uint32_t ToUInt32(bool exact, SoftFloatState state) => ToUInt32(state.RoundingMode, exact, state);

    public uint64_t ToUInt64(bool exact, SoftFloatState state) => ToUInt64(state.RoundingMode, exact, state);

    public int32_t ToInt32(bool exact, SoftFloatState state) => ToInt32(state.RoundingMode, exact, state);

    public int64_t ToInt64(bool exact, SoftFloatState state) => ToInt64(state.RoundingMode, exact, state);

    // f128_to_ui32
    public uint32_t ToUInt32(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_to_ui64
    public uint64_t ToUInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_to_i32
    public int32_t ToInt32(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_to_i64
    public int64_t ToInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_to_ui32_r_minMag
    public uint32_t ToUInt32RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_to_ui64_r_minMag
    public uint64_t ToUInt64RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_to_i32_r_minMag
    public int32_t ToInt32RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_to_i64_r_minMag
    public int64_t ToInt64RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Floating-point-to-floating-point Conversions

    // f128_to_f16
    public static Float16 ToFloat16(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_to_f32
    public static Float32 ToFloat32(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_to_f64
    public static Float64 ToFloat64(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_to_extF80
    public static ExtFloat80 ToExtFloat80(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Arithmetic Operations

    // f128_roundToInt
    public Float128 RoundToInt(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_add
    public static Float128 Add(Float128 a, Float128 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_sub
    public static Float128 Subtract(Float128 a, Float128 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_mul
    public static Float128 Multiply(Float128 a, Float128 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_mulAdd
    public static Float128 MultiplyAndAdd(Float128 a, Float128 b, Float128 c, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_div
    public static Float128 Divide(Float128 a, Float128 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_rem
    public static Float128 Modulus(Float128 a, Float128 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_sqrt
    public Float128 SquareRoot(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Comparison Operations

    // f128_eq (quiet=true) & f128_eq_signaling (quiet=false)
    public static bool CompareEqual(Float128 a, Float128 b, bool quiet, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_le (quiet=false) & f128_le_quiet (quiet=true)
    public static bool CompareLessThanOrEqual(Float128 a, Float128 b, bool quiet, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f128_lt (quiet=false) & f128_lt_quiet (quiet=true)
    public static bool CompareLessThan(Float128 a, Float128 b, bool quiet, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #endregion
}
