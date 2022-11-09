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

    #region Properties

    // f16_isSignalingNaN
    public bool IsSignalingNaN => IsSigNaNF16UI(_v);

    #endregion

    #region Methods

    public static explicit operator Float16(Half value) => new(value);
    public static implicit operator Half(Float16 value) => BitConverter.UInt16BitsToHalf(value._v);

    public static Float16 FromUInt16Bits(ushort value) => FromBitsUI16(value);

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
        throw new NotImplementedException();
    }

    // f16_to_ui64
    public uint64_t ToUInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_to_i32
    public int32_t ToInt32(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_to_i64
    public int64_t ToInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_to_ui32_r_minMag
    public uint32_t ToUInt32RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_to_ui64_r_minMag
    public uint64_t ToUInt64RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_to_i32_r_minMag
    public int32_t ToInt32RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_to_i64_r_minMag
    public int64_t ToInt64RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Floating-point-to-floating-point Conversions

    // f16_to_f32
    public static Float32 ToFloat32(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_to_f64
    public static Float64 ToFloat64(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_to_extF80
    public static ExtFloat80 ToExtFloat80(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_to_f128
    public static Float128 ToFloat128(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Arithmetic Operations

    // f16_roundToInt
    public Float16 RoundToInt(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_add
    public static Float16 Add(Float16 a, Float16 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_sub
    public static Float16 Subtract(Float16 a, Float16 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_mul
    public static Float16 Multiply(Float16 a, Float16 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_mulAdd
    public static Float16 MultiplyAndAdd(Float16 a, Float16 b, Float16 c, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_div
    public static Float16 Divide(Float16 a, Float16 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_rem
    public static Float16 Modulus(Float16 a, Float16 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_sqrt
    public Float16 SquareRoot(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Comparison Operations

    // f16_eq (quiet=true) & f16_eq_signaling (quiet=false)
    public static bool CompareEqual(Float16 a, Float16 b, bool quiet, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_le (quiet=false) & f16_le_quiet (quiet=true)
    public static bool CompareLessThanOrEqual(Float16 a, Float16 b, bool quiet, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f16_lt (quiet=false) & f16_lt_quiet (quiet=true)
    public static bool CompareLessThan(Float16 a, Float16 b, bool quiet, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #endregion
}
