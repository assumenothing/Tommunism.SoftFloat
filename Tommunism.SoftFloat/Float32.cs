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

[StructLayout(LayoutKind.Sequential, Pack = sizeof(uint), Size = sizeof(uint))]
public readonly struct Float32
{
    #region Fields

    // WARNING: DO NOT ADD OR CHANGE ANY OF THESE FIELDS!!!
    private readonly uint _v;

    #endregion

    #region Constructors

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used to avoid accidentally calling other overloads.")]
    private Float32(uint v, bool dummy)
    {
        _v = v;
    }

    public Float32(float value)
    {
        _v = BitConverter.SingleToUInt32Bits(value);
    }

    #endregion

    #region Properties

    // f32_isSignalingNaN
    public bool IsSignalingNaN => IsSigNaNF32UI(_v);

    #endregion

    #region Methods

    public static explicit operator Float32(float value) => new(value);
    public static implicit operator float(Float32 value) => BitConverter.UInt32BitsToSingle(value._v);

    public static Float32 FromUInt32Bits(ushort value) => FromBitsUI32(value);

    public uint ToUInt32Bits() => _v;

    // THIS IS THE INTERNAL CONSTRUCTOR FOR RAW BITS.
    internal static Float32 FromBitsUI32(uint v) => new(v, dummy: false);

    #region Integer-to-floating-point Conversions

    // NOTE: These operators use the default software floating-point state.
    public static explicit operator Float32(uint32_t a) => FromUInt32(a);
    public static explicit operator Float32(uint64_t a) => FromUInt64(a);
    public static explicit operator Float32(int32_t a) => FromInt32(a);
    public static explicit operator Float32(int64_t a) => FromInt64(a);

    // ui32_to_f32
    public static Float32 FromUInt32(uint32_t a, SoftFloatState? state = null)
    {
        if (a == 0)
            return FromBitsUI32(0);

        state ??= SoftFloatState.Default;
        return (a & 0x80000000) != 0
            ? RoundPackToF32(state, false, 0x9D, (a >> 1) | (a & 1))
            : NormRoundPackToF32(state, false, 0x9C, a);
    }

    // ui64_to_f32
    public static Float32 FromUInt64(uint64_t a, SoftFloatState? state = null)
    {
        var shiftDist = CountLeadingZeroes64(a) - 40;
        if (0 <= shiftDist)
            return FromBitsUI32(a != 0 ? PackToF32UI(false, 0x95 - shiftDist, (uint_fast32_t)a << shiftDist) : 0U);

        shiftDist += 7;
        var sig = (shiftDist < 0)
            ? (uint_fast32_t)ShortShiftRightJam64(a, -shiftDist)
            : ((uint_fast32_t)a << shiftDist);
        return RoundPackToF32(state ?? SoftFloatState.Default, false, 0x9C - shiftDist, sig);
    }

    // i32_to_f32
    public static Float32 FromInt32(int32_t a, SoftFloatState? state = null)
    {
        var sign = a < 0;
        if ((a & 0x7FFFFFFF) == 0)
            return FromBitsUI32(sign ? PackToF32UI(true, 0x9E, 0U) : 0U);

        var absA = (uint_fast32_t)(sign ? -a : a);
        return NormRoundPackToF32(state ?? SoftFloatState.Default, sign, 0x9C, absA);
    }

    // i64_to_f32
    public static Float32 FromInt64(int64_t a, SoftFloatState? state = null)
    {
        var sign = a < 0;
        var absA = (uint_fast64_t)(sign ? -a : a);
        var shiftDist = CountLeadingZeroes64(absA) - 40;
        if (0 <= shiftDist)
            return FromBitsUI32(a != 0 ? PackToF32UI(sign, 0x95 - shiftDist, (uint_fast32_t)absA << shiftDist) : 0U);

        shiftDist += 7;
        var sig = (shiftDist < 0)
            ? (uint_fast32_t)ShortShiftRightJam64(absA, -shiftDist)
            : ((uint_fast32_t)absA << shiftDist);
        return RoundPackToF32(state ?? SoftFloatState.Default, sign, 0x9C - shiftDist, sig);
    }

    #endregion

    #region Floating-point-to-integer Conversions

    public uint32_t ToUInt32(bool exact, SoftFloatState state) => ToUInt32(state.RoundingMode, exact, state);

    public uint64_t ToUInt64(bool exact, SoftFloatState state) => ToUInt64(state.RoundingMode, exact, state);

    public int32_t ToInt32(bool exact, SoftFloatState state) => ToInt32(state.RoundingMode, exact, state);

    public int64_t ToInt64(bool exact, SoftFloatState state) => ToInt64(state.RoundingMode, exact, state);

    // f32_to_ui32
    public uint32_t ToUInt32(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_to_ui64
    public uint64_t ToUInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_to_i32
    public int32_t ToInt32(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_to_i64
    public int64_t ToInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_to_ui32_r_minMag
    public uint32_t ToUInt32RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_to_ui64_r_minMag
    public uint64_t ToUInt64RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_to_i32_r_minMag
    public int32_t ToInt32RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_to_i64_r_minMag
    public int64_t ToInt64RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Floating-point-to-floating-point Conversions

    // f32_to_f16
    public static Float16 ToFloat16(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_to_f64
    public static Float64 ToFloat64(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_to_extF80
    public static ExtFloat80 ToExtFloat80(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_to_f128
    public static Float128 ToFloat128(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Arithmetic Operations

    // f32_roundToInt
    public Float32 RoundToInt(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_add
    public static Float32 Add(Float32 a, Float32 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_sub
    public static Float32 Subtract(Float32 a, Float32 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_mul
    public static Float32 Multiply(Float32 a, Float32 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_mulAdd
    public static Float32 MultiplyAndAdd(Float32 a, Float32 b, Float32 c, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_div
    public static Float32 Divide(Float32 a, Float32 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_rem
    public static Float32 Modulus(Float32 a, Float32 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_sqrt
    public Float32 SquareRoot(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Comparison Operations

    // f32_eq (quiet=true) & f32_eq_signaling (quiet=false)
    public static bool CompareEqual(Float32 a, Float32 b, bool quiet, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_le (quiet=false) & f32_le_quiet (quiet=true)
    public static bool CompareLessThanOrEqual(Float32 a, Float32 b, bool quiet, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f32_lt (quiet=false) & f32_lt_quiet (quiet=true)
    public static bool CompareLessThan(Float32 a, Float32 b, bool quiet, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #endregion
}
